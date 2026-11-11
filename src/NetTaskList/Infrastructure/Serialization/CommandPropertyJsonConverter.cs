using System.Text.Json;
using System.Text.Json.Serialization;
using NetTaskList.Infrastructure.Dtos;

namespace NetTaskList.Infrastructure.Serialization;

public sealed class CommandPropertyJsonConverter : JsonConverter<List<TaskCommandDto>>
{
    public override List<TaskCommandDto>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var singleCommand = reader.GetString();
            return singleCommand is null ? null : new List<TaskCommandDto> { new TaskCommandDto { RawCommand = singleCommand } };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var shortcut = ReadTaskShortcutObject(ref reader);
            return new List<TaskCommandDto> { shortcut };
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var commands = new List<TaskCommandDto>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return commands;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var cmd = reader.GetString();
                    commands.Add(new TaskCommandDto { RawCommand = cmd ?? string.Empty });
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var shortcut = ReadTaskShortcutObject(ref reader);
                    commands.Add(shortcut);
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    commands.Add(new TaskCommandDto { RawCommand = string.Empty });
                }
                else
                {
                    throw new JsonException($"Elemento inválido no array de comandos: esperado texto ou objeto de atalho, obtido '{reader.TokenType}'.");
                }
            }

            throw new JsonException("Array de comandos JSON não foi fechado corretamente.");
        }

        throw new JsonException($"Token JSON inesperado para a propriedade 'command': '{reader.TokenType}'. Esperado texto, objeto com atalho de tarefa ou array.");
    }

    private static TaskCommandDto ReadTaskShortcutObject(ref Utf8JsonReader reader)
    {
        string? targetTask = null;
        bool hasTaskProp = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propName = reader.GetString();
                reader.Read();

                if (string.Equals(propName, "task", StringComparison.OrdinalIgnoreCase))
                {
                    hasTaskProp = true;
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        targetTask = reader.GetString();
                    }
                    else if (reader.TokenType == JsonTokenType.Null)
                    {
                        targetTask = string.Empty;
                    }
                    else
                    {
                        throw new JsonException($"A propriedade 'task' deve ser uma string com o nome da tarefa, obtido '{reader.TokenType}'.");
                    }
                }
                else
                {
                    // Skip any unrecognized property
                    reader.Skip();
                }
            }
        }

        if (!hasTaskProp)
        {
            throw new JsonException("Objeto de comando inválido: esperado propriedade 'task' especificando o nome da tarefa.");
        }

        return new TaskCommandDto { TargetTask = targetTask ?? string.Empty };
    }

    public override void Write(Utf8JsonWriter writer, List<TaskCommandDto> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Count == 1)
        {
            WriteCommandItem(writer, value[0]);
            return;
        }

        writer.WriteStartArray();
        foreach (var item in value)
        {
            WriteCommandItem(writer, item);
        }
        writer.WriteEndArray();
    }

    private static void WriteCommandItem(Utf8JsonWriter writer, TaskCommandDto item)
    {
        if (item.IsShortcut)
        {
            writer.WriteStartObject();
            writer.WriteString("task", item.TargetTask);
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteStringValue(item.RawCommand ?? string.Empty);
        }
    }
}
