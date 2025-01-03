// Copyright 2015 Destructurama Contributors
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

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.SystemTextJson;

internal sealed class SystemTextJsonDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        if (value is JsonDocument jdoc)
        {
            result = Destructure(jdoc.RootElement, propertyValueFactory);
            return true;
        }
        else if (value is JsonElement element)
        {
            result = Destructure(in element, propertyValueFactory);
            return true;
        }

        result = null;
        return false;
    }

    private static readonly ScalarValue _false = new(false);
    private static readonly ScalarValue _true = new(true);

    private static LogEventPropertyValue Destructure(in JsonElement element, ILogEventPropertyValueFactory propertyValueFactory)
    {
        return element.ValueKind switch
        {
            JsonValueKind.False => _false,
            JsonValueKind.True => _true,
            JsonValueKind.Null or JsonValueKind.Undefined => ScalarValue.Null,
            JsonValueKind.Number => propertyValueFactory.CreatePropertyValue(element.GetDecimal(), destructureObjects: true),
            JsonValueKind.String => propertyValueFactory.CreatePropertyValue(element.GetString(), destructureObjects: true),
            JsonValueKind.Array => new SequenceValue(element.EnumerateArray().Select(arrElement => Destructure(in arrElement, propertyValueFactory))),
            JsonValueKind.Object => DestructureObject(element, propertyValueFactory),
            _ => throw new ArgumentException($"Unrecognized value kind {element.ValueKind}.", nameof(element)),
        };
    }

    private static LogEventPropertyValue DestructureObject(JsonElement element, ILogEventPropertyValueFactory propertyValueFactory)
    {
        string? typeTag = null;
        var props = new List<LogEventProperty>(element.GetPropertyCount());

        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Name == "$type")
            {
                if (prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() is string v)
                {
                    typeTag = v;
                    continue;
                }
            }
            else if (!LogEventProperty.IsValidName(prop.Name))
            {
                return DestructureToDictionaryValue(element, propertyValueFactory);
            }

            props.Add(new LogEventProperty(prop.Name, Destructure(prop.Value, propertyValueFactory)));
        }

        return new StructureValue(props, typeTag);
    }

    private static LogEventPropertyValue DestructureToDictionaryValue(JsonElement element, ILogEventPropertyValueFactory propertyValueFactory)
    {
        var elements = element.EnumerateObject().Select(
            prop => new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                    new ScalarValue(prop.Name),
                    Destructure(prop.Value, propertyValueFactory))
        );
        return new DictionaryValue(elements);
    }
}
