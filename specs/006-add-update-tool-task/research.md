# Research: Adição do Comando de Atualização da Ferramenta ao nxtl.json Padrão

**Feature**: `006-add-update-tool-task` | **Date**: 2026-11-01

Este documento registra as análises técnicas, padrões de comando e decisões de arquitetura para a inclusão da tarefa de atualização no template de configuração padrão.

---

## 1. Definição do Comando e Identificador do Pacote

### Contexto
O comando de atualização para ferramentas globais no ecossistema .NET utiliza a sintaxe `dotnet tool update -g <PackageId>`.
No arquivo de projeto `src/NetTaskList/NetTaskList.csproj`, o identificador do pacote NuGet está registrado como:
```xml
<PackageId>NetTaskList</PackageId>
```
No ecossistema .NET CLI e no NuGet.org, os identificadores de pacotes não diferenciam maiúsculas de minúsculas (`case-insensitive`).
O usuário confirmou que o identificador correto é `nettasklist` (sendo `nexttasklist` um equívoco de digitação na solicitação original).

### Decisão
Definir a nova tarefa com:
- **`name`**: `"update-nettasklist"`
- **`command`**: `"dotnet tool update -g nettasklist"`
- **`type`**: `"tool"`
- **`description`**: `"Atualiza a ferramenta nxtl de modo global"`

### Justificativa
- Segue a convenção de nomenclatura de scripts em minúsculas com hífen.
- Garante resolução correta no registro de pacotes do NuGet.org.
- A descrição fornece clareza imediata no menu interativo de seleção de tarefas (`nxtl`).

---

## 2. Posição no Template de Configuração (`TemplateConfigurationGenerator`)

### Contexto
Atualmente, `TemplateConfigurationGenerator.GenerateDefaultJson()` gera 3 tarefas de demonstração:
1. `build` (`dotnet build`)
2. `test` (`dotnet test`)
3. `seed` (`scripts/seed.cs`)

### Decisão
Acrescentar a tarefa `update-nettasklist` logo após a tarefa `seed` (ou como quarta tarefa padrão da lista).

### Justificativa
- Mantém as tarefas de ciclo de vida de código (`build`, `test`, `seed`) agrupadas inicialmente.
- Posiciona a tarefa utilitária de manutenção do ambiente como uma opção de conveniência.
- O gerador continuará produzindo um JSON limpo e diretamente deserializável por `JsonConfigurationReader`.

---

## 3. Impacto em Testes e Validações

### Contexto
Em `TemplateConfigurationGeneratorTests.cs`, o teste existente valida a quantidade de tarefas geradas:
```csharp
Assert.Equal(3, config.Count);
```

### Decisão
1. Atualizar a asserção para verificar 4 tarefas geradas (`Assert.Equal(4, config.Count)`).
2. Adicionar asserções específicas para a presença de `"update-nettasklist"` e `"dotnet tool update -g nettasklist"`.
3. Validar que o mapeamento no leitor de configuração atribui o tipo `TaskType.Tool` e a descrição esperada.
