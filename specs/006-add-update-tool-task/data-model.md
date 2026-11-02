# Data Model: Adição do Comando de Atualização da Ferramenta ao nxtl.json Padrão

**Feature**: `006-add-update-tool-task` | **Date**: 2026-11-01

Este documento especifica os modelos de dados e a representação JSON do template padrão expandido.

---

## 1. Representação do Arquivo de Configuração (`nxtl.json` Padrão)

O novo template gerado pelo comando `nxtl --init` conterá 4 tarefas pré-configuradas:

```json
{
  "scripts": [
    {
      "name": "build",
      "command": "dotnet build",
      "type": "tool",
      "description": "Compila a solução do projeto"
    },
    {
      "name": "test",
      "command": "dotnet test",
      "type": "tool",
      "description": "Executa todos os testes automatizados"
    },
    {
      "name": "seed",
      "command": "scripts/seed.cs",
      "type": "csharp",
      "description": "Executa o script de carga inicial em C#"
    },
    {
      "name": "update-nettasklist",
      "command": "dotnet tool update -g nettasklist",
      "type": "tool",
      "description": "Atualiza a ferramenta nxtl de modo global"
    }
  ]
}
```

---

## 2. Entidades de Domínio e DTOs

### `ScriptTaskDto`
- **`name`**: `"update-nettasklist"` (não-nulo, único).
- **`command`**: `"dotnet tool update -g nettasklist"` (comando executável no shell).
- **`type`**: `"tool"` (mapeia para `TaskType.Tool`).
- **`description`**: `"Atualiza a ferramenta nxtl de modo global"` (texto orientativo em pt-BR).

### `TaskConfiguration`
- **Count**: 4 tarefas cadastradas na inicialização padrão.
- **HasTasks**: `true`.
- **Tasks**:
  1. `build` (`TaskType.Tool`)
  2. `test` (`TaskType.Tool`)
  3. `seed` (`TaskType.CSharp`)
  4. `update-nettasklist` (`TaskType.Tool`)
