# Contract: nxtl.json Configuration Schema for Task Shortcuts

**Branch**: `009-task-command-shortcuts` | **Spec**: [spec.md](../spec.md)

Este contrato formaliza a sintaxe declarativa aceita pelo arquivo de configuração `nxtl.json` para tarefas que utilizam atalhos de tarefas (`task shortcuts`) como comando único ou dentro de arrays de comandos sequenciais.

---

## 1. Definição do Esquema (nxtl-v1.schema.json)

A propriedade `command` de cada objeto de tarefa em `scripts` ou `tasks` suporta três formas válidas:
1. Uma `string` direta de comando shell.
2. Um `object` de atalho com o formato `{ "task": "<nome da tarefa>" }`.
3. Um `array` contendo itens do tipo `string` ou `object` (`{ "task": "<nome da tarefa>" }`).

```json
{
  "$defs": {
    "scriptTask": {
      "type": "object",
      "properties": {
        "name": {
          "type": "string",
          "minLength": 1,
          "description": "Identificador único e amigável da tarefa."
        },
        "command": {
          "description": "Comando do sistema operacional, atalho de tarefa ou lista sequencial de comandos/atalhos.",
          "oneOf": [
            {
              "type": "string",
              "minLength": 1,
              "description": "Comando único de execução."
            },
            {
              "$ref": "#/$defs/taskShortcut",
              "description": "Atalho para outra tarefa de ferramenta configurada."
            },
            {
              "type": "array",
              "minItems": 1,
              "items": {
                "oneOf": [
                  {
                    "type": "string",
                    "minLength": 1
                  },
                  {
                    "$ref": "#/$defs/taskShortcut"
                  }
                ]
              },
              "description": "Lista sequencial de comandos e/ou atalhos de tarefas a serem executados um por vez."
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
    },
    "taskShortcut": {
      "type": "object",
      "title": "TaskShortcut",
      "description": "Referência de atalho para outra tarefa configurada no nxtl.json.",
      "properties": {
        "task": {
          "type": "string",
          "minLength": 1,
          "description": "Nome da tarefa de destino a ser executada."
        }
      },
      "required": ["task"],
      "additionalProperties": false
    }
  }
}
```

---

## 2. Exemplos de Payloads Válidos

### Exemplo 1: Atalho de Comando Único
```json
{
  "scripts": [
    {
      "name": "build",
      "command": "dotnet build",
      "type": "tool"
    },
    {
      "name": "compilar",
      "command": { "task": "build" },
      "type": "tool",
      "description": "Atalho em português para a tarefa de build"
    }
  ]
}
```

### Exemplo 2: Sequência Mista de Comandos e Atalhos
```json
{
  "scripts": [
    {
      "name": "clean",
      "command": "dotnet clean",
      "type": "tool"
    },
    {
      "name": "build",
      "command": "dotnet build",
      "type": "tool"
    },
    {
      "name": "rebuild-and-test",
      "command": [
        "echo Iniciando processo de rebuild...",
        { "task": "clean" },
        { "task": "build" },
        "dotnet test --no-build"
      ],
      "type": "tool",
      "description": "Executa limpeza, compilação e testes"
    }
  ]
}
```

### Exemplo 3: Estrutura em Diamante (Válida / Sem Loop)
```json
{
  "scripts": [
    {
      "name": "shared-prep",
      "command": "echo preparando ambiente",
      "type": "tool"
    },
    {
      "name": "module-a",
      "command": [
        { "task": "shared-prep" },
        "echo compilando modulo A"
      ],
      "type": "tool"
    },
    {
      "name": "module-b",
      "command": [
        { "task": "shared-prep" },
        "echo compilando modulo B"
      ],
      "type": "tool"
    },
    {
      "name": "all",
      "command": [
        { "task": "module-a" },
        { "task": "module-b" }
      ],
      "type": "tool"
    }
  ]
}
```

---

## 3. Exemplos de Payloads Inválidos (Rejeitados)

### Exemplo 4: Atalho com Nome Vazio
```json
{
  "scripts": [
    {
      "name": "invalido",
      "command": { "task": "" },
      "type": "tool"
    }
  ]
}
```
*Erro emitido: `A tarefa 'invalido' possui um atalho com nome de tarefa vazio.`*

### Exemplo 5: Atalho Declarado em Tarefa C#
```json
{
  "scripts": [
    {
      "name": "script-csharp",
      "command": { "task": "clean" },
      "type": "csharp"
    }
  ]
}
```
*Erro emitido: `A tarefa 'script-csharp' é do tipo 'csharp' e não pode conter atalhos para outras tarefas.`*
