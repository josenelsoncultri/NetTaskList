# Phase 0 Research: Suporte ao Arquivo package.json (package.json Support)

## Context & Objectives
O objetivo desta pesquisa é definir a arquitetura e os padrões de implementação para capacitar o `nxtl` a descobrir, validar, ler e executar tarefas a partir do arquivo `package.json` de projetos Node.js/JavaScript/TypeScript, funcionando como substituto para a ferramenta `ntl` e respeitando a precedência mandatória do `nxtl.json`.

---

## Decisions

### 1. Descoberta de Arquivo de Configuração e Regra de Precedência

- **Decisão**: Criar um componente de resolução de arquivo (`ConfigurationFileResolver` ou estender o fluxo em `Program.cs`) que avalia os caminhos na seguinte ordem:
  1. Se `--config <caminho>` for fornecido: valida e utiliza estritamente o arquivo indicado.
  2. Caso contrário (auto-descoberta no diretório atual):
     - Verifica se `nxtl.json` existe. Se existir, adota `nxtl.json` como fonte exclusiva.
     - Se `nxtl.json` não existir, verifica se `package.json` existe. Se existir, adota `package.json`.
     - Se nenhum dos dois existir, retorna erro informativo em português (`pt-BR`) sugerindo `nxtl --init`.
- **Racional**: Garante que o `nxtl.json` tenha prevalência absoluta e impede mesclagem ambígua de configurações, atendendo rigorosamente à regra de negócio estipulada.
- **Alternativas consideradas**:
  - *Mesclar scripts de ambos os arquivos*: Rejeitado porque a regra do usuário exige explicitamente que `nxtl.json` prevaleça e `package.json` seja ignorado quando ambos coexistirem.
  - *Tentar adivinhar pelo tipo de projeto*: Rejeitado por adicionar complexidade desnecessária e violar o princípio de KISS/YAGNI.

---

### 2. Desserialização Segura para Native AOT com System.Text.Json

- **Decisão**: Utilizar os geradores de código fonte nativos de `System.Text.Json` (`JsonSerializerContext`) sem depender de reflexão em tempo de execução. Criar o DTO `PackageJsonDto` com a propriedade `scripts` tipada como `Dictionary<string, JsonElement>` ou leitor customizado com `[JsonSerializable(typeof(PackageJsonDto))]` adicionado ao `TaskJsonContext`.
  - Mapear cada entrada do dicionário `"scripts"` para um `ScriptTask(name, command, TaskType.Tool, null)`.
  - Validar se o valor do comando é uma cadeia de caracteres válida e não vazia; valores nulos, numéricos ou objetos são rejeitados com mensagem amigável em `pt-BR`.
- **Racional**: Mantém a conformidade com o Princípio IV da Constituição (zero dependências extras, lean footprint) e preserva a trim-safety e compatibilidade com compilação Native AOT do .NET 10.
- **Alternativas consideradas**:
  - *Adicionar Newtonsoft.Json*: Rejeitado por violar o Princípio IV (dependência externa pesada e desnecessária).
  - *Ler via expressões regulares*: Rejeitado por ser frágil e propenso a falhas de escape JSON.

---

### 3. Orquestração do Ciclo de Vida npm (`pre<tarefa>` e `post<tarefa>`)

- **Decisão**: Implementar a orquestração de ciclo de vida em um coordenador de execução de tarefas (`TaskExecutionCoordinator` / `LifecycleExecutionRunner`):
  - Quando a tarefa for originada de um `package.json`:
    1. Verifica se existe uma tarefa com nome `pre<tarefa>` na configuração:
       - Se existir, executa `pre<tarefa>`.
       - Se o código de saída for diferente de 0, aborta imediatamente o restante da cadeia e retorna esse código.
    2. Executa a tarefa principal `<tarefa>`.
       - Se o código de saída for diferente de 0, aborta o restante da cadeia e retorna esse código.
    3. Verifica se existe uma tarefa com nome `post<tarefa>` na configuração:
       - Se existir, executa `post<tarefa>` e propaga seu código de saída.
  - Caso o usuário invoque diretamente `nxtl --run pre<tarefa>`, executa apenas o comando do gancho de forma avulsa, sem encadeamentos.
- **Racional**: Entrega paridade exata com a ferramenta `ntl` e com o padrão do npm, permitindo que scripts como compilação prévia e limpeza pós-execução funcionem de forma transparente.
- **Alternativas consideradas**:
  - *Delegar para o `npm run <tarefa>`*: Rejeitado pois o `nxtl` é um executor agnóstico e o ambiente pode utilizar `pnpm`, `yarn`, `bun` ou nem sequer ter o `npm` no PATH em determinados ambientes de teste; a execução nativa de processos pelo `nxtl` preserva total independência e controle.

---

### 4. Filtragem Inteligente no Menu Interativo (`InteractiveTaskSelector`)

- **Decisão**: Na camada de apresentação, ao preparar as tarefas para exibição interativa:
  - Se a origem for `package.json`, filtrar tarefas que iniciam com `"pre"` ou `"post"` APENAS se o comando principal correspondente existir na lista (ex: `prebuild` é ocultado se `build` existir; `posttest` é ocultado se `test` existir).
  - Scripts como `prepare` ou `precommit` que não possuem o comando raiz (não existe `pare` nem `commit`) continuam visíveis no menu.
  - Todas as tarefas permanecem acessíveis via execução direta (`--run <nome>`).
  - Para `nxtl.json`, nenhuma filtragem é aplicada.
- **Racional**: Mantém o menu interativo enxuto e intuitivo, sem duplicar opções para o usuário, alinhando-se à experiência do `ntl`.

---

### 5. Resiliência e Mensagens Diagnósticas em pt-BR

- **Decisão**: Garantir que todos os cenários de erro e aviso (arquivo corrompido, ausência de scripts, JSON malformado, comandos inválidos) retornem mensagens diagnósticas precisas em português do Brasil, em conformidade com o Princípio I da Constituição.
- **Racional**: Satisfaz os requisitos funcionais FR-008, FR-009, FR-010 e critérios de sucesso SC-004.

