---
title: Home
layout: simple
og_type: website
---

<section class="text-center py-5">
  <div class="container">
    <img src="{{site.basepath}}/img/scriban.svg" alt="Scriban — Fast, powerful text templating for .NET" class="img-fluid" style="width: min(100%, 12rem); height: auto;">
    <h1 class="display-4 mt-4">Scriban</h1>
    <p class="lead mt-3 mb-4">
      A fast, powerful, safe and lightweight <strong>scripting language</strong> and <strong>text templating engine</strong> for .NET.
    </p>
    <div class="d-flex justify-content-center gap-3 mt-4 flex-wrap">
      <a href="{{site.basepath}}/docs/getting-started/" class="btn btn-primary btn-lg"><i class="bi bi-rocket-takeoff"></i> Get started</a>
      <a href="{{site.basepath}}/docs/" class="btn btn-outline-secondary btn-lg"><i class="bi bi-book"></i> Documentation</a>
      <a href="https://github.com/scriban/scriban" class="btn btn-info btn-lg"><i class="bi bi-github"></i> GitHub</a>
    </div>
    <div class="mt-4 text-start mx-auto" style="max-width: 48rem;">
      <pre class="language-shell-session"><code>dotnet add package Scriban</code></pre>
      <p class="text-center text-secondary mt-2" style="font-size: 0.85rem;">Available on <a href="https://www.nuget.org/packages/Scriban/" class="text-secondary">NuGet</a> — .NET Standard 2.0+</p>
    </div>
  </div>
</section>

<!-- Playground section -->
<section id="playground" class="container my-5" data-api-url="{{site.playground_api_url}}">
  <div class="card">
    <div class="card-header display-6">
      <i class="bi bi-play-circle lunet-feature-icon lunet-icon--controls"></i> Playground
    </div>
    <div class="card-body">
      <p class="card-text mb-3">
        Try Scriban live! Edit the template or data and click <strong>Run</strong> (or press <kbd>Ctrl</kbd>+<kbd>Enter</kbd>) to see the output.
      </p>
      <div class="row gx-3 gy-3">
        <div class="col-md-6">
          <label for="playground-data" class="form-label fw-bold"><i class="bi bi-braces"></i> Model / Data (JSON)</label>
          <textarea id="playground-data" class="form-control font-monospace" rows="6" spellcheck="false">{
  "name": "World",
  "items": ["Apple", "Banana", "Cherry"]
}</textarea>
        </div>
        <div class="col-md-6">
          <label for="playground-template" class="form-label fw-bold"><i class="bi bi-file-code"></i> Template (Scriban)</label>
          <textarea id="playground-template" class="form-control font-monospace" rows="6" spellcheck="false">Hello {{ "{{" }} name {{ "}}" }}!
&lt;ul>
{{ "{{~" }} for item in items {{ "~}}" }}
  &lt;li>{{ "{{" }} item | string.upcase {{ "}}" }}&lt;/li>
{{ "{{~" }} end {{ "~}}" }}
&lt;/ul></textarea>
        </div>
      </div>
      <div class="mt-3 d-flex align-items-center gap-2">
        <button id="playground-run" class="btn btn-primary" disabled>
          <i class="bi bi-play-fill"></i> Run
        </button>
        <span id="playground-status" class="text-secondary small"><i class="bi bi-hourglass-split"></i> Checking service availability…</span>
      </div>
      <div class="mt-3">
        <label for="playground-output" class="form-label fw-bold"><i class="bi bi-terminal"></i> Output</label>
        <pre id="playground-output" class="border rounded p-3 bg-body-tertiary" style="min-height: 4rem; white-space: pre-wrap;"><code>Hello World!
&lt;ul>
  &lt;li>APPLE&lt;/li>
  &lt;li>BANANA&lt;/li>
  &lt;li>CHERRY&lt;/li>
&lt;/ul>
</code></pre>
      </div>
    </div>
  </div>
</section>

<!-- Feature cards -->
<section class="container my-5">
  <div class="row row-cols-1 row-cols-lg-2 gx-5 gy-4">
    <div class="col">
      <div class="card h-100">
        <div class="card-header display-6"><i class="bi bi-lightning-charge lunet-feature-icon lunet-icon--controls"></i> Fast &amp; lightweight</div>
        <div class="card-body">
          <p class="card-text">
            Scriban uses a custom Lexer/Parser with a full AST for blazing-fast parsing and low-allocation rendering. CPU and GC-friendly by design.
          </p>

[Language reference](docs/language/readme.md) · [Built-in functions](docs/builtins/readme.md)

</div>
      </div>
    </div>
    <div class="col">
      <div class="card h-100">
        <div class="card-header display-6"><i class="bi bi-shield-check lunet-feature-icon lunet-icon--themes"></i> Safe sandbox</div>
        <div class="card-body">
          <p class="card-text">
            By default, no .NET objects are exposed unless explicitly allowed. You control exactly what is available to templates — perfect for user-facing scenarios.
          </p>

[Runtime &amp; security](docs/runtime/readme.md) · [Safe runtime](docs/runtime/readme.md#safe-runtime)

</div>
      </div>
    </div>
    <div class="col">
      <div class="card h-100">
        <div class="card-header display-6"><i class="bi bi-water lunet-feature-icon lunet-icon--editing"></i> Liquid compatible</div>
        <div class="card-body">
          <p class="card-text">
            Parse and render Liquid templates with <code>Template.ParseLiquid</code>. Migrate from Liquid to Scriban incrementally, or use both side by side.
          </p>

[Liquid support](docs/liquid-support.md)

</div>
      </div>
    </div>
    <div class="col">
      <div class="card h-100">
        <div class="card-header display-6"><i class="bi bi-puzzle lunet-feature-icon lunet-icon--data"></i> Extensible runtime</div>
        <div class="card-body">
          <p class="card-text">
            Import .NET classes, delegates,  or custom <code>IScriptCustomFunction</code> implementations. Use member renamers, filters, and template loaders to tailor the engine.
          </p>

[.NET runtime API](docs/runtime/readme.md) · [Custom functions](docs/runtime/readme.md#advanced-usages)

</div>
      </div>
    </div>
    <div class="col">
      <div class="card h-100">
        <div class="card-header display-6"><i class="bi bi-arrow-repeat lunet-feature-icon lunet-icon--chrome"></i> Async support</div>
        <div class="card-body">
          <p class="card-text">
            Full <code>async</code>/<code>await</code> support with <code>Template.RenderAsync</code>. Evaluate templates without blocking — ideal for web servers and cloud functions.
          </p>

[Getting started](docs/getting-started.md) · [Runtime API](docs/runtime/readme.md)

</div>
      </div>
    </div>
    <div class="col">
      <div class="card h-100">
        <div class="card-header display-6"><i class="bi bi-filetype-cs lunet-feature-icon lunet-icon--binding"></i> Rich language</div>
        <div class="card-body">
          <p class="card-text">
            Variables, expressions, conditionals, loops, functions, pipes, objects, arrays — all the constructs you need for powerful templating and scripting scenarios.
          </p>

[Language reference](docs/language/readme.md) · [Examples](docs/getting-started.md)

</div>
      </div>
    </div>
  </div>
</section>

<section class="container my-5">
  <div class="card">
    <div class="card-header display-6">
      <i class="bi bi-code-slash lunet-feature-icon lunet-icon--lists"></i> Quick example
    </div>
    <div class="card-body">

```csharp
using Scriban;

// Parse a template
var template = Template.Parse("Hello {{ "{{" }} name {{ "}}" }}! You have {{ "{{" }} count {{ "}}" }} messages.");

// Render with a model
var result = template.Render(new { Name = "Alice", Count = 5 });
// => "Hello Alice! You have 5 messages."
```

For more examples, see the [Getting started](docs/getting-started.md) guide.

</div>
  </div>
</section>
<script>
(function () {
  "use strict";
  var section = document.getElementById("playground");
  if (!section) return;
  var apiUrl = (section.getAttribute("data-api-url") || "").replace(/\/+$/, "");
  var btnRun = document.getElementById("playground-run");
  var status = document.getElementById("playground-status");
  var tmplEl = document.getElementById("playground-template");
  var dataEl = document.getElementById("playground-data");
  var outEl  = document.getElementById("playground-output");
  //
  function setStatus(html, cls) {
    status.className = "small " + (cls || "text-secondary");
    status.innerHTML = html;
  }
  //
  function enableRun() {
    btnRun.disabled = false;
    btnRun.classList.remove("btn-secondary");
    btnRun.classList.add("btn-primary");
  }
  //
  function disableRun() {
    btnRun.disabled = true;
    btnRun.classList.remove("btn-primary");
    btnRun.classList.add("btn-secondary");
  }
  //
  // Health check on load
  if (!apiUrl) {
    setStatus('<i class="bi bi-exclamation-triangle"></i> Playground API URL not configured.', "text-warning");
    return;
  }
  //
  // Read template/model from URL query parameters (e.g. ?template=...&model={})
  var params = new URLSearchParams(window.location.search);
  var urlTemplate = params.get("template");
  var urlModel = params.get("model");
  var hasUrlParams = urlTemplate !== null;
  if (hasUrlParams) {
    tmplEl.value = urlTemplate;
    if (urlModel !== null) dataEl.value = urlModel;
    section.scrollIntoView({ behavior: "smooth" });
    // Clean URL without reloading
    if (window.history.replaceState) {
      window.history.replaceState(null, "", window.location.pathname);
    }
  }
  //
  fetch(apiUrl + "/api/health", { method: "GET", mode: "cors" })
    .then(function (r) {
      if (!r.ok) throw new Error("HTTP " + r.status);
      return r.json();
    })
    .then(function () {
      setStatus('<i class="bi bi-check-circle"></i> Service available', "text-success");
      enableRun();
      // Auto-run if template was provided via URL
      if (hasUrlParams) runTemplate();
    })
    .catch(function () {
      setStatus('<i class="bi bi-x-circle"></i> Service unavailable \u2014 try again later.', "text-danger");
    });
  //
  // Run handler
  function runTemplate() {
    if (btnRun.disabled) return;
    disableRun();
    setStatus('<i class="bi bi-hourglass-split"></i> Rendering\u2026', "text-info");
    outEl.textContent = "";
    //
    var body = JSON.stringify({
      template: tmplEl.value,
      model: dataEl.value || null
    });
    //
    fetch(apiUrl + "/api/render", {
      method: "POST",
      mode: "cors",
      headers: { "Content-Type": "application/json" },
      body: body
    })
      .then(function (r) { return r.json().then(function (d) { return { ok: r.ok, data: d }; }); })
      .then(function (res) {
        if (res.ok) {
          outEl.textContent = res.data.result;
          setStatus('<i class="bi bi-check-circle"></i> Done', "text-success");
        } else {
          outEl.textContent = res.data.error || "Unknown error";
          setStatus('<i class="bi bi-exclamation-triangle"></i> Error', "text-danger");
        }
        enableRun();
      })
      .catch(function (err) {
        outEl.textContent = "Network error: " + err.message;
        setStatus('<i class="bi bi-x-circle"></i> Request failed', "text-danger");
        enableRun();
      });
  }
  //
  btnRun.addEventListener("click", runTemplate);
  //
  document.addEventListener("keydown", function (e) {
    if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
      e.preventDefault();
      runTemplate();
    }
  });
})();
</script>