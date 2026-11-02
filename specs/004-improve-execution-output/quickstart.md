# Quickstart & Validation Guide: Melhoria do Output de Execução e Navegação de Tarefas

**Feature**: `004-improve-execution-output` | **Date**: 2026-11-01

Este guia fornece cenários práticos e comandos executáveis para validar ponta a ponta a nova experiência de output e seleção interativa do `nxtl`.

---

## 1. Pré-Requisitos

- **.NET 10 SDK** instalado no ambiente.
- Terminal com suporte a saída ANSI e modo interativo.
- Arquivo `nxtl.json` com múltiplas tarefas para validação de paginação.

---

## 2. Cenários de Validação

### Cenário 1: Ausência da Tabela e Presença do Cabeçalho de Versão

**Objetivo**: Confirmar que a tabela estática antiga foi removida e o cabeçalho de versão aparece em verde.

1. **Configuração de Teste (`nxtl.json`)**:
   ```json
   {
     "scripts": [
       { "name": "build", "command": "echo build", "type": "tool", "description": "Compila o projeto" },
       { "name": "test", "command": "echo test", "type": "tool", "description": "Executa testes" }
     ]
   }
   ```

2. **Comando de Execução**:
   ```bash
   dotnet run --project src/NetTaskList
   ```

3. **Verificação Visual**:
   - A linha inicial deve exibir: `nxtl versão 1.0.0` formatada em verde.
   - O texto `"Tarefas Disponíveis (nxtl)"` **não** deve aparecer.
   - Nenhuma tabela com colunas `Nome`, `Tipo`, `Comando`, `Descrição` deve ser desenhada.

---

### Cenário 2: Validação do Viewport de 8 Tarefas e Título Ciano

**Objetivo**: Validar que listas com mais de 8 tarefas respeitam o limite de 8 itens visíveis simultaneamente e exibem o título em ciano.

1. **Configuração com 10 Tarefas (`nxtl.json`)**:
   ```json
   {
     "scripts": [
       { "name": "task-01", "command": "echo 1", "type": "tool" },
       { "name": "task-02", "command": "echo 2", "type": "tool" },
       { "name": "task-03", "command": "echo 3", "type": "tool" },
       { "name": "task-04", "command": "echo 4", "type": "tool" },
       { "name": "task-05", "command": "echo 5", "type": "tool" },
       { "name": "task-06", "command": "echo 6", "type": "tool" },
       { "name": "task-07", "command": "echo 7", "type": "tool" },
       { "name": "task-08", "command": "echo 8", "type": "tool" },
       { "name": "task-09", "command": "echo 9", "type": "tool" },
       { "name": "task-10", "command": "echo 10", "type": "tool" }
     ]
   }
   ```

2. **Comando de Execução**:
   ```bash
   dotnet run --project src/NetTaskList
   ```

3. **Verificação Visual**:
   - O título exibido deve ser `Selecionar uma tarefa para executar` na cor ciano.
   - Exatamente 8 escolhas aparecem inicialmente na tela.
   - O texto cinza `(Mova para cima e para baixo para ver mais opções)` é exibido abaixo da listagem.
   - Ao pressionar a seta para baixo até a 9ª opção, a lista rola suavemente, mantendo o viewport fixo em 8 itens.

---

### Cenário 3: Validação da Suíte de Testes Automatizados

**Objetivo**: Assegurar 100% de aprovação nos testes automatizados que cobrem as novas asserções de apresentação.

1. **Comando de Execução**:
   ```bash
   dotnet test
   ```

2. **Resultado Esperado**:
   - Todos os testes unitários em `tests/NetTaskList.Tests/Presentation/` devem passar com sucesso.
