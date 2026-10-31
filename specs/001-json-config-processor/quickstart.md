# Quickstart & Validation Guide: Processador de Configuração JSON

**Feature**: `001-json-config-processor`  
**Date**: 2026-10-30  
**Status**: Ready  

Este guia fornece cenários de validação executáveis ponta a ponta para comprovar o funcionamento correto do leitor e processador de configurações JSON da ferramenta `nxtl`.

---

## 1. Pré-Requisitos

- **.NET 10 SDK** (v10.0.x) instalado na máquina.
- Terminal compatível com caracteres UTF-8 e ANSI (PowerShell 7+, Windows Terminal, bash, zsh).

---

## 2. Cenários de Validação Ponta a Ponta

### Cenário 1: Inicialização do Arquivo de Configuração Padrão (`--init`)

**Objetivo**: Validar a geração de um arquivo de configuração `nxtl.json` inicial e legível por editores.

1. **Executar comando no diretório de teste**:
   ```bash
   dotnet run --project src/NetTaskList -- --init
   ```

2. **Resultado Esperado**:
   - Arquivo `nxtl.json` é criado na raiz do diretório.
   - Mensagem de sucesso no terminal:
     ```text
     ✔ Arquivo de configuração padrão 'nxtl.json' criado com sucesso!
     ```
   - O arquivo criado valida contra o esquema [config-schema.json](contracts/config-schema.json).

---

### Cenário 2: Descoberta e Listagem Interativa de Tarefas (`nxtl`)

**Objetivo**: Validar a leitura e exibição das tarefas na tabela rica com Spectre.Console e navegação interativa por setas.

1. **Configurar `nxtl.json` com múltiplas tarefas**:
   ```json
   {
     "$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
     "scripts": [
       {
         "name": "build",
         "command": "dotnet build",
         "type": "tool",
         "description": "Compila os projetos da solução"
       },
       {
         "name": "test",
         "command": "dotnet test",
         "type": "tool",
         "description": "Executa todos os testes unitários"
       },
       {
         "name": "seed",
         "command": "scripts/seed.cs",
         "type": "csharp",
         "description": "Script C# file-scoped para popular dados de teste"
       }
     ]
   }
   ```

2. **Executar a CLI diretamente sem argumentos**:
   ```bash
   dotnet run --project src/NetTaskList
   ```

3. **Resultado Esperado**:
   - Renderização de tabela colorida detalhando Nome, Tipo (`tool` em ciano, `csharp` em verde), Comando e Descrição.
   - Menu interativo com as opções disponíveis, permitindo navegar com as setas `[Up]` e `[Down]`.
   - Tempo de resposta e renderização em menos de 300ms.

---

### Cenário 3: Seleção Direta de Tarefa via Parâmetro (`--run`)

**Objetivo**: Validar a execução direta sem prompt interativo (modo ideal para CI/CD e automações).

1. **Executar comando com a flag `--run`**:
   ```bash
   dotnet run --project src/NetTaskList -- --run build
   ```

2. **Resultado Esperado**:
   - Identificação imediata da tarefa sem travar no terminal:
     ```text
     ✔ Tarefa selecionada: build [tool]
     Comando: dotnet build
     ```
   - Código de retorno `0`.

---

### Cenário 4: Intercepção de Nomes Duplicados (Validação Semântica)

**Objetivo**: Comprovar que o processador rejeita configurações com tarefas de nomes idênticos (case-insensitive).

1. **Configurar `nxtl.json` com colisão de nomes**:
   ```json
   {
     "scripts": [
       { "name": "Build", "command": "dotnet build" },
       { "name": "build", "command": "dotnet build --no-restore" }
     ]
   }
   ```

2. **Executar a ferramenta**:
   ```bash
   dotnet run --project src/NetTaskList
   ```

3. **Resultado Esperado**:
   - Operação abortada antes de qualquer execução.
   - Mensagem de erro em português brasileiro (`pt-BR`):
     ```text
     ✖ Erro de validação: O script com o nome 'build' está duplicado no arquivo de configuração. Cada script deve possuir um nome único.
     ```
   - Código de retorno `1`.

---

### Cenário 5: Tratamento de Arquivo Ausente

**Objetivo**: Validar a experiência de erro amigável quando nenhum `nxtl.json` existir no diretório.

1. **Executar em uma pasta vazia**:
   ```bash
   dotnet run --project src/NetTaskList
   ```

2. **Resultado Esperado**:
   - Mensagem explicativa amigável em português:
     ```text
     ✖ Nenhum arquivo de configuração 'nxtl.json' foi encontrado no diretório atual.
     Dica: Execute 'nxtl --init' para criar um arquivo de configuração de exemplo ou utilize '--config <caminho>' para indicar outro local.
     ```
   - Código de retorno `1`.

---

### Cenário 6: Empacotamento NuGet como Ferramenta Global

**Objetivo**: Validar a geração correta do pacote NuGet (`.nupkg`) como .NET Global Tool.

1. **Executar comando de empacotamento**:
   ```bash
   dotnet pack src/NetTaskList/NetTaskList.csproj -c Release
   ```

2. **Resultado Esperado**:
   - O arquivo `NetTaskList.0.1.0.nupkg` é gerado em `src/NetTaskList/bin/Release/`.
   - O pacote contém o manifesto de ferramenta global com o comando executável `nxtl`.

---

### Cenário 7: Compilação e Execução de Binário Nativo Independente (Native AOT)

**Objetivo**: Validar a geração de binário único nativo, independente e *self-contained* através do compilador Native AOT do .NET 10.

1. **Executar publicação Native AOT para a plataforma atual (ex: win-x64)**:
   ```bash
   dotnet publish src/NetTaskList/NetTaskList.csproj -c Release -r win-x64 --self-contained
   ```

2. **Executar diretamente o executável nativo standalone**:
   ```bash
   ./src/NetTaskList/bin/Release/net10.0/win-x64/publish/nxtl.exe --help
   ```

3. **Resultado Esperado**:
   - Binário único independente gerado em `publish/` (ex: `nxtl.exe` no Windows ou `nxtl` no Linux).
   - Execução quase instantânea (< 15 milissegundos) sem necessitar do SDK ou Runtime do .NET instalado no ambiente de destino.
   - Zero avisos de quebra de reflexão ou corte de código (*trimming warnings*) durante a compilação, comprovando a eficácia do `TaskJsonContext` (Source Generator).
