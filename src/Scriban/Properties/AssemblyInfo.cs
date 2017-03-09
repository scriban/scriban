using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Scriban")]
[assembly: AssemblyDescription("A fast and powerful text templating language and engine for .NET")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Scriban")]
[assembly: AssemblyCopyright("Copyright © 2016 Alexandre Mutel")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.3.0")]
[assembly: AssemblyFileVersion("0.3.0")]

[assembly: InternalsVisibleTo("Scriban.Tests")]

