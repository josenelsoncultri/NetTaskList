# Quickstart: Validação de Comandos Sequenciais em Tarefas

**Branch**: `008-sequential-task-commands` | **Spec**: [spec.md](./spec.md)

Este guia descreve os cenários de validação prática ponta a ponta para comprovar o funcionamento de tarefas configuradas com múltiplos comandos sequenciais e a interrupção em caso de falha.

---

## Pré-requisitos

- .NET 10 SDK instalado.
- Repositório compilado: `dotnet build`.

---

## Cenário 1: Execução Sequencial com Sucesso

### 1. Preparação do arquivo `nxtl.json` de teste:
```json
{
  "scripts": [
    {
      "name": "multi-success",
      "command": [
        "dotnet --version",
        "dotnet --info"
      ],
      "type": "tool",
      "description": "Executa dois comandos do dotnet com sucesso"
    }
  ]
}
```

### 2. Comando de Execução:
```bash
dotnet run --project src/NetTaskList -- --config nxtl.json --run multi-success
```

### 3. Resultado Esperado:
- O terminal exibe:
  - `Tarefa selecionada: multi-success [[tool]]`
  - `Comando [1/2]: dotnet --version`
  - Saída do comando de versão
  - `Comando [2/2]: dotnet --info`
  - Saída das informações do .NET
- O comando finaliza com código de saída `0`.

---

## Cenário 2: Interrupção Imediata em Caso de Falha Intermediária

### 1. Preparação do arquivo `nxtl.json` com falha simulada:
```json
{
  "scripts": [
    {
      "name": "multi-failure",
      "command": [
        "dotnet --version",
        "comando-inexistente-para-gerar-erro",
        "dotnet --info"
      ],
      "type": "tool",
      "description": "Demonstra a parada imediata no segundo comando"
    }
  ]
}
```

### 2. Comando de Execução:
```bash
dotnet run --project src/NetTaskList -- --config nxtl.json --run multi-failure
```

### 3. Resultado Esperado:
- O primeiro comando (`dotnet --version`) executa normalmente.
- O segundo comando falha ao iniciar ou retorna código de saída diferente de zero.
- O terminal exibe a mensagem de interrupção:
  `Execução interrompida: O comando falhou com código de saída <código>. Comandos posteriores não serão executados.`
- O terceiro comando (`dotnet --info`) **NUNCA** é executado.
- O processo do `nxtl` finaliza com o mesmo código de erro do comando que falhou.

---

## Cenário 3: Exibição no Menu Interativo sem Descrição

### 1. Preparação do arquivo `nxtl.json`:
```json
{
  "scripts": [
    {
      "name": "chain-no-desc",
      "command": [
        "echo 1",
        "echo 2"
      ],
      "type": "tool"
    }
  ]
}
```

### 2. Comando de Execução:
```bash
dotnet run --project src/NetTaskList -- --config nxtl.json
```

### 3. Resultado Esperado:
- O menu interativo de seleção exibe a opção no formato:
  `chain-no-desc [tool] - <sem descrição>`

---

## Cenário 4: Validação de Configuração Inválida (Array Vazio ou Itens em Branco)

### 1. Preparação do arquivo `nxtl.json`:
```json
{
  "scripts": [
    {
      "name": "empty-array",
      "command": []
    }
  ]
}
```

### 2. Comando de Execução:
```bash
dotnet run --project src/NetTaskList -- --config nxtl.json
```

### 3. Resultado Esperado:
- O sistema recusa o arquivo antes de qualquer menu ou execução.
- Exibe mensagem em `pt-BR`:
  `A tarefa 'empty-array' não define um comando a ser executado.`
- O comando finaliza com código de saída `1`.

