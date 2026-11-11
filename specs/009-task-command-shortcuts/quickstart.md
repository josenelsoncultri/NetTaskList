# Quickstart & Validation Guide: Task Command Shortcuts

**Feature**: Task Command Shortcuts and Recursion Detection in `nxtl.json`  
**Branch**: `009-task-command-shortcuts`  
**Date**: 2026-11-10

Este guia descreve os cenários de teste e validação ponta a ponta para verificar o suporte a atalhos de tarefas e detecção de recursão no `nxtl`.

---

## Pré-requisitos

- .NET 10 SDK instalado.
- O projeto `NetTaskList` compilado (`dotnet build`).

---

## Cenário 1: Atalho de Comando Único

### 1.1 Configuração (`nxtl.json`)
```json
{
  "scripts": [
    {
      "name": "say-hello",
      "command": "echo Olá Mundo",
      "type": "tool"
    },
    {
      "name": "cumprimentar",
      "command": { "task": "say-hello" },
      "type": "tool",
      "description": "Atalho em português para say-hello"
    }
  ]
}
```

### 1.2 Execução
```bash
dotnet run --project src/NetTaskList -- --run cumprimentar
```

### 1.3 Resultado Esperado
- O terminal exibe `Executando atalho: say-hello`.
- O comando `echo Olá Mundo` é executado com sucesso.
- O código de saída retornado é `0`.

---

## Cenário 2: Sequência Mista com Múltiplos Atalhos e Comandos Shell

### 2.1 Configuração (`nxtl.json`)
```json
{
  "scripts": [
    {
      "name": "step-clean",
      "command": "echo Limpando artefatos",
      "type": "tool"
    },
    {
      "name": "step-build",
      "command": "echo Compilando binarios",
      "type": "tool"
    },
    {
      "name": "pipeline",
      "command": [
        "echo [Inicio]",
        { "task": "step-clean" },
        { "task": "step-build" },
        "echo [Fim]"
      ],
      "type": "tool"
    }
  ]
}
```

### 2.2 Execução
```bash
dotnet run --project src/NetTaskList -- --run pipeline
```

### 2.3 Resultado Esperado
- `Comando [1/4]: echo [Inicio]` é executado.
- `Executando atalho: step-clean` é exibido e `echo Limpando artefatos` executa.
- `Executando atalho: step-build` é exibido e `echo Compilando binarios` executa.
- `Comando [4/4]: echo [Fim]` é executado.
- Código de saída `0`.

---

## Cenário 3: Detecção de Recursão em Ciclo Direto (2 tarefas)

### 3.1 Configuração (`nxtl.json`)
```json
{
  "scripts": [
    {
      "name": "build",
      "command": { "task": "clean" },
      "type": "tool"
    },
    {
      "name": "clean",
      "command": { "task": "build" },
      "type": "tool"
    }
  ]
}
```

### 3.2 Execução
```bash
dotnet run --project src/NetTaskList -- --run build
```

### 3.3 Resultado Esperado
- A validação prévia detecta o ciclo antes de executar qualquer processo.
- Mensagem de erro exibida:
  `Erro: Referência circular detectada entre tarefas: build -> clean -> build. A execução causaria um loop infinito no nxtl.`
- Nenhum comando shell é disparado.
- Código de saída é `1`.

---

## Cenário 4: Detecção de Recursão em Árvore Profunda

### 4.1 Configuração (`nxtl.json`)
```json
{
  "scripts": [
    {
      "name": "task-a",
      "command": { "task": "task-b" },
      "type": "tool"
    },
    {
      "name": "task-b",
      "command": { "task": "task-c" },
      "type": "tool"
    },
    {
      "name": "task-c",
      "command": { "task": "task-a" },
      "type": "tool"
    }
  ]
}
```

### 4.2 Execução
```bash
dotnet run --project src/NetTaskList -- --run task-a
```

### 4.3 Resultado Esperado
- Mensagem de erro:
  `Erro: Referência circular detectada entre tarefas: task-a -> task-b -> task-c -> task-a. A execução causaria um loop infinito no nxtl.`
- Código de saída `1`.

---

## Cenário 5: Grafo Acíclico em Diamante (Válido / Sem Falso Positivo)

### 5.1 Configuração (`nxtl.json`)
```json
{
  "scripts": [
    {
      "name": "shared-seed",
      "command": "echo preparando banco",
      "type": "tool"
    },
    {
      "name": "unit-tests",
      "command": [
        { "task": "shared-seed" },
        "echo rodando testes unitarios"
      ],
      "type": "tool"
    },
    {
      "name": "integration-tests",
      "command": [
        { "task": "shared-seed" },
        "echo rodando testes integrados"
      ],
      "type": "tool"
    },
    {
      "name": "test-all",
      "command": [
        { "task": "unit-tests" },
        { "task": "integration-tests" }
      ],
      "type": "tool"
    }
  ]
}
```

### 5.2 Execução
```bash
dotnet run --project src/NetTaskList -- --run test-all
```

### 5.3 Resultado Esperado
- Validação prévia aprova o grafo sem disparar falsos positivos de ciclo.
- `shared-seed` executa duas vezes (uma vez dentro de `unit-tests` e outra dentro de `integration-tests`).
- Todos os passos concluem com sucesso e código de saída `0`.

---

## Cenário 6: Rejeição de Referência Inexistente e Referência a Tarefa C#

### 6.1 Referência Inexistente
Configuração contendo `{ "task": "fantasma" }`:
```bash
dotnet run --project src/NetTaskList -- --run tarefa-invalida
```
**Resultado Esperado**:
- `Erro: A tarefa referenciada 'fantasma' não foi encontrada no arquivo de configuração.`
- Código de saída `1`.

### 6.2 Referência a Script C#
Configuração onde uma tarefa `tool` possui `{ "task": "meu-script-cs" }` (com `type: "csharp"`):
```bash
dotnet run --project src/NetTaskList -- --run chamar-csharp
```
**Resultado Esperado**:
- `Erro: A tarefa 'chamar-csharp' tenta referenciar 'meu-script-cs', que é um script C#. Tarefas C# não podem ser referenciadas como atalhos.`
- Código de saída `1`.
