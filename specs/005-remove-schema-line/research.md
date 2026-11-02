# Research: Remoção da Linha de Schema do nxtl.json de Configuração

**Feature**: `005-remove-schema-line` | **Date**: 2026-11-01

Este documento registra as pesquisas, decisões de arquitetura e alternativas avaliadas para a remoção da linha indicativa de `$schema` dos arquivos de configuração gerados e do repositório.

---

## 1. Geração de Template Inicial (`nxtl --init`)

### Contexto
Atualmente, `TemplateConfigurationGenerator.GenerateDefaultJson()` inclui explicitamente no início do JSON a propriedade:
```json
"$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
```
O objetivo original era fornecer suporte a IntelliSense no VS Code e outros editores. No entanto, a obrigatoriedade visual desse link externo causa ruído nos arquivos de configuração criados pelos usuários e adiciona acoplamento visual a uma URL pública no GitHub que pode não ser desejável ou acessível em redes isoladas.

### Decisão
Remover a propriedade `"$schema"` da string JSON gerada em `TemplateConfigurationGenerator.GenerateDefaultJson()`. O template padrão começará diretamente com a propriedade `"scripts": [...]`.

### Justificativa
- Simplifica a estrutura visual do arquivo `nxtl.json`.
- Elimina a dependência de URLs remotas nos arquivos gerados em projetos locais.
- Editores e usuários ainda podem referenciar esquemas globalmente ou via configurações do editor (ex: `settings.json` do VS Code), caso queiram validação formal de schema.

### Alternativas Consideradas
- *Comentar a linha de schema*: JSON padrão não suporta comentários de forma portável e limpa sem quebrar parsers estritos. Descartado.
- *Fornecer flag opcional `--no-schema`*: Adicionaria complexidade desnecessária à CLI para um comportamento que deve ser o padrão enxuto. Descartado.

---

## 2. Compatibilidade Retroativa no Carregamento de Configurações (`JsonConfigurationReader` e `ConfigurationDto`)

### Contexto
Usuários que já inicializaram projetos utilizando versões anteriores do `nxtl` possuem arquivos `nxtl.json` que contêm a propriedade `"$schema"`. Além disso, novos usuários criarão arquivos sem essa propriedade.

### Decisão
Manter a propriedade `public string? Schema { get; set; }` mapeada com `[JsonPropertyName("$schema")]` em `ConfigurationDto`.
O leitor de configuração `JsonConfigurationReader` continuará processando perfeitamente:
1. Arquivos sem a propriedade `"$schema"` (novo padrão).
2. Arquivos legados com a propriedade `"$schema"` (compatibilidade retroativa transparente).

### Justificativa
- Não causa nenhum impacto de quebra (*breaking change*) em projetos existentes na transição entre versões do `nxtl`.
- `System.Text.Json` no .NET 10 ignora ou atribui `null` quando a propriedade não estiver presente no JSON, e popula a propriedade caso ela exista, sem disparar exceções.
- O mapeamento para a entidade de domínio `TaskConfiguration` já desconsidera o campo `Schema`, focando apenas na lista de scripts/tarefas.

### Alternativas Consideradas
- *Remover completamente a propriedade do DTO*: Funcionaria se configurado `JsonUnmappedMemberHandling.Skip`, mas manter o mapeamento explícito com `[JsonPropertyName("$schema")]` garante robustez total em cenários de source generation AOT (`TaskJsonContext`).

---

## 3. Atualização do Arquivo `nxtl.json` e Documentação do Repositório

### Contexto
O arquivo `nxtl.json` localizado na raiz do repositório `NetTaskList` possui atualmente a linha:
```json
"$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
```
Além disso, a documentação em `README.md` exibe esse cabeçalho nos exemplos de configuração.

### Decisão
1. Atualizar o arquivo `nxtl.json` na raiz removendo a propriedade `"$schema"`.
2. Atualizar o `README.md` no exemplo de estrutura de `nxtl.json` e na seção de inicialização, refletindo a estrutura limpa e sem `$schema`.
3. Manter o arquivo de schema físico em `src/NetTaskList/schemas/nxtl-v1.schema.json` para consulta e empacotamento opcional da ferramenta, sem forçar sua inclusão nos arquivos de configuração do usuário.

### Justificativa
- Mantém o próprio repositório como exemplo vivo do formato recomendado pela ferramenta.
- Evita discrepâncias entre o que o `README.md` descreve e o que o comando `nxtl --init` produz.

---

## 4. Impacto em Testes Automatizados

### Contexto
`TemplateConfigurationGeneratorTests.cs` atualmente valida explicitamente `Assert.Contains("$schema", template);` e `Assert.Contains("nxtl-v1.schema.json", template);`.

### Decisão
1. Atualizar `TemplateConfigurationGeneratorTests.cs` para validar que `"$schema"` e `"nxtl-v1.schema.json"` NÃO estão contidos no template gerado (`Assert.DoesNotContain`).
2. Adicionar teste específico em `JsonConfigurationReaderTests.cs` para garantir que arquivos de configuração contendo `$schema` continuam sendo lidos com 100% de sucesso (garantia de compatibilidade retroativa).
3. Garantir que a suíte completa de testes passe com zero falhas.
