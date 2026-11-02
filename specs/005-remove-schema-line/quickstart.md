# Quickstart: Validação da Remoção da Linha de Schema

**Feature**: `005-remove-schema-line` | **Date**: 2026-11-01

Este guia descreve os passos práticos para validação ponta a ponta da remoção da linha indicativa de schema.

---

## Pré-requisitos

- .NET 10 SDK instalado.
- Repositório compilado: `dotnet build NetTaskList.sln`.

---

## Cenário 1: Validação do Comando de Inicialização (`nxtl --init`)

### Objetivo
Confirmar que novos arquivos `nxtl.json` são gerados sem a linha `"$schema"`.

### Passos
1. Criar um diretório temporário isolado:
   ```bash
   mkdir temp-nxtl-test
   cd temp-nxtl-test
   ```
2. Executar a inicialização via `nxtl`:
   ```bash
   dotnet run --project src/NetTaskList/NetTaskList.csproj -- --init
   ```
3. Inspecionar o conteúdo de `nxtl.json`:
   - Verificar se o arquivo foi criado.
   - Confirmar que **NÃO** existe menção a `"$schema"` ou `nxtl-v1.schema.json`.
   - Confirmar que as tarefas padrão (`build`, `test`, `seed`) estão presentes.
4. Executar o `nxtl` no diretório:
   ```bash
   dotnet run --project src/NetTaskList/NetTaskList.csproj -- --run test
   ```
   - O comando deve carregar o arquivo sem erros e acionar a tarefa solicitada.

---

## Cenário 2: Compatibilidade com Configuração Legada (Com `"$schema"`)

### Objetivo
Garantir que arquivos existentes que ainda possuam `"$schema"` continuem funcionando sem advertências ou erros.

### Passos
1. No diretório de testes, criar um arquivo `nxtl.json` simulando uma versão anterior:
   ```json
   {
     "$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
     "scripts": [
       { "name": "echo-test", "command": "echo Hello from legacy" }
     ]
   }
   ```
2. Executar:
   ```bash
   dotnet run --project src/NetTaskList/NetTaskList.csproj -- --run echo-test
   ```
3. Resultado esperado:
   - Execução bem-sucedida da tarefa `echo-test`.

---

## Cenário 3: Validação do Arquivo `nxtl.json` da Raiz do Projeto

### Objetivo
Assegurar que o arquivo `nxtl.json` na raiz de desenvolvimento do `NetTaskList` foi limpo.

### Passos
1. Abrir `nxtl.json` na raiz do repositório.
2. Confirmar a ausência da linha contendo `"$schema"`.
3. Executar o comando no repositório:
   ```bash
   dotnet run --project src/NetTaskList/NetTaskList.csproj -- -h
   ```
   - O CLI deve funcionar normalmente.

---

## Cenário 4: Execução dos Testes Automatizados

### Objetivo
Validar que todos os testes unitários cobrem o novo comportamento e asseguram ausência de regressões.

### Passos
```bash
dotnet test --no-restore
```

### Resultado Esperado
- 100% dos testes aprovados, incluindo testes de ausência de schema no gerador e leitura de arquivos legados com schema.
