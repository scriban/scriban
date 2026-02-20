using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------
const int MaxTemplateSize = 1 * 1024;  // 1 KB
const int MaxModelSize = 1 * 1024;    // 1 KB
const int RenderTimeoutMs = 2_000;     // 2 seconds

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddCors();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware
// ---------------------------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["https://scriban.github.io"];

app.UseCors(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .WithMethods("POST", "GET", "OPTIONS"));

app.UseRateLimiter();

// ---------------------------------------------------------------------------
// Health / readiness endpoint – used by the playground to detect availability.
// ---------------------------------------------------------------------------
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health");

// ---------------------------------------------------------------------------
// Render endpoint
// ---------------------------------------------------------------------------
app.MapPost("/api/render", async (RenderRequest request) =>
{
    // --- Input validation ---------------------------------------------------
    if (string.IsNullOrWhiteSpace(request.Template))
    {
        return Results.BadRequest(new RenderError("Template must not be empty."));
    }

    if (request.Template.Length > MaxTemplateSize)
    {
        return Results.BadRequest(new RenderError($"Template exceeds the maximum allowed size of {MaxTemplateSize / 1024} KB."));
    }

    if (request.Model is not null && request.Model.Length > MaxModelSize)
    {
        return Results.BadRequest(new RenderError($"Model exceeds the maximum allowed size of {MaxModelSize / 1024} KB."));
    }

    // --- Parse the template -------------------------------------------------
    Template template;
    try
    {
        template = Template.Parse(request.Template);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new RenderError($"Template parse error: {ex.Message}"));
    }

    if (template.HasErrors)
    {
        var messages = string.Join("\n", template.Messages);
        return Results.BadRequest(new RenderError($"Template parse error:\n{messages}"));
    }

    // --- Build the model ----------------------------------------------------
    ScriptObject model;
    try
    {
        model = BuildModel(request.Model);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new RenderError($"Model error: {ex.Message}"));
    }

    // --- Render with timeout ------------------------------------------------
    var context = new TemplateContext
    {
        LoopLimit = 1_000,
        RecursiveLimit = 50,
        StrictVariables = false
    };
    context.PushGlobal(model);

    using var cts = new CancellationTokenSource(RenderTimeoutMs);

    try
    {
        var result = await Task.Run(() => template.Render(context), cts.Token);
        return Results.Ok(new RenderResponse(result));
    }
    catch (OperationCanceledException)
    {
        return Results.Json(
            new RenderError("Render timed out. Simplify your template or reduce loop iterations."),
            statusCode: StatusCodes.Status408RequestTimeout);
    }
    catch (ScriptRuntimeException ex)
    {
        return Results.BadRequest(new RenderError($"Runtime error: {ex.OriginalMessage}"));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new RenderError($"Render error: {ex.Message}"));
    }
})
.WithName("Render");

app.Run();

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
static ScriptObject BuildModel(string? json)
{
    var scriptObject = new ScriptObject();
    if (string.IsNullOrWhiteSpace(json))
    {
        return scriptObject;
    }

    using var doc = JsonDocument.Parse(json);
    if (doc.RootElement.ValueKind != JsonValueKind.Object)
    {
        throw new ArgumentException("Model must be a JSON object.");
    }

    foreach (var property in doc.RootElement.EnumerateObject())
    {
        scriptObject[property.Name] = ConvertJsonElement(property.Value);
    }

    return scriptObject;
}

static object? ConvertJsonElement(JsonElement element) => element.ValueKind switch
{
    JsonValueKind.String => element.GetString(),
    JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
    JsonValueKind.True => true,
    JsonValueKind.False => false,
    JsonValueKind.Null => null,
    JsonValueKind.Array => new ScriptArray(element.EnumerateArray().Select(ConvertJsonElement)),
    JsonValueKind.Object => BuildNestedObject(element),
    _ => element.ToString()
};

static ScriptObject BuildNestedObject(JsonElement element)
{
    var obj = new ScriptObject();
    foreach (var property in element.EnumerateObject())
    {
        obj[property.Name] = ConvertJsonElement(property.Value);
    }
    return obj;
}

// ---------------------------------------------------------------------------
// Request / Response records
// ---------------------------------------------------------------------------
record RenderRequest(string Template, string? Model);
record RenderResponse(string Result);
record RenderError(string Error);
