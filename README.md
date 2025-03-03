# Destructurama.SystemTextJson

![License](https://img.shields.io/github/license/destructurama/system-text-json)

[![codecov](https://codecov.io/gh/destructurama/system-text-json/graph/badge.svg?token=abGh9D57gU)](https://codecov.io/gh/destructurama/system-text-json)
[![Nuget](https://img.shields.io/nuget/dt/Destructurama.SystemTextJson)](https://www.nuget.org/packages/Destructurama.SystemTextJson)
[![Nuget](https://img.shields.io/nuget/v/Destructurama.SystemTextJson)](https://www.nuget.org/packages/Destructurama.SystemTextJson)

[![GitHub Release Date](https://img.shields.io/github/release-date/destructurama/system-text-json?label=released)](https://github.com/destructurama/system-text-json/releases)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/destructurama/system-text-json/latest?label=new+commits)](https://github.com/destructurama/system-text-json/commits/master)
![Size](https://img.shields.io/github/repo-size/destructurama/system-text-json)

[![GitHub contributors](https://img.shields.io/github/contributors/destructurama/system-text-json)](https://github.com/destructurama/system-text-json/graphs/contributors)
![Activity](https://img.shields.io/github/commit-activity/w/destructurama/system-text-json)
![Activity](https://img.shields.io/github/commit-activity/m/destructurama/system-text-json)
![Activity](https://img.shields.io/github/commit-activity/y/destructurama/system-text-json)

[![Run unit tests](https://github.com/destructurama/system-text-json/actions/workflows/test.yml/badge.svg)](https://github.com/destructurama/system-text-json/actions/workflows/test.yml)
[![Publish preview to GitHub registry](https://github.com/destructurama/system-text-json/actions/workflows/publish-preview.yml/badge.svg)](https://github.com/destructurama/system-text-json/actions/workflows/publish-preview.yml)
[![Publish release to Nuget registry](https://github.com/destructurama/system-text-json/actions/workflows/publish-release.yml/badge.svg)](https://github.com/destructurama/system-text-json/actions/workflows/publish-release.yml)
[![CodeQL analysis](https://github.com/destructurama/system-text-json/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/destructurama/system-text-json/actions/workflows/codeql-analysis.yml)

Adds support for logging System.Text.Json dynamic types as structured data with [Serilog](https://serilog.net).
For JSON.NET dynamic types see [this repo](https://github.com/destructurama/json-net).

# Installation

Install from NuGet:

```powershell
Install-Package Destructurama.SystemTextJson
```

# Usage

Modify logger configuration:

```csharp
var log = new LoggerConfiguration().Destructure.SystemTextJsonTypes()
```

Now any System.Text.Json dynamic object can be represented in the log event's properties:

```csharp
using System.Text.Json;
using Destructurama;
using Serilog;

var logger1 = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var logger2 = new LoggerConfiguration().Destructure.SystemTextJsonTypes().WriteTo.Console().CreateLogger();

var json = """
    {
      "name": "Tom",
      "age": 42,
      "isDeveloper": true
    }
    """;

var obj = JsonSerializer.Deserialize<dynamic>(json);

logger1.Information("Deserialized without SystemTextJsonTypes(): {@Obj}", obj);

logger2.Information("Deserialized with SystemTextJsonTypes(): {@Obj}", obj);

Console.ReadKey();
```

Output:

```
[21:15:52 INF] Deserialized without SystemTextJsonTypes(): {"ValueKind": "Object", "$type": "JsonElement"}
[21:15:53 INF] Deserialized with SystemTextJsonTypes(): {"name": "Tom", "age": 42, "isDeveloper": true}
```

# Benchmarks

The results are available [here](https://destructurama.github.io/system-text-json/dev/bench/).
