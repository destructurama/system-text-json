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
