# Data Model & Domain Entities: Verificação de Nova Versão no NuGet

**Feature**: Verificação de Nova Versão no NuGet com Alerta no Console  
**Branch**: `010-check-nuget-update`  
**Date**: 2026-11-11

---

## 1. Domain Entities & Value Objects

### `SemanticVersion` (`NetTaskList.Domain.Models`)
Representa e encapsula uma versão semântica compatível com SemVer 2.0 para comparação segura entre versões da aplicação e versões publicadas no NuGet.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Major` | `int` | Número da versão maior (Major). |
| `Minor` | `int` | Número da versão menor (Minor). |
| `Patch` | `int` | Número da versão de correção/build (Patch). |
| `PreRelease` | `string?` | Identificador de pré-lançamento opcional (ex: `preview.1`, `beta`). |
| `IsPreRelease` | `bool` | `true` se contiver tag de pré-lançamento; caso contrário, `false`. |

**Operações e Invariantes**:
- `static bool TryParse(string? text, out SemanticVersion? result)`: Converte strings no formato `X.Y.Z` ou `X.Y.Z-prerelease` (ignorando sufixos de metadados após `+`). Retorna `false` para formatos inválidos ou vazios.
- `int CompareTo(SemanticVersion? other)`: Implementa `IComparable<SemanticVersion>`. Compara `Major`, `Minor` e `Patch` numericamente. Se os números forem idênticos, uma versão estável é estritamente maior que uma versão de pré-lançamento.
- `bool IsNewerThan(SemanticVersion other)`: Atalho booleano para `CompareTo(other) > 0`.

---

### `VersionCheckResult` (`NetTaskList.Domain.Models`)
Representa o resultado da verificação de versão da ferramenta no NuGet.

| Propriedade | Tipo | Descrição |
|---|---|---|
| `HasUpdate` | `bool` | `true` se foi encontrada uma versão mais recente estável; caso contrário, `false`. |
| `CurrentVersion` | `string` | Versão instalada atualmente no binário do executável. |
| `LatestVersion` | `string?` | Versão mais recente estável disponível no NuGet (se houver). |
| `IsSuccess` | `bool` | `true` se a consulta ao NuGet foi concluída com sucesso; `false` em caso de erro/timeout. |

**Métodos de Fábrica (Factory Methods)**:
- `VersionCheckResult.NoUpdate(string currentVersion)`: Indica que a ferramenta já está na versão mais recente.
- `VersionCheckResult.UpdateAvailable(string currentVersion, string latestVersion)`: Indica que uma nova versão está disponível.
- `VersionCheckResult.Failed(string currentVersion)`: Indica que a consulta falhou ou expirou por timeout (silencioso, com `HasUpdate = false`).

---

## 2. Infrastructure DTOs & Serialization

### `NugetPackageVersionsDto` (`NetTaskList.Infrastructure.Dtos`)
DTO para deserialização do JSON retornado pelo endpoint Flat Container do NuGet:
`https://api.nuget.org/v3-flatcontainer/nettasklist/index.json`

| Propriedade | Campo JSON | Tipo | Descrição |
|---|---|---|---|
| `Versions` | `versions` | `List<string>?` | Lista contendo todas as versões publicadas do pacote no NuGet. |

**AOT Context**:
Registrado no contexto do gerador de código `TaskJsonContext`:
```csharp
[JsonSerializable(typeof(NugetPackageVersionsDto))]
```

---

## 3. Interfaces & Presentation

### `INugetVersionChecker` (`NetTaskList.Infrastructure.Abstractions`)
Abstração para consulta do registro de pacotes NuGet:
```csharp
public interface INugetVersionChecker
{
    VersionCheckResult CheckForUpdate(string currentVersion, TimeSpan timeout);
    Task<VersionCheckResult> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default);
}
```

### `UpdateNotificationPresenter` (`NetTaskList.Presentation`)
Responsável por formatar e renderizar o painel Spectre.Console com base no `VersionCheckResult`.
- Renderiza o painel com borda arredondada amarela somente quando `result.HasUpdate == true`.
- Utiliza textos estritamente em português do Brasil (`pt-BR`).
