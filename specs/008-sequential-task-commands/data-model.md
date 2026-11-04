# Data Model: Suporte a Múltiplos Comandos Sequenciais em Tarefas

**Branch**: `008-sequential-task-commands` | **Spec**: [spec.md](./spec.md)

Este documento descreve os modelos de dados, estruturas de transferência (DTOs), regras de validação e estados de ciclo de vida para a execução de múltiplos comandos sequenciais em tarefas do `nxtl.json`.

---

## 1. Entidades de Domínio

### ScriptTask

Representa uma tarefa executável única ou sequencial no sistema.

| Campo | Tipo | Descrição |
|---|---|---|
| `Name` | `string` | Identificador único e amigável da tarefa (obrigatório, case-insensitive para busca). |
| `Commands` | `IReadOnlyList<string>` | Lista ordenada de comandos a serem executados na sequência configurada (mínimo 1 comando). |
| `Command` | `string` | Propriedade de conveniência retrocompatível que retorna o primeiro comando de `Commands`. |
| `Type` | `TaskType` | Categoria da tarefa (`Tool` ou `CSharp`). |
| `Description` | `string?` | Descrição textual opcional da tarefa. |

#### Métodos de Domínio:
- `ResolveExecutionCommands() -> IReadOnlyList<string>`: Itera sobre `Commands` e resolve a linha final de comando para cada um. Para tarefas `CSharp`, comandos que terminam em `.cs` e não começam com `dotnet run` são resolvidos com o prefixo `dotnet run `. Para `Tool`, os comandos permanecem inalterados.
- `ResolveExecutionCommand() -> string`: Retorna a resolução do primeiro comando da lista (mantido para compatibilidade prévia).

---

## 2. Objetos de Transferência (DTOs) e Serialização

### ScriptTaskDto

Representa o payload desserializado de uma tarefa declarada em `nxtl.json`.

```csharp
public sealed class ScriptTaskDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("command")]
    [JsonConverter(typeof(CommandPropertyJsonConverter))]
    public List<string>? Commands { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
```

### CommandPropertyJsonConverter

Conversor customizado para `System.Text.Json` com suporte a Native AOT que deserializa polimorficamente a propriedade `command`:
- Se o valor no JSON for uma `string`: instancia `List<string>` com 1 elemento.
- Se o valor no JSON for um `array`: lê todos os elementos e popula `List<string>`.
- Se o valor for nulo: popula com lista nula ou vazia.

---

## 3. Regras de Validação de Configuração (`ConfigurationValidator`)

| Código | Regra | Condição de Erro | Mensagem em Português (`pt-BR`) |
|---|---|---|---|
| **VR-001** | Nome Único | Tarefa com nome duplicado (case-insensitive) | `O script com o nome '{nome}' está duplicado no arquivo de configuração. Cada script deve possuir um nome único.` |
| **VR-002** | Nome Obrigatório | Tarefa sem `name` ou apenas com espaços | `Foi encontrada uma tarefa sem nome válido no arquivo de configuração.` |
| **VR-003** | Comando Obrigatório | `Commands` nulo ou contendo 0 itens | `A tarefa '{nome}' não define um comando a ser executado.` |
| **VR-004** | Item de Comando Válido | Qualquer item de `Commands` nulo ou vazio/espaços | `A tarefa '{nome}' possui um comando vazio ou inválido na lista de comandos.` |
| **VR-005** | Tipo Válido | Tipo informado diferente de `tool` e `csharp` | `A tarefa '{nome}' possui o tipo '{tipo}' não reconhecido. Os tipos aceitos são 'tool' e 'csharp'.` |

---

## 4. Máquina de Estados / Ciclo de Vida da Execução

```
[Início: Tarefa Selecionada]
         │
         ▼
[Exibir "Tarefa selecionada: <nome> [<tipo>]"]
         │
         ▼
  [Etapa i = 0 .. N-1]
         │
         ▼
[Validar arquivo .cs (se CSharp)] ──(Arquivo ausente)──► [Exibir Erro e Abortar (ExitCode = 1)]
         │ (Válido)
         ▼
[Exibir "Comando [X/N]: <cmd>" ou "Comando: <cmd>"]
         │
         ▼
[Executar comando via IProcessExecutor]
         │
    ┌────┴──────────────────────────┐
(ExitCode == 0)                (ExitCode != 0)
    │                               │
    ▼                               ▼
(i < N - 1)                 [Exibir Erro de Interrupção]
  ├── Sim ──► [Próximo i]          │
  └── Não ──► [Sucesso (0)]         ▼
                             [Encerrar e Retornar ExitCode]
```

