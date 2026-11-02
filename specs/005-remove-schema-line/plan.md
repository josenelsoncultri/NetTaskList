# Implementation Plan: Remoção da Linha de Schema do nxtl.json de Configuração

**Branch**: `005-remove-schema-line` | **Date**: 2026-11-01 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/005-remove-schema-line/spec.md`

## Summary

Remover a linha indicativa de `"$schema"` gerada no arquivo modelo de configuração (`nxtl.json`) e no arquivo de configuração do próprio repositório. O gerador padrão em `TemplateConfigurationGenerator.GenerateDefaultJson()` deixará de incluir o atributo `"$schema"`, entregando um JSON conciso e direto. O leitor `JsonConfigurationReader` e o DTO `ConfigurationDto` manterão tolerância e compatibilidade retroativa com configurações legadas que ainda possuam `"$schema"`. Além disso, o arquivo `nxtl.json` da raiz do repositório e os exemplos documentados em `README.md` serão alinhados ao formato sem a linha de schema.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (`net10.0`), compilado com suporte a Native AOT (`<PublishAot>true</PublishAot>`).

**Primary Dependencies**:
- `System.Text.Json` (nativo do .NET 10): Desserialização através de Source Generators AOT (`TaskJsonContext`).
- `Spectre.Console` (v0.57.2): Apresentação no terminal para o comando `nxtl --init` e execução de tarefas.

**Storage**: Arquivos locais JSON (`nxtl.json`).

**Testing**: xUnit (`net10.0`) com testes unitários em `tests/NetTaskList.Tests/` cobrindo `TemplateConfigurationGenerator` e `JsonConfigurationReader`.

**Target Platform**: Multiplataforma (Windows, Linux, macOS) como .NET Global Tool (`nxtl`) e binário nativo autônomo.

**Project Type**: CLI para desenvolvedores.

**Performance Goals**: Desempenho de leitura e geração sub-5ms, mantendo inicialização rápida e compatível com Native AOT.

**Constraints**:
- Zero adição de novas dependências externas (Princípio IV).
- Conformidade estrita com Native AOT.
- Nenhuma mensagem ou saída de erro do CLI em inglês (Princípio I - Localization pt-BR).
- Código-fonte, DTOs e testes nomeados exclusivamente em inglês (Princípio I).
- Preservação da organização das pastas `src/` e `tests/`.

**Scale/Scope**: Modificação pontual no gerador de template, no `nxtl.json` raiz, testes e documentação de suporte.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Status | Avaliação de Conformidade |
|---|---|---|
| **I. Rich Terminal UX (Spectre.Console & Localization)** | **PASS** | Mensagens de sucesso e erro do comando `nxtl --init` permanecem em português brasileiro (`pt-BR`). Códigos e testes em inglês. |
| **II. Declarative Script Configuration (JSON / YAML)** | **PASS** | O formato JSON declarativo permanece compatível. A remoção de `"$schema"` despolui o arquivo sem quebrar a validação das tarefas. Compatibilidade com arquivos legados contendo `"$schema"` é preservada. |
| **III. Test-First & Mandatory Unit Test Coverage** | **PASS** | Testes automatizados cobrem a ausência de `"$schema"` no gerador e o suporte a arquivos legados com `"$schema"`. Todos os testes passam via `dotnet test`. |
| **IV. Minimal Dependencies & Explicit Justification** | **PASS** | Nenhuma dependência externa adicionada. |
| **V. Tool Packaging & Distribution (.NET Global Tool - nxtl)** | **PASS** | Mantém a compilação como ferramenta global e Native AOT. |
| **VI. Versioning Policy & Spec Implementation Lifecycle** | **PASS** | Mudança avaliada como ajuste pequeno/refinamento de template e configuração. Bump estimado como PATCH ou conforme regras ao concluir a implementação. |
| **Estrutura de Diretórios (`src/` e `tests/`)** | **PASS** | Todas as alterações restritas aos diretórios `src/NetTaskList/`, `tests/NetTaskList.Tests/` e arquivos de documentação/configuração raiz. |

*Resultado do Gate*: **APROVADO (Zero violações)**.

## Project Structure

### Documentation (this feature)

```text
specs/005-remove-schema-line/
├── plan.md              # Este plano de implementação
├── research.md          # Pesquisa técnica e decisões de arquitetura (Fase 0)
├── data-model.md        # Modelagem de dados e contratos JSON (Fase 1)
├── quickstart.md        # Guia prático de validação ponta a ponta (Fase 1)
├── contracts/           # Contratos formais da interface e arquivo de configuração (Fase 1)
│   └── nxtl-config-contract.md
└── checklists/
    └── requirements.md  # Checklist de qualidade dos requisitos
```

### Source Code (repository root)

```text
nxtl.json                                       # ATUALIZADO: Remoção da propriedade "$schema"
README.md                                       # ATUALIZADO: Ajuste dos exemplos e referências de nxtl.json

src/NetTaskList/
├── Infrastructure/
│   ├── TemplateConfigurationGenerator.cs      # ATUALIZADO: Remoção de "$schema" do template gerado
│   ├── JsonConfigurationReader.cs             # Inalterado (já suporta arquivos sem schema)
│   └── Dtos/
│       └── ConfigurationDto.cs                 # Mantém Schema opcional para compatibilidade retroativa
└── schemas/
    └── nxtl-v1.schema.json                     # Mantido para referência e empacotamento opcional

tests/NetTaskList.Tests/
└── Infrastructure/
    ├── TemplateConfigurationGeneratorTests.cs # ATUALIZADO: Validação de ausência de "$schema" no template gerado
    └── JsonConfigurationReaderTests.cs        # ATUALIZADO: Validação de compatibilidade com arquivos legados com "$schema"
```

**Structure Decision**: Preserva a arquitetura limpa de projeto único em `src/NetTaskList/` e `tests/NetTaskList.Tests/`, em plena conformidade com a Constituição.

## Complexity Tracking

> Nenhuma violação constitucional identificada. Tabela de justificativas dispensada.
