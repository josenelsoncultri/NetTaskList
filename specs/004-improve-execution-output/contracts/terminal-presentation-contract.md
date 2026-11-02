# Terminal Presentation Contract: Interface do Usuário e Output do Terminal

**Feature**: `004-improve-execution-output` | **Date**: 2026-11-01

Este contrato formaliza as especificações de renderização, sequências de controle ANSI, textos literais e formatação de saída que o utilitário `nxtl` expõe no terminal.

---

## 1. Contrato do Cabeçalho da Listagem (Listing Header)

### 1.1 Formato e Posicionamento
- **Localização**: Primeira linha de saída emitida pelo `nxtl` após carregamento bem-sucedido de tarefas na execução sem parâmetros.
- **Padrão de Texto**: `nxtl versão <versão>`
- **Cor**: Verde ANSI (`ConsoleColor.Green` / Spectre markup `[green]`).
- **Exemplo de Saída**:
  ```text
  nxtl versão 1.0.0
  ```

### 1.2 Regras de Formatação
- O nome da ferramenta (`nxtl`) e o texto descritivo (`versão`) devem estar na mesma linha que o número de versão.
- A cor verde deve ser aplicada a todo o segmento ou ao identificador da versão de forma contínua.

---

## 2. Contrato do Menu Interativo de Seleção (Selection Prompt)

### 2.1 Título da Seleção
- **Texto**: `Selecionar uma tarefa para executar`
- **Cor**: Ciano ANSI (`ConsoleColor.Cyan` / Spectre markup `[cyan]`).
- **Pontuação**: Sem dois pontos (`:`) finais.

### 2.2 Viewport e Paginação
- **Tamanho da Janela (PageSize)**: Exatamente `8` escolhas simultâneas visíveis.
- **Texto de Paginação (`MoreChoicesText`)**:
  ```text
  (Mova para cima e para baixo para ver mais opções)
  ```
  Estilizado em cinza (`[grey]`).

### 2.3 Opção de Saída
- A última opção na lista deve permanecer como `[Sair]`, tratada de forma literal com escape de colchetes no Spectre.Console para prevenir exceções de formatação.

---

## 3. Contrato de Remoção da Tabela Estática

### 3.1 Proibição de Renderização
- O bloco de tabela com bordas retangulares/arredondadas (`TableBorder.Rounded`) contendo as colunas `Nome`, `Tipo`, `Comando` e `Descrição` **NÃO DEVE** ser impresso durante a execução do comando `nxtl`.
- A cadeia de caracteres `"Tarefas Disponíveis (nxtl)"` **NÃO DEVE** constar em nenhum ponto da saída de execução do terminal.

---

## 4. Contrato de Ambiente Não-Interativo (Fallback)

### 4.1 Comportamento em Redirecionamento (Pipes / CI)
Quando a entrada do console estiver redirecionada (`Console.IsInputRedirected == true`):
1. O cabeçalho de versão em verde (`nxtl versão <versão>`) é emitido normalmente.
2. A tabela estática **NÃO** é renderizada.
3. É emitida a mensagem de aviso em amarelo:
   ```text
   Aviso: Execução interativa não disponível em console redirecionado. Utilize 'nxtl --run <nome>' para executar uma tarefa diretamente.
   ```
4. A aplicação finaliza com código de saída `0`.
