# Contract: nxtl.json Configuration Schema

**Branch**: `008-sequential-task-commands` | **Spec**: [spec.md](../spec.md)

Este contrato formaliza a sintaxe declarativa aceita pelo arquivo de configuração `nxtl.json` para tarefas com comandos únicos ou múltiplos.

---

## 1. Definição do Esquema (nxtl-v1.schema.json)

A propriedade `command` de cada objeto de tarefa em `scripts` ou `tasks` suporta dois formatos válidos:

```json
{
  "$defs": {
    "scriptTask": {
      "type": "object",
      "properties": {
        "name": {
          "type": "string",
          "minLength": 1
        },
        "command": {
          "description": "Comando do sistema operacional ou caminho de script C# a ser executado, ou uma lista sequencial de comandos.",
          "oneOf": [
            {
              "type": "string",
              "minLength": 1,
              "description": "Comando único de execução."
            },
            {
              "type": "array",
              "minItems": 1,
              "items": {
                "type": "string",
                "minLength": 1
              },
              "description": "Lista sequencial de comandos a serem executados um por vez."
            }
          ]
        },
        "type": {
          "type": "string",
          "enum": ["tool", "csharp"],
          "default": "tool"
        },
        "description": {
          "type": "string"
        }
      },
      "required": ["name", "command"],
      "additionalProperties": false
    }
  }
}
```

---

## 2. Exemplos de Payloads Válidos

### Exemplo 1: Tarefa com Array de Comandos Sequenciais
```json
{
  "scripts": [
    {
      "name": "full-build",
      "command": [
        "dotnet clean",
        "dotnet build -c Release",
        "dotnet test"
      ],
      "type": "tool",
      "description": "Limpa, compila em release e roda os testes"
    }
  ]
}
```

### Exemplo 2: Tarefa Legada com Comando Único (Retrocompatibilidade)
```json
{
  "scripts": [
    {
      "name": "build",
      "command": "dotnet build",
      "type": "tool",
      "description": "Compila a solução"
    }
  ]
}
```

### Exemplo 3: Tarefa sem Descrição com Múltiplos Comandos
```json
{
  "scripts": [
    {
      "name": "publish-flow",
      "command": [
        "gh workflow run publish.yml",
        "gh run watch"
      ]
    }
  ]
}
```
*No menu interativo, o rótulo desta tarefa será renderizado como: `publish-flow [tool] - <sem descrição>`.*

