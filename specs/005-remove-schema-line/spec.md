# Feature Specification: Remoção da Linha de Schema do nxtl.json de Configuração

**Feature Branch**: `005-remove-schema-line`

**Created**: 2026-11-01

**Status**: Draft

**Input**: User description: "Remover a linha indicativa de schema do nxtl.json de configuração"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Geração de Arquivo de Configuração Limpo sem Linha de Schema (Priority: P1)

Como um desenvolvedor inicializando o `nxtl` em um projeto através do comando `nxtl --init` (ou `nxtl -i`), desejo que o arquivo `nxtl.json` gerado não contenha a linha com a declaração `"$schema"`, para que eu tenha um arquivo de configuração mais conciso, limpo e sem referências a URLs externas desnecessárias no meu repositório.

**Why this priority**: O comando de inicialização é o ponto inicial de adoção do `nxtl` em novos projetos e define o padrão oficial de configuração apresentado aos usuários.

**Independent Test**: Pode ser testado executando o comando de inicialização (`nxtl --init`) em um diretório sem arquivo de configuração e validando que o arquivo `nxtl.json` gerado contém os scripts de exemplo, mas não contém a propriedade `"$schema"`.

**Acceptance Scenarios**:

1. **Given** a execução do comando `nxtl --init` (ou `nxtl -i`) em um diretório sem arquivo de configuração, **When** o arquivo `nxtl.json` for criado, **Then** o conteúdo gerado NÃO deve conter a propriedade `"$schema"` nem qualquer referência de URL para esquemas JSON externos.
2. **Given** um arquivo `nxtl.json` gerado sem a linha `"$schema"`, **When** o comando `nxtl` for executado para listar ou disparar tarefas, **Then** a ferramenta deve carregar e processar o arquivo com sucesso, sem exibir advertências ou falhas.

---

### User Story 2 - Atualização da Configuração do Projeto e Compatibilidade Retroativa (Priority: P2)

Como um mantenedor do projeto ou usuário do `nxtl`, desejo que o arquivo de configuração `nxtl.json` na raiz deste repositório não tenha a linha de schema e que o sistema continue compatível com arquivos que contenham ou não essa linha, para assegurar tanto a modernização da configuração interna quanto a compatibilidade com projetos legados.

**Why this priority**: Assegura a consistência no repositório de desenvolvimento e garante que usuários que já possuam arquivos `nxtl.json` com `"$schema"` não sofram quebras ao atualizar o `nxtl`.

**Independent Test**: Pode ser testado inspecionando o arquivo `nxtl.json` do projeto para garantir a remoção da linha de schema, e rodando a suíte de testes com cenários que avaliem a leitura de configurações tanto com quanto sem `"$schema"`.

**Acceptance Scenarios**:

1. **Given** o arquivo `nxtl.json` no repositório do projeto, **When** for inspecionado ou utilizado na execução de scripts internos, **Then** ele não contém a propriedade `"$schema"` e suas tarefas são executadas normalmente.
2. **Given** um arquivo de configuração legado que ainda contenha a propriedade `"$schema"`, **When** for lido pelo `nxtl`, **Then** a aplicação carrega e valida o arquivo sem disparar erros de deserialização ou validação.

---

### Edge Cases

- **Arquivo contendo apenas a linha de schema (sem tarefas/scripts)**: Continua sendo tratado pelas regras existentes de validação (rejeitado por ausência de tarefas válidas cadastradas).
- **Arquivo contendo `$schema` com conteúdo nulo ou malformatado**: O sistema tolera graciosamente a propriedade sem afetar a identificação e execução dos scripts declarados.
- **Ambientes desconectados (offline)**: A ausência da URL de schema evita que editores e ferramentas tentem resolver esquemas remotos em redes restritas ou sem acesso à internet.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O gerador de configuração modelo (`nxtl --init` ou `nxtl -i`) NÃO DEVE incluir a propriedade `"$schema"` no JSON produzido.
- **FR-002**: O arquivo de configuração `nxtl.json` presente na raiz do repositório DEVE ter a propriedade `"$schema"` removida.
- **FR-003**: O leitor de configuração do utilitário `nxtl` DEVE processar com sucesso arquivos de configuração sem a propriedade `"$schema"`.
- **FR-004**: O leitor de configuração do utilitário `nxtl` DEVE manter compatibilidade com arquivos legados que ainda contenham a propriedade `"$schema"`, tratando-a como opcional e não impeditiva.
- **FR-005**: Documentações e referências no repositório (ex.: `README.md`) que exibem exemplos de `nxtl.json` DEVEM ser atualizadas para não exibir a linha de schema.

### Key Entities *(include if feature involves data)*

- **Modelo de Configuração Inicial**: Conteúdo JSON padrão gerado ao inicializar uma configuração, contendo unicamente os blocos essenciais de definição de scripts e tarefas.
- **Configuração de Tarefas**: Estrutura que representa os scripts configurados, operando de forma independente da presença ou ausência da propriedade `"$schema"`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos novos arquivos `nxtl.json` criados via comando de inicialização são gerados sem a propriedade `"$schema"`.
- **SC-002**: O arquivo `nxtl.json` da raiz do repositório é validado e executado pelo `nxtl` sem possuir a linha de schema.
- **SC-003**: 100% dos cenários de teste automatizados para arquivos sem schema e com schema legado passam com sucesso.
- **SC-004**: A suíte de testes automatizados mantém 100% de aprovação e zero regressões.

## Assumptions

- A remoção da linha de schema não afeta nenhuma funcionalidade de execução de comandos ou validação semântica de tarefas.
- Usuários que desejarem validação por schema em seus editores poderão associar o esquema local ou globalmente pelo próprio editor caso necessário, mas o arquivo de configuração gerado por padrão não terá a dependência visual ou remota.
