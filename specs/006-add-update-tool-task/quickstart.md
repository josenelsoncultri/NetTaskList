# Quickstart: Validação da Tarefa update-nettasklist no Template Padrão

**Feature**: `006-add-update-tool-task` | **Date**: 2026-11-01

Este guia detalha o procedimento para validação prática da tarefa de atualização no template gerado.

---

## Pré-requisitos
- .NET 10 SDK instalado.
- Repositório compilado: `dotnet build NetTaskList.sln`.

---

## Cenário 1: Validação do Arquivo Gerado por `nxtl --init`

### Passos
1. Em um diretório temporário isolado:
   ```bash
   mkdir temp-template-test
   cd temp-template-test
   ```
2. Executar a inicialização:
   ```bash
   dotnet run --project src/NetTaskList/NetTaskList.csproj -- --init
   ```
3. Inspecionar o arquivo `nxtl.json`:
   - Verificar a presença da tarefa `"name": "update-nettasklist"`.
   - Verificar o comando `"command": "dotnet tool update -g nettasklist"`.
   - Verificar a descrição `"description": "Atualiza a ferramenta nxtl de modo global"`.
   - Confirmar o total de 4 tarefas configuradas.
4. Testar a listagem:
   ```bash
   dotnet run --project src/NetTaskList/NetTaskList.csproj -- -h
   ```

---

## Cenário 2: Validação Automatizada de Testes Unitários

### Passos
```bash
dotnet test --no-restore
```

### Resultado Esperado
- Testes de `TemplateConfigurationGeneratorTests` confirmam 4 tarefas e a presença de `update-nettasklist`.
- 100% dos testes da suíte aprovados sem regressões.
