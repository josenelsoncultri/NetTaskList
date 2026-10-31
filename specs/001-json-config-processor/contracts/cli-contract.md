# CLI Interface Contract: `nxtl`

**Feature**: `001-json-config-processor`  
**Date**: 2026-10-30  
**Status**: Ready  

Este documento define o contrato de interface de linha de comando (CLI) do executável `nxtl`, detalhando comandos, opções, saídas no terminal e códigos de retorno.

---

## 1. Comando Executável e Visão Geral

- **Nome da ferramenta**: `nxtl` (binário nativo compilado com Native AOT ou instalável via `dotnet tool install -g NetTaskList`)
- **Compilação e Execução**: Binário nativo independente (*self-contained*), compilado via Native AOT no .NET 10, sem dependência de runtime externo instalado na máquina
- **Performance de Inicialização**: Sub-15 milissegundos (sem custo de JIT warmup)
- **Linguagem de saída**: Português Brasileiro (`pt-BR`)
- **Framework de interface**: `Spectre.Console`

---

## 2. Sintaxe de Invocação

```bash
nxtl [opções]
```

### Opções e Parâmetros

| Opção | Alias | Descrição | Exemplo |
|-------|-------|-----------|---------|
| `--config <caminho>` | `-c <caminho>` | Especifica um caminho personalizado para o arquivo de configuração JSON. | `nxtl -c configs/tarefas.json` |
| `--run <nome>` | `-r <nome>` | Seleciona e direciona para execução direta uma tarefa específica sem prompt interativo. | `nxtl --run build` |
| `--init` | `-i` | Gera um arquivo modelo padrão `nxtl.json` com exemplos práticos no diretório atual. | `nxtl --init` |
| `--help` | `-h`, `-?` | Exibe a mensagem de ajuda com todas as opções em português. | `nxtl --help` |
| `--version` | `-v` | Exibe a versão instalada da ferramenta. | `nxtl --version` |

---

## 3. Modos de Operação

### 3.1 Modo Padrão: Navegação Interativa
Acionado ao executar `nxtl` sem argumentos adicionais em um terminal interativo (TTY):
1. O sistema procura `nxtl.json` no diretório atual (ou no caminho especificado via `--config`).
2. Se válido:
   - Renderiza uma tabela estilizada com as tarefas:
     - **Coluna Nome**: Exibida em negrito com cor distintiva.
     - **Coluna Tipo**: Badge colorido (`[cyan]tool[/]` ou `[green]csharp[/]`).
     - **Coluna Comando**: Instrução a ser executada formatada em cinza/amarelo.
     - **Coluna Descrição**: Resumo do propósito da tarefa.
   - Apresenta o prompt interativo `SelectionPrompt` permitindo navegar com as setas `[Up]` e `[Down]` e selecionar com `[Enter]`.
3. Ao pressionar `[Enter]`, a tarefa selecionada é confirmada na tela com mensagem diagnóstica.
4. Para sair sem selecionar, o usuário pode pressionar `Ctrl+C` ou selecionar a opção `[Sair]`.

---

### 3.2 Modo Direto / Automação (`--run <nome>`)
Acionado ao fornecer a flag `--run <nome>`:
1. Carrega a configuração JSON.
2. Realiza busca case-insensitive pelo nome da tarefa.
3. Se encontrada:
   - Exibe a confirmação visual da tarefa identificada:
     ```text
     Tarefa selecionada: build [tool]
     Comando: dotnet build
     ```
   - Retorna código de saída `0`.
4. Se não encontrada:
   - Exibe mensagem de erro:
     ```text
     Erro: Tarefa 'xyz' não encontrada no arquivo de configuração 'nxtl.json'.
     Execute 'nxtl' para listar todas as tarefas disponíveis.
     ```
   - Retorna código de saída `1`.

---

### 3.3 Modo Fallback Não-Interativo
Acionado automaticamente quando `Console.IsInputRedirected` for verdadeiro (ex: pipes, scripts automatizados, CI/CD):
1. Carrega e valida o arquivo de configuração.
2. Renderiza a tabela estática com as tarefas no `stdout`.
3. Emite instrução informando que a execução interativa não está disponível em consoles redirecionados e orienta o uso de `nxtl --run <nome>`.
4. Retorna código de saída `0`.

---

### 3.4 Modo Inicialização (`--init`)
Acionado ao fornecer a flag `--init`:
1. Verifica se já existe um arquivo `nxtl.json` no diretório atual.
2. Se já existir:
   - Emite alerta informando que o arquivo já existe e não será sobrescrito acidentalmente.
   - Retorna código `1`.
3. Se não existir:
   - Cria o arquivo `nxtl.json` estruturado, contendo o esquema e tarefas de exemplo (`build`, `test`, `script-csharp`).
   - Retorna mensagem de sucesso e código `0`.

---

## 4. Códigos de Saída (Exit Codes)

| Código | Significado |
|--------|-------------|
| `0` | Sucesso na operação (listagem, seleção de tarefa, inicialização do template ou exibição de ajuda). |
| `1` | Erro funcional ou de validação (arquivo não encontrado, sintaxe JSON inválida, nomes duplicados, tarefa não localizada via `--run`). |
| `2` | Erro de argumentos de linha de comando (opção inválida ou argumento ausente). |
| `130` | Execução interrompida pelo usuário via `Ctrl+C`. |
