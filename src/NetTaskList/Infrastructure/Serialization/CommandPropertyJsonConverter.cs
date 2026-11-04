using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetTaskList.Infrastructure.Serialization;

public sealed class CommandPropertyJsonConverter : JsonConverter<List<string>>
{
    public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var singleCommand = reader.GetString();
            return singleCommand is null ? null : new List<string> { singleCommand };
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var commands = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return commands;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var cmd = reader.GetString();
                    commands.Add(cmd ?? string.Empty);
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    commands.Add(string.Empty);
                }
                else
                {
                    throw new JsonException($"Elemento inválido no array de comandos: esperado texto, obtido '{reader.TokenType}'.");
                }
            }

            throw new JsonException("Array de comandos JSON não foi fechado corretamente.");
        }

        throw new JsonException($"Token JSON inesperado para a propriedade 'command': '{reader.TokenType}'. Esperado texto ou array de textos.");
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Count == 1)
        {
            writer.WriteStringValue(value[0]);
            return;
        }

        writer.WriteStartArray();
        foreach (var cmd in value)
        {
            writer.WriteStringValue(cmd);
        }
        writer.WriteEndArray();
    }
}

