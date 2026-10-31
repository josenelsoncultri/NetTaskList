# Research & Technical Decisions: Processador de Configuração JSON

**Feature**: `001-json-config-processor`  
**Date**: 2026-10-30  
**Status**: Resolved  

Este documento registra as pesquisas técnicas, decisões arquiteturais e análises de alternativas para a implementação do processador de arquivos de configuração em JSON, interface com Spectre.Console, validação de integridade e empacotamento NuGet para o .NET 10.

---

## 1. Plataforma de Execução e Versão do .NET

### Decisão
Utilizar **.NET 10 (`net10.0`)** com C# 14 como runtime e linguagem padrão da solução.

### Justificativa
- O SDK .NET 10 (`10.0.403`) e o runtime `10.0.14` estão instalados e disponíveis no ambiente.
- Cumpre a exigência explícita do usuário e o Princípio V da Constituição (`nxtl`), aproveitando as melhorias de desempenho do runtime mais recente, modernizações em `System.Text.Json` e recursos aprimorados de C#.

### Alternativas Consideradas
- **.NET 8 LTS**: Embora também instalado na máquina, a diretriz constitucional e o pedido do usuário exigem expressamente a versão mais recente instalada (.NET 10).

---

## 2. Estrutura do Esquema JSON e Mecanismo de Parsing

### Decisão
Utilizar o namespace nativo **`System.Text.Json`** com deserialização estruturada e validação em dois estágios:
1. Leitura flexível com suporte a modelo de objeto raiz (`{ "scripts": [ ... ] }` ou `{ "tasks": [ ... ] }`) e modelo de array direto na raiz (`[ ... ]`).
2. Validação semântica pós-leitura:
   - Verificação de campos obrigatórios (`name`, `command`).
   - Normalização e checagem de nomes duplicados usando `HashSet<string>(StringComparer.OrdinalIgnoreCase)`.
   - Inferência de tipo padrão (`tool`) e reconhecimento de tipo C# file-scoped (`csharp`).
3. Fornecimento de um **JSON Schema oficial** (`config-schema.json`) e funcionalidade de geração de arquivo modelo padrão (`nxtl.json`) com tarefas de exemplo comentadas.

### Justificativa
- `System.Text.Json` é nativo do .NET 10 (zero dependências externas adicionais, em estrita conformidade com o Princípio IV da Constituição).
- Desempenho de parsing ultrarrápido com alocações mínimas de memória via `Utf8JsonReader` e `JsonDocument`.
- Oferece suporte completo a validações personalizadas e mensagens de erro diagnósticas em português brasileiro (`pt-BR`).

### Alternativas Consideradas
- **Newtonsoft.Json**: Rejeitado pois adicionaria dependência externa desnecessária contrariando o Princípio IV da Constituição, uma vez que `System.Text.Json` no .NET 10 atende a todos os requisitos com superior performance.
- **Formato YAML**: Rejeitado nesta fase pois o requisito especificou expressamente o formato JSON e a biblioteca padrão nativa do .NET atende integralmente.

---

## 3. Terminal UX com Spectre.Console e Fallback Não-Interativo

### Decisão
Utilizar **`Spectre.Console`** para a camada de apresentação do CLI:
1. **Modo Interativo (Console TTY padrão)**:
   - Quando `nxtl` for invocado sem parâmetros, renderiza uma tabela rica estilizada com as tarefas (Nome, Tipo, Comando, Descrição) e exibe um prompt interativo `SelectionPrompt<ScriptTask>` navegável pelas setas do teclado (`Up`/`Down`).
2. **Modo Direto / Automação (`--run <tarefa>` ou `-r <tarefa>`)**:
   - Localiza imediatamente a tarefa pelo nome (case-insensitive) e a seleciona para execução sem travar no prompt interativo.
3. **Modo Fallback Não-Interativo (Pipelines / CI/CD)**:
   - Se `Console.IsInputRedirected` ou o ambiente não suportar leitura interativa de teclado, exibe a tabela formatada estática das tarefas e finaliza com código 0, orientando o uso de `--run <nome>` para execuções automatizadas.

### Justificativa
- Alinhamento total ao Princípio I da Constituição (`nxtl`), que estipula o uso de `Spectre.Console` como o único framework de UI interativa no terminal.
- Garante robustez contra exceções em ambientes de CI/CD (onde tentar ler teclas em streams redirecionados causaria quebra da aplicação).
- Todas as mensagens, cabeçalhos, dicas e avisos são emitidos em `pt-BR`.

### Alternativas Consideradas
- **System.CommandLine nativo**: Rejeitado como biblioteca externa para UI; o parsing de argumentos simples (`--config`, `--run`, `--init`, etc.) pode ser feito de forma limpa e direta ou via CLI minimalista sem adicionar frameworks pesados.
- **Apenas tabela estática**: Rejeitado pois o usuário solicitou especificamente a navegação com as teclas de seta (Up/Down).

---

## 4. Tipificação de Tarefas (`tool` vs `csharp`)

### Decisão
O campo `type` é categorizado como:
- `tool` (padrão): Ferramentas CLI externas e comandos do sistema operacional (ex: `dotnet build`, `npm test`, `git status`).
- `csharp`: Arquivos de código C# executáveis em arquivo único / file-scoped.
  - Se `command` apontar para um arquivo `.cs` (ex: `scripts/seed.cs`), o resolvedor infere o comando como `dotnet run scripts/seed.cs`.
  - Se `command` já contiver argumentos ou a instrução completa (ex: `dotnet run --project scripts/seed.cs --verbose`), a instrução é preservada integralmente.

### Justificativa
- Cumpre o acordo firmado na sessão de clarificação (Opção C - híbrido/flexível).
- Oferece extrema ergonomia para desenvolvedores .NET que mantêm scripts C# no repositório.

### Alternativas Consideradas
- **Inferência automática por extensão de arquivo apenas**: Descartada pois o tipo explícito no JSON permite maior clareza visual e categorização na tabela.

---

## 5. Compilação Independente com Native AOT (Self-Contained) e Distribuição

### Decisão
Configurar o projeto `src/NetTaskList/NetTaskList.csproj` para compilação **Native AOT** (`<PublishAot>true</PublishAot>`) e geração de binários independentes e autocontidos (*self-contained*):
1. **Compilação Nativa AOT**:
   - `<PublishAot>true</PublishAot>` habilitado no `.csproj`.
   - Gera um executável binário nativo único para o sistema operacional alvo (ex: `nxtl.exe` em Windows, `nxtl` em Linux/macOS).
   - Não requer a instalação prévia do .NET Runtime ou do .NET SDK na máquina do usuário final.
   - Tempo de inicialização quase instantâneo (< 15 milissegundos) e consumo de memória significativamente reduzido.
2. **Serialização Segura para AOT (Source Generators)**:
   - Uso obrigatório de **C# Source Generators** do `System.Text.Json` via `JsonSerializerContext` (`TaskJsonContext`).
   - Elimina qualquer uso de reflexão dinâmica em tempo de execução, garantindo total compatibilidade com o corte de código (*trimming*) e análise estática do AOT.
3. **Distribuição e Empacotamento**:
   - Geração de binários nativos autônomos via `dotnet publish -c Release -r <rid> --self-contained`.
   - Manutenção de metadados de distribuição NuGet e compatibilidade com ferramentas globais .NET.

### Justificativa
- Atende à solicitação direta do usuário para que o executável seja independente, self-contained e compilado com Native AOT.
- Elimina a barreira de entrada para usuários finais e máquinas de CI/CD que não possuam o runtime do .NET 10 instalado.
- O .NET 10 traz otimizações de ponta no compilador AOT, gerando binários mais enxutos e de altíssimo desempenho.

### Alternativas Consideradas
- **Framework-Dependent JIT**: Descartado por exigir a instalação do .NET 10 Runtime na máquina do usuário e apresentar latência de inicialização inicial (JIT warmup).
- **Self-Contained com JIT**: Descartado pois embute a máquina virtual completa (.NET runtime DLLs) resultando em binários maiores e tempo de inicialização superior ao do Native AOT.

---

## 6. Estrutura do Repositório e Separação de Código

### Decisão
Organizar o código-fonte rigorosamente nas pastas `src/` e `tests/`:
- `src/NetTaskList/`: Projeto principal da ferramenta CLI.
  - `Domain/`: Modelos de dados (`ScriptTask`, `TaskType`, `TaskConfiguration`), regras de validação.
  - `Infrastructure/`: Leitura e parsing de JSON (`JsonConfigurationReader`), resolução de caminhos, abstração de arquivos.
  - `Presentation/`: Renderização de tabelas e menus com Spectre.Console, tratamento de argumentos CLI.
- `tests/NetTaskList.Tests/`: Projeto xUnit contendo suíte completa de testes unitários para o leitor de JSON, validação de duplicidade, inferência de comandos e argumentos CLI.

### Justificativa
- Cumpre integralmente a Constituição v1.1.0 ratificada, mantendo a raiz limpa de arquivos de projeto e assegurando testabilidade desacoplada.
