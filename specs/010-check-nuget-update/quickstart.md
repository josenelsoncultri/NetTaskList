# Quickstart & Validation Guide: Verificação de Nova Versão no NuGet

**Feature**: Verificação de Nova Versão no NuGet com Alerta no Console  
**Branch**: `010-check-nuget-update`  
**Date**: 2026-11-11

Este guia descreve os cenários de teste e validação para confirmar o funcionamento da checagem de novas versões e a exibição do alerta no console.

---

## Pré-requisitos

- .NET 10 SDK instalado.
- Projeto compilado com êxito (`dotnet build`).

---

## Cenário 1: Execução Interativa com Nova Versão Disponível

### 1.1 Configuração
Simular o registro NuGet retornando uma versão superior à versão local instalada (ex: local `1.6.1` e remoto `1.7.0`).

### 1.2 Execução
```bash
dotnet run --project src/NetTaskList
```

### 1.3 Resultado Esperado
- No topo do console, é exibido o painel com borda amarela arredondada:
  - Texto: `Nova versão disponível: 1.6.1 → 1.7.0`
  - Instrução: `Para atualizar a ferramenta globalmente, execute:`
  - Comando: `dotnet tool update -g nettasklist`
- Logo abaixo do painel, o aplicativo exibe o cabeçalho normal `nxtl versão 1.6.1` e abre o menu de seleção interativa de tarefas.

---

## Cenário 2: Execução Direta (`--run`) com Nova Versão Disponível

### 2.1 Configuração
Com uma tarefa configurada (ex: `build`) no `nxtl.json` e versão mais recente disponível no NuGet.

### 2.2 Execução
```bash
dotnet run --project src/NetTaskList -- --run build
```

### 2.3 Resultado Esperado
- O painel de alerta de atualização é exibido no topo do console antes de qualquer outra linha de saída.
- Em seguida, os comandos da tarefa `build` são executados normalmente.

---

## Cenário 3: Ferramenta Já Atualizada na Versão Mais Recente

### 3.1 Configuração
A versão local instalada é igual à versão máxima estável disponível no NuGet (ex: `1.6.1`).

### 3.2 Execução
```bash
dotnet run --project src/NetTaskList
```

### 3.3 Resultado Esperado
- Nenhum painel de alerta de versão é exibido.
- O terminal exibe diretamente o cabeçalho e a lista de tarefas.

---

## Cenário 4: Ambiente Offline ou com Falha de Conexão

### 4.1 Configuração
Desconectar a máquina da internet ou bloquear requisições para `api.nuget.org`.

### 4.2 Execução
```bash
dotnet run --project src/NetTaskList
```

### 4.3 Resultado Esperado
- A consulta ao NuGet expira silenciosamente (em até 2 segundos) ou falha de forma transparente.
- Nenhum erro técnico, exceção ou stack trace é impresso no console.
- Nenhum painel de alerta é exibido.
- O aplicativo abre a lista de tarefas normalmente.

---

## Cenário 5: Verificação de Versão Limpa (`--version`)

### 5.1 Execução
```bash
dotnet run --project src/NetTaskList -- --version
```

### 5.2 Resultado Esperado
- Saída limpa contendo apenas a versão instalada: `nxtl versão 1.6.1`.
- Nenhuma chamada de rede de verificação é realizada e nenhum painel de alerta é exibido.
