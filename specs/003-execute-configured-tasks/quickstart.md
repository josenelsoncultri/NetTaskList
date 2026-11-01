# Quickstart & Validation Guide: Mecanismo de Execução de Tarefas e Scripts Configurados

**Feature**: `003-execute-configured-tasks` | **Date**: 2026-11-01

Este guia fornece cenários práticos e executáveis para validar ponta a ponta o mecanismo de execução de tarefas do `nxtl`, cobrindo comandos normais de terminal (`tool`), scripts C# file-scoped (`csharp`), propagação de códigos de saída e diagnóstico de erros.

---

## 1. Pré-Requisitos

- **.NET 10 SDK** instalado no ambiente.
- Terminal com suporte a saída padrão e cores ANSI.
- Arquivo de configuração de teste (`nxtl.json`).

---

## 2. Cenários de Validação Ponta a Ponta

### Cenário 1: Execução Direta de Comando Normal de Ferramenta (`tool`)

**Objetivo**: Validar a execução direta de um comando de shell comum através da flag `--run`.

1. **Configuração de teste (`nxtl.json`)**:
   ```json
   {
     "$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
     "scripts": [
       {
         "name": "hello",
         "command": "echo \"Executando comando tool com sucesso!\"",
         "type": "tool",
         "description": "Imprime mensagem de boas-vindas"
       }
     ]
   }
   ```

2. **Comando de Execução**:
   ```bash
   dotnet run --project src/NetTaskList -- --run hello
   ```

3. **Resultado Esperado**:
   - A ferramenta identifica a tarefa `hello` do tipo `tool`.
   - A linha de comando é executada no terminal.
   - A saída `Executando comando tool com sucesso!` é exibida imediatamente no console.
   - O código de saída da ferramenta é `0` (`$LASTEXITCODE` / `$? == 0`).

---

### Cenário 2: Execução Direta de Script C# File-Scoped (`csharp`)

**Objetivo**: Validar que arquivos `.cs` configurados são executados automaticamente através do `dotnet run`.

1. **Criar script de teste (`scripts/demo.cs`)**:
   ```csharp
   System.Console.WriteLine("Script C# file-scoped executado com sucesso!");
   ```

2. **Configuração de teste (`nxtl.json`)**:
   ```json
   {
     "$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
     "scripts": [
       {
         "name": "demo-csharp",
         "command": "scripts/demo.cs",
         "type": "csharp",
         "description": "Executa script C# demonstrativo"
       }
     ]
   }
   ```

3. **Comando de Execução**:
   ```bash
   dotnet run --project src/NetTaskList -- --run demo-csharp
   ```

4. **Resultado Esperado**:
   - A ferramenta resolve automaticamente a execução para `dotnet run scripts/demo.cs`.
   - O script C# é compilado e executado pelo runtime .NET.
   - O texto `Script C# file-scoped executado com sucesso!` é impresso no terminal.
   - O código de saída é `0`.

---

### Cenário 3: Seleção e Execução no Modo Interativo

**Objetivo**: Validar que após navegar e selecionar uma tarefa com as setas no menu interativo, ela é executada imediatamente.

1. **Executar sem argumentos**:
   ```bash
   dotnet run --project src/NetTaskList
   ```

2. **Ações**:
   - O terminal exibe a tabela de tarefas e o menu de seleção interativa.
   - Pressione `Enter` para selecionar a primeira tarefa disponível.

3. **Resultado Esperado**:
   - A tarefa selecionada é iniciada em seguida.
   - A saída do processo filho é refletida no terminal.
   - O processo do `nxtl` finaliza com o mesmo código de saída da tarefa.

---

### Cenário 4: Propagação de Código de Saída de Falha (Exit Code Não-Zero)

**Objetivo**: Validar que códigos de erro retornados por tarefas com falha são propagados fielmente ao chamador.

1. **Configuração de teste com comando de erro**:
   - No Windows: `exit 42`
   - No Linux/macOS: `exit 42` ou `sh -c "exit 42"`
   ```json
   {
     "scripts": [
       {
         "name": "fail-task",
         "command": "exit 42",
         "type": "tool",
         "description": "Retorna código de erro 42"
       }
     ]
   }
   ```

2. **Comando de Execução**:
   ```bash
   dotnet run --project src/NetTaskList -- --run fail-task
   ```

3. **Resultado Esperado**:
   - A tarefa executa e finaliza com código `42`.
   - O código de saída do `nxtl` é exatamente `42` (`echo $LASTEXITCODE` exibe `42`).

---

### Cenário 5: Tratamento de Arquivo de Script C# Inexistente

**Objetivo**: Validar mensagem amigável em português quando um script `.cs` referenciado não existe.

1. **Configuração de teste com script inexistente**:
   ```json
   {
     "scripts": [
       {
         "name": "missing",
         "command": "scripts/nao_existe.cs",
         "type": "csharp",
         "description": "Script que não existe no disco"
       }
     ]
   }
   ```

2. **Comando de Execução**:
   ```bash
   dotnet run --project src/NetTaskList -- --run missing
   ```

3. **Resultado Esperado**:
   - Mensagem de erro em português:
     `[bold red]Erro:[/] O arquivo de script C# '[yellow]scripts/nao_existe.cs[/]' não foi encontrado.`
   - O processo encerra com código de saída de erro `1`.

---

## 3. Verificação Automatizada da Suíte de Testes

Para validar a integridade dos testes unitários desacoplados:
```bash
dotnet test tests/NetTaskList.Tests/NetTaskList.Tests.csproj
```
Todos os testes unitários de resolução de comando, validação de existência, mock de execução de processos e interação de console devem passar com 100% de sucesso.
