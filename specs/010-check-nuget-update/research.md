# Technical Research & Architecture Decisions: Verificação de Nova Versão no NuGet

**Feature**: Verificação de Nova Versão no NuGet com Alerta no Console  
**Branch**: `010-check-nuget-update`  
**Date**: 2026-11-11

---

## 1. NuGet Version Query Protocol & Endpoint

### Context
A ferramenta `nxtl` precisa descobrir se existe uma versão mais recente do pacote `NetTaskList` publicada no NuGet, de forma leve, resiliente e compatível com as regras de dependências mínimas da Constituição (Princípio IV).

### Decision
Utilizar o endpoint oficial do NuGet Flat Container v3:
`https://api.nuget.org/v3-flatcontainer/nettasklist/index.json`

Formato retornado pela API:
```json
{
  "versions": [
    "1.0.0",
    "1.2.0",
    "1.6.0",
    "1.6.1"
  ]
}
```

A requisição será realizada utilizando o `HttpClient` nativo do .NET 10, consumindo os dados via `System.Text.Json` com código-fonte gerado (`TaskJsonContext`), garantindo 100% de compatibilidade com compilação Native AOT e trimming.

### Rationale
- **Leveza e Desempenho**: O payload JSON contém apenas uma lista de strings de versões (poucos bytes), sem metadados pesados de pacotes.
- **CDN Global**: O endpoint do flat container é distribuído por CDN global (Cloudflare/Fastly), oferecendo tempos de resposta em dezenas de milissegundos.
- **Zero Dependências Adicionais**: Não requer inclusão dos pacotes do `NuGet.Protocol` ou `NuGet.Versioning`, mantendo o grafo de dependências do projeto limpo (Princípio IV).

### Alternatives Considered
- *NuGet.Protocol SDK*: Rejeitado porque introduz dezenas de dependências transitivas desnecessárias para apenas ler um array de versões.
- *Search Query Service (`azuresearch-usnc.nuget.org/query`)*: Rejeitado por payload significativamente maior, latência superior e atraso de indexação em relação ao flat container.

---

## 2. Semantic Version Comparison (SemVer)

### Context
As versões retornadas pelo NuGet seguem o padrão SemVer (ex: `1.6.1`, `1.7.0`, `2.0.0-preview.1`). A versão atual instalada é extraída do próprio binário executável (`AppVersion.Current`). Precisamos comparar se a versão remota mais alta é estritamente maior que a versão local.

### Decision
Implementar um comparador leve de versões semânticas (`SemanticVersion`) em C# puro:
- Realiza parse de `Major.Minor.Patch`.
- Identifica tags de pré-lançamento (ex: sufixo `-preview`, `-rc`).
- Filtra apenas versões estáveis (sem tags de pré-lançamento) para alertas de atualização, a menos que a versão instalada atual já seja uma versão de pré-lançamento.
- Implementa `IComparable<SemanticVersion>` ou método `IsNewerThan(SemanticVersion other)`.

### Rationale
- `System.Version` nativo do .NET não lida nativamente com sufixos SemVer 2.0 (como `-preview`).
- Uma implementação enxuta (~40 linhas) é totalmente previsível, rápida e 100% testável unitariamente sem qualquer dependência externa.

### Alternatives Considered
- *System.Version*: Falha caso versões no NuGet ou locais contenham sufixos de pré-lançamento.
- *NuGet.Versioning Package*: Adicionaria dependência externa dispensável.

---

## 3. Network Resilience, Strict Timeout & Stateless Design

### Context
O usuário esclareceu na fase de especificação/clarificação:
1. A verificação deve consultar o NuGet diretamente a cada execução com timeout rápido (1 a 2 segundos).
2. Não devem ser criados arquivos de cache ou temporários em disco (operação stateless).
3. O CLI nunca deve travar, atrasar ou exibir erros técnicos se o usuário estiver offline ou com conexão instável.

### Decision
- **Timeout Rígido**: Configurar `CancellationTokenSource` com timeout de 2 segundos (1.5s a 2s).
- **Tratamento Silencioso de Erros**: Capturar e tratar internamente qualquer exceção de rede (`HttpRequestException`, `TaskCanceledException`, `OperationCanceledException`, `SocketException`, `JsonException`).
- **Retorno Seguro**: Em caso de timeout ou falha de conexão, retornar imediatamente `VersionCheckResult(hasUpdate: false)` sem qualquer mensagem de erro ao usuário.
- **Stateless**: Nenhuma gravação em arquivo no disco.

### Rationale
- Mantém o CLI rápido e previsível. Em ambientes corporativos restritos ou offline, o usuário sequer percebe que houve uma tentativa de verificação e suas tarefas locais executam sem interrupção.

---

## 4. Visual Presentation in Spectre.Console

### Context
O usuário definiu que o alerta de atualização deve aparecer no topo do console (antes de qualquer outra saída) como um painel (`Panel`) com borda arredondada amarela.

### Decision
Utilizar `Spectre.Console.Panel`:
```csharp
var content = new Markup(
    $"[bold yellow]Nova versão disponível:[/] [white]{currentVersion}[/] → [bold green]{latestVersion}[/]\n" +
    $"Para atualizar a ferramenta globalmente, execute:\n" +
    $"[bold cyan]dotnet tool update -g nettasklist[/]");

var panel = new Panel(content)
{
    Border = BoxBorder.Rounded,
    BorderStyle = new Style(Color.Yellow),
    Padding = new Padding(1, 0, 1, 0)
};

console.Write(panel);
console.WriteLine();
```

### Rationale
- Excelente destaque visual no terminal sem poluição visual.
- Mensagens 100% em português brasileiro (`pt-BR`), respeitando o Princípio I da Constituição.
- Borda arredondada amarela confere aviso amigável e legível.

---

## 5. Arquitetura e Inversão de Dependência para Testabilidade

### Context
O Princípio III da Constituição exige testes unitários obrigatórios e desacoplamento do console e de chamadas externas de I/O / rede.

### Decision
Introduzir a abstração `INugetVersionChecker`:
```csharp
namespace NetTaskList.Infrastructure.Abstractions;

public interface INugetVersionChecker
{
    VersionCheckResult CheckForUpdate(string currentVersion, TimeSpan timeout);
    Task<VersionCheckResult> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default);
}
```

Implementação concreta `NugetVersionChecker` em `NetTaskList.Infrastructure`:
- Utiliza `HttpClient` para invocar a API do NuGet.
- Usa `NugetPackageVersionsDto` registrado no `TaskJsonContext`.

Injeção em `Program.Run`:
```csharp
public static int Run(
    string[] args,
    IFileSystem? fileSystem = null,
    IProcessExecutor? processExecutor = null,
    IAnsiConsole? console = null,
    INugetVersionChecker? nugetVersionChecker = null)
```

No ambiente de testes (`NetTaskList.Tests`), injeta-se um `MockNugetVersionChecker` ou instância mockada, validando todos os cenários sem realizar requisições reais à internet durante o `dotnet test`.
