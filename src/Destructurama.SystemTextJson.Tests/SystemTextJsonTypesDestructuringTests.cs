// Copyright 2017 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text.Json;
using Destructurama.SystemTextJson.Tests.Support;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Shouldly;

namespace Destructurama.SystemTextJson.Tests;

internal class HasName
{
    public string? Name { get; set; }
}

public class SystemTextJsonTypesDestructuringTests
{
    [Fact]
    public void AttributesAreConsultedWhenDestructuring()
    {
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.SystemTextJsonTypes()
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        var test = new
        {
            HN = new HasName { Name = "Some name" },
            Arr = new[] { 1, 2, 3 },
            S = "Some string",
            D = new Dictionary<int, string> { { 1, "One" }, { 2, "Two" } },
            E = (object?)null,
            ESPN = JsonSerializer.Deserialize<dynamic>("{\"\":\"Empty string property name\"}"),
            WSPN = JsonSerializer.Deserialize<dynamic>("{\"  \":\"Whitespace property name\"}")
        };

        string ser = JsonSerializer.Serialize(test);
        var dyn = JsonSerializer.Deserialize<dynamic>(ser);

        log.Information("Here is {@Dyn}", dyn);

        var sv = (StructureValue)evt.Properties["Dyn"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props["HN"].ShouldBeOfType<StructureValue>();
        props["Arr"].ShouldBeOfType<SequenceValue>();
        props["S"].LiteralValue().ShouldBeOfType<string>();
        props["D"].ShouldBeOfType<StructureValue>();
        props["E"].LiteralValue().ShouldBeNull();
        props["ESPN"].ShouldBeOfType<DictionaryValue>();
        props["WSPN"].ShouldBeOfType<DictionaryValue>();

        foreach (var value in props.Values.OfType<StructureValue>())
        {
            value.TypeTag.ShouldBeNull();
        }
    }

    [Fact]
    public void TryDestructure_Should_Return_False_When_Called_With_Null()
    {
        var policy = new SystemTextJsonDestructuringPolicy();
        policy.TryDestructure(null!, null!, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryDestructure_Should_Handle_TypeToken_As_Ordinal_Property_When_Its_Value_Not_String()
    {
        var policy = new SystemTextJsonDestructuringPolicy();
        var o = JsonDocument.Parse("{ \"$type\": 42 }");
        policy.TryDestructure(o, new StubFactory(), out var value).ShouldBeTrue();
        var sv = value.ShouldBeOfType<StructureValue>();
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("$type");
        sv.Properties[0].Value.LiteralValue().ShouldBe(42);
    }

    [Fact]
    public void TryDestructure_Should_Handle_TypeToken_As_Ordinal_Property_When_Its_Not_JValue()
    {
        var policy = new SystemTextJsonDestructuringPolicy();
        var o = JsonDocument.Parse("{ \"$type\": [1,2,3]}");
        policy.TryDestructure(o, new StubFactory(), out var value).ShouldBeTrue();
        var sv = value.ShouldBeOfType<StructureValue>();
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("$type");
        var seq = sv.Properties[0].Value.ShouldBeOfType<SequenceValue>();
        seq.Elements.Count.ShouldBe(3);
    }

    private sealed class StubFactory : ILogEventPropertyValueFactory
    {
        public LogEventPropertyValue CreatePropertyValue(object? value, bool destructureObjects = false)
        {
            if (value is decimal i && (i == 42 || i == 1 || i == 2 || i == 3))
                return new ScalarValue(i);

            throw new NotImplementedException();
        }
    }
}
