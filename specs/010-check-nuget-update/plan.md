# Implementation Plan: Verificação de Nova Versão no NuGet com Alerta no Console

**Branch**: `010-check-nuget-update` | **Date**: 2026-11-11 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/010-check-nuget-update/spec.md`

## Summary

O objetivo desta funcionalidade é acrescentar à ferramenta `nxtl` uma verificação leve e resiliente para detectar se existe uma versão mais recente do pacote publicada no NuGet oficial (`NetTaskList`). Caso uma versão superior seja detectada, um alerta em formato de painel do `Spectre.Console` com borda arredondada amarela é exibido no topo do console antes de qualquer outra informação ou execução de tarefas, informando a versão atual, a versão disponível e instruindo o usuário com o comando `dotnet tool update -g nettasklist`.

A verificação é realizada diretamente a cada execução de forma assíncrona com timeout estrito de 2 segundos, de maneira 100% *stateless* (sem gravação de arquivos de cache em disco). Em casos de falha de rede, modo offline ou latência excessiva, a checagem falha de modo transparente e silencioso, garantindo que o usuário nunca seja bloqueado ou sofra atrasos em suas tarefas locais. Chamadas com `--version` e `--help` não executam a checagem para manter a saída de automação limpa.

---

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)  
**Primary Dependencies**: `Spectre.Console`, `System.Net.Http.HttpClient`, `System.Text.Json` (com código-fonte gerado para Native AOT)  
**Storage**: N/A (operação stateless, sem persistência em disco conforme clarificação)  
**Testing**: `xUnit`, `FluentAssertions` executados via `dotnet test` com mocks de rede  
**Target Platform**: Multiplataforma (Windows, Linux, macOS)  
**Project Type**: CLI tool (.NET Global Tool `nxtl`)  
**Performance Goals**: Tempo limite máximo de rede de 2 segundos; tempo de processamento local da comparação de versões inferior a 5ms  
**Constraints**: Zero dependências de pacotes NuGet externos adicionais; Native AOT trim-safe; mensagens de console 100% em português brasileiro (`pt-BR`); símbolos e testes em inglês  
**Scale/Scope**: Verificação do pacote `NetTaskList` no NuGet Flat Container v3 (`https://api.nuget.org/v3-flatcontainer/nettasklist/index.json`)  

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Principle I (Rich Terminal UX & pt-BR Localization)**: O alerta utiliza `Spectre.Console.Panel` com bordas arredondadas e estilo amarelo. Todas as mensagens voltadas ao usuário (`Nova versão disponível:`, `Para atualizar a ferramenta globalmente, execute:`) estão rigorosamente em `pt-BR`. O código, interfaces, métodos e testes unitários são nomeados estritamente em inglês.
- [x] **Principle II (Declarative Script Configuration)**: A execução das tarefas declaradas em `nxtl.json` e `package.json` permanece inalterada; o alerta é exibido no topo antes de qualquer fluxo de execução.
- [x] **Principle III (Test-First & Mandatory Unit Test Coverage)**: A lógica de verificação e comparação semântica é desacoplada do console e do `HttpClient` via abstrações (`INugetVersionChecker`, `IAnsiConsole`), garantindo 100% de testabilidade unitária sem chamadas reais de rede nos testes automatizados.
- [x] **Principle IV (Minimal Dependencies & Explicit Justification)**: Zero pacotes adicionados. Uso exclusivo de APIs nativas do .NET 10 (`System.Net.Http`, `System.Text.Json`).
- [x] **Principle V (Tool Packaging & Distribution)**: Empacotamento mantido como ferramenta global (`nxtl`) no projeto `src/NetTaskList/NetTaskList.csproj`.
- [x] **Principle VI (Versioning Policy & Spec Implementation Lifecycle)**: Como a especificação adiciona uma nova capacidade e funcionalidade ao CLI, a versão da ferramenta sofrerá incremento Minor (`X.(Y+1).0`) ao final da implementação (de `1.6.1` para `1.7.0`).

---

## Project Structure

### Documentation (this feature)

```text
specs/010-check-nuget-update/
├── plan.md              # Este plano de implementação (/speckit-plan)
├── research.md          # Pesquisa técnica e decisões de arquitetura (/speckit-plan)
├── data-model.md        # Modelos de domínio e DTOs (/speckit-plan)
├── quickstart.md        # Cenários executáveis para validação prática (/speckit-plan)
├── contracts/
│   ├── nuget-api.contract.md      # Contrato da API Flat Container do NuGet
│   └── console-alert.contract.md  # Contrato visual e de posicionamento do alerta no console
└── checklists/
    └── requirements.md  # Checklist de qualidade da especificação
```

### Source Code (repository layout)

```text
src/NetTaskList/
├── Domain/
│   └── Models/
│       ├── SemanticVersion.cs              # Parse e comparação semântica de versões SemVer 2.0
│       └── VersionCheckResult.cs           # Resultado da checagem com métodos de fábrica
├── Infrastructure/
│   ├── Abstractions/
│   │   └── INugetVersionChecker.cs         # Abstração desacoplada para checagem de versão
│   ├── Dtos/
│   │   └── NugetPackageVersionsDto.cs      # DTO para deserializar a lista de versões do NuGet
│   ├── Serialization/
│   │   └── TaskJsonContext.cs              # Registra NugetPackageVersionsDto no JsonSerializerContext
│   └── NugetVersionChecker.cs              # Implementação concreta com HttpClient e timeout de 2s
├── Presentation/
│   └── UpdateNotificationPresenter.cs      # Renderiza o painel Spectre.Console com borda amarela
└── Program.cs                              # Orquestração do alerta no topo antes de tarefas/interativo

tests/NetTaskList.Tests/
├── Domain/
│   ├── SemanticVersionTests.cs             # Testes de parse e comparação de versões (maior, menor, prerelease)
│   └── VersionCheckResultTests.cs          # Testes dos métodos de fábrica
├── Infrastructure/
│   ├── NugetPackageVersionsDtoTests.cs     # Testes de serialização/deserialização AOT
│   └── NugetVersionCheckerTests.cs         # Testes com HttpMessageHandler mockado (sucesso, 404, timeout, offline)
├── Mocks/
│   └── MockNugetVersionChecker.cs          # Mock reutilizável para simular respostas nos testes de Program
└── Presentation/
    ├── UpdateNotificationPresenterTests.cs # Testes de markup e exibição condicional do painel
    └── ProgramExecutionTests.cs            # Testes de integração do alerta no topo em modo interativo e --run
```

---

## Complexity Tracking

> Nenhuma violação das regras da Constituição foi necessária. Zero pacotes de terceiros adicionados.
