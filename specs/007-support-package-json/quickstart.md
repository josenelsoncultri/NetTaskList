# Quickstart: Suporte ao Arquivo package.json (package.json Support)

## Overview
Este guia descreve cenários executáveis passo a passo para validação manual ou automatizada da funcionalidade de leitura, precedência e execução de tarefas do `package.json` no `nxtl`.

---

## Pré-requisitos
- .NET 10 SDK instalado.
- Projeto compilado sem avisos (`dotnet build`).

---

## Cenários de Validação

### Cenário 1: Execução em Projeto com apenas `package.json`
1. **Configuração**:
   Crie um diretório de teste contendo apenas `package.json`:
   ```json
   {
     "name": "sample-node-project",
     "scripts": {
       "start": "echo Starting Application",
       "test": "echo Running Tests"
     }
   }
   ```
2. **Execução**:
   Execute `nxtl --run test` dentro desse diretório.
3. **Resultado Esperado**:
   - O `nxtl` detecta `package.json`.
   - Executa `echo Running Tests`.
   - Código de saída: `0`.

---

### Cenário 2: Precedência de `nxtl.json` sobre `package.json`
1. **Configuração**:
   No mesmo diretório do Cenário 1, crie também um `nxtl.json`:
   ```json
   {
     "scripts": [
       {
         "name": "test",
         "command": "echo Running from nxtl.json"
       }
     ]
   }
   ```
2. **Execução**:
   Execute `nxtl --run test`.
3. **Resultado Esperado**:
   - A saída deve exibir `Running from nxtl.json`.
   - O `package.json` é 100% ignorado.

---

### Cenário 3: Ciclo de Vida npm (`pre` e `post`)
1. **Configuração**:
   Crie um diretório contendo apenas `package.json`:
   ```json
   {
     "scripts": {
       "prebuild": "echo [PRE-BUILD]",
       "build": "echo [MAIN-BUILD]",
       "postbuild": "echo [POST-BUILD]"
     }
   }
   ```
2. **Execução**:
   Execute `nxtl --run build`.
3. **Resultado Esperado**:
   - Execução sequencial:
     1. `[PRE-BUILD]`
     2. `[MAIN-BUILD]`
     3. `[POST-BUILD]`
   - Código de saída: `0`.

---

### Cenário 4: Aborto de Cadeia por Falha em `pre<tarefa>`
1. **Configuração**:
   Modifique o `package.json` do Cenário 3 para que `prebuild` falhe (ex: `exit 1` ou comando inexistente).
2. **Execução**:
   Execute `nxtl --run build`.
3. **Resultado Esperado**:
   - O comando `prebuild` falha.
   - `build` e `postbuild` NÃO são executados.
   - O código de saída do `nxtl` é `1` (ou o código de retorno da falha do pré-script).

---

### Cenário 5: Nenhum Arquivo de Configuração Encontrado
1. **Configuração**:
   Em um diretório vazio (sem `nxtl.json` e sem `package.json`).
2. **Execução**:
   Execute `nxtl`.
3. **Resultado Esperado**:
   - Mensagem de erro em português indicando que nenhum dos dois arquivos foi encontrado e sugerindo `nxtl --init`.
   - Código de saída: `1`.

