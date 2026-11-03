# Implementation Plan: Suporte ao Arquivo package.json (package.json Support)

**Branch**: `007-support-package-json` | **Date**: 2026-11-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/007-support-package-json/spec.md`

## Summary

O objetivo desta funcionalidade é permitir que o `nxtl` funcione como um substituto transparente da ferramenta `ntl` do ecossistema Node.js, capacitando a ferramenta a descobrir e ler scripts diretamente do arquivo `package.json` caso o arquivo nativo `nxtl.json` não esteja presente no diretório. Caso ambos os arquivos existam, `nxtl.json` possui prevalência absoluta e `package.json` é totalmente ignorado. Além disso, quando tarefas forem executadas a partir de um `package.json`, o `nxtl` orquestrará automaticamente os ganchos de ciclo de vida (`pre<tarefa>` e `post<tarefa>`) e manterá o menu interativo limpo, ocultando esses ganchos quando pertencerem a uma tarefa principal existente.

A abordagem técnica utiliza o `System.Text.Json` com código-fonte gerado (`JsonSerializerContext`) para garantir máxima performance e 100% de conformidade com Native AOT sem adicionar dependências externas ao projeto.

---

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)  
**Primary Dependencies**: `Spectre.Console`, `System.Text.Json` (código gerado / zero reflexão)  
**Storage**: Arquivos do sistema local (`nxtl.json`, `package.json`)  
**Testing**: `xUnit`, `FluentAssertions` executados via `dotnet test`  
**Target Platform**: Multiplataforma (Windows, Linux, macOS)  
**Project Type**: CLI tool (.NET Global Tool `nxtl`)  
**Performance Goals**: Tempo de detecção, carregamento e renderização de scripts inferior a 200ms  
**Constraints**: Zero pacotes NuGet adicionais; trim-safe (Native AOT); mensagens de interface 100% em português brasileiro (`pt-BR`); símbolos de código e testes em inglês  
**Scale/Scope**: Repositórios com arquivos `package.json` e `nxtl.json`  

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Principle I (Rich Terminal UX & pt-BR Localization)**: Todas as mensagens interativas, erros de arquivos ausentes, avisos e saídas diagnósticas do terminal são formuladas em `pt-BR`. O código e seus testes são nomeados em inglês.
- [x] **Principle II (Declarative Script Configuration)**: Validação explícita de esquema de scripts em `package.json` com mensagens amigáveis em português em caso de JSON inválido ou comandos malformados.
- [x] **Principle III (Test-First & Mandatory Unit Test Coverage)**: Desacoplamento via abstrações (`IFileSystem`, `IProcessExecutor`), permitindo testes unitários abrangentes cobrindo resolução de precedência, leitura de `package.json`, orquestração de ciclo de vida e filtragem de menu interativo.
- [x] **Principle IV (Minimal Dependencies & Explicit Justification)**: Nenhuma dependência NuGet externa adicionada. A serialização faz uso dos recursos nativos do .NET 10.
- [x] **Principle V (Tool Packaging & Distribution)**: Mantém o empacotamento padrão como global tool em `src/NetTaskList/NetTaskList.csproj` direcionado ao .NET 10.
- [x] **Principle VI (Versioning Policy & Spec Implementation Lifecycle)**: Por adicionar uma nova capacidade de configuração e comando, a versão sofrerá incremento Minor (`X.(Y+1).0`) ao término da implementação.

---

## Project Structure

### Documentation (this feature)

```text
specs/007-support-package-json/
├── plan.md              # Este plano de implementação
├── research.md          # Pesquisa técnica e decisões de arquitetura
├── data-model.md        # Modelos de domínio, DTOs e regras de ciclo de vida
├── quickstart.md        # Cenários executáveis para validação prática
├── contracts/
│   ├── configuration-resolution.contract.md  # Contrato de precedência de arquivos
│   ├── lifecycle-execution.contract.md       # Contrato de orquestração pre/post
│   └── package-json.schema.json              # Subconjunto do esquema JSON lido
└── checklists/
    └── requirements.md  # Validação de qualidade da especificação
```

### Source Code (repository layout)

```text
src/NetTaskList/
├── Domain/
│   ├── Models/
│   │   ├── ConfigurationSourceType.cs        # Enum: NxtlJson, PackageJson, Custom
│   │   ├── ScriptTask.cs
│   │   ├── TaskConfiguration.cs              # Atualizado para incluir ConfigurationSourceType
│   │   └── TaskType.cs
│   └── Validation/
│       ├── ConfigurationValidator.cs
│       └── ValidationResult.cs
├── Infrastructure/
│   ├── Abstractions/
│   │   ├── IFileSystem.cs
│   │   └── IProcessExecutor.cs
│   ├── ConfigurationFileResolver.cs          # Descoberta e resolução de precedência
│   ├── JsonConfigurationReader.cs            # Leitor polimórfico de nxtl.json e package.json
│   ├── Dtos/
│   │   ├── ConfigurationDto.cs
│   │   └── PackageJsonDto.cs                 # DTO de desserialização para package.json
│   └── Serialization/
│       └── TaskJsonContext.cs                # Adiciona suporte AOT para PackageJsonDto
├── Presentation/
│   ├── CommandLineParser.cs
│   ├── InteractiveTaskSelector.cs            # Atualizado para filtrar pre*/post* em package.json
│   └── TaskExecutionCoordinator.cs           # Orquestrador do fluxo sequencial pre/main/post
└── Program.cs                                # Ponto de entrada integrando resolução e coordenação

tests/NetTaskList.Tests/
├── Domain/
│   └── TaskConfigurationTests.cs
├── Infrastructure/
│   ├── ConfigurationFileResolverTests.cs     # Testes da regra de precedência nxtl.json vs package.json
│   └── PackageJsonReaderTests.cs             # Testes de leitura, parsing e validação de package.json
└── Presentation/
    ├── InteractiveTaskSelectorFilteringTests.cs # Testes de ocultação de ganchos no menu interativo
    └── TaskExecutionCoordinatorTests.cs         # Testes do encadeamento de pre/main/post e abortos
```

**Structure Decision**: Adota a estrutura de projeto único existente em `src/NetTaskList/` e `tests/NetTaskList.Tests/`, respeitando a separação arquitetural estrita entre Apresentação, Domínio e Infraestrutura conforme ratificado na Constituição.

---

## Complexity Tracking

> Nenhuma violação ou dependência excessiva identificada. O design respeita integralmente os princípios de YAGNI e KISS.
