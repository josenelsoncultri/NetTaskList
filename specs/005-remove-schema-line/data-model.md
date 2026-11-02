# Data Model: Remoção da Linha de Schema do nxtl.json de Configuração

**Feature**: `005-remove-schema-line` | **Date**: 2026-11-01

Este documento descreve as estruturas de dados, representações JSON e mapeamentos de domínio relacionados ao arquivo de configuração `nxtl.json`.

---

## 1. Representação do Arquivo de Configuração (`nxtl.json`)

### Formato Padrão Limpo (Novo Padrão)
O arquivo de configuração padrão não possui a chave `"$schema"`. Ele declara apenas os nós de configuração e tarefas.

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
    }
  ]
}
```

### Formato Legado com Schema (Compatibilidade Suportada)
Arquivos gerados em versões anteriores que possuam a chave `"$schema"` continuam sendo processados sem erro.

```json
{
  "$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
  "scripts": [ ... ]
}
```

---

## 2. Estrutura de DTOs (`NetTaskList.Infrastructure.Dtos`)

### `ConfigurationDto`
Objeto de transferência de dados utilizado durante a deserialização via `System.Text.Json`.

| Propriedade | Tipo | Mapeamento JSON | Obrigatoriedade | Descrição |
|---|---|---|---|---|
| `Schema` | `string?` | `"$schema"` | Opcional | Preservado para tolerar arquivos legados sem gerar erros de desserialização. |
| `Scripts` | `List<ScriptTaskDto>?` | `"scripts"` | Opcional (ou `Tasks`) | Lista principal de tarefas declaradas. |
| `Tasks` | `List<ScriptTaskDto>?` | `"tasks"` | Opcional (ou `Scripts`) | Alias legado suportado para a lista de tarefas. |

### `ScriptTaskDto`

| Propriedade | Tipo | Mapeamento JSON | Obrigatoriedade | Descrição |
|---|---|---|---|---|
| `Name` | `string?` | `"name"` | Obrigatório | Nome identificador único da tarefa. |
| `Command` | `string?` | `"command"` | Obrigatório | Comando ou caminho de script a executar. |
| `Type` | `string?` | `"type"` | Opcional (Default: `"tool"`) | Tipo da tarefa (`"tool"` ou `"csharp"`). |
| `Description` | `string?` | `"description"` | Opcional | Texto explicativo da tarefa. |

---

## 3. Modelo de Domínio (`NetTaskList.Domain.Models`)

Os modelos de domínio permanecem estritamente isolados da representação textual ou do formato de arquivo:

### `TaskConfiguration`
- **FilePath**: Caminho absoluto do arquivo `nxtl.json` carregado.
- **Tasks**: Coleção imutável de `ScriptTask`.
- **HasTasks**: Booleano indicando se existem tarefas carregadas.
- **Count**: Quantidade total de tarefas registradas.

*Nota*: O modelo de domínio `TaskConfiguration` não armazena referências a `$schema`, garantindo total desacoplamento entre infraestrutura e regras de negócio.
