# Contract: Resolução e Precedência de Configuração

## Escopo
Este contrato especifica as regras determinísticas de descoberta, seleção e precedência de arquivos de configuração executadas pela ferramenta CLI `nxtl`.

---

## Matriz de Decisão de Resolução

| Parâmetro `--config` | `nxtl.json` no CWD | `package.json` no CWD | Arquivo Selecionado | Comportamento / Saída |
|---|---|---|---|---|
| Informado (`-c <path>`) | Qualquer | Qualquer | `<path>` | Lê estritamente o arquivo informado. Se o arquivo não existir, erro fatal com saída exit code 1. |
| Não informado | Existe | Existe | `nxtl.json` | `nxtl.json` prevalece com exclusividade. `package.json` é totalmente ignorado. |
| Não informado | Existe | Não existe | `nxtl.json` | `nxtl.json` é carregado normalmente. |
| Não informado | Não existe | Existe | `package.json` | `package.json` é carregado como configuração ativa. |
| Não informado | Não existe | Não existe | Nenhum | Exibe mensagem diagnóstica em `pt-BR` informando que nem `nxtl.json` nem `package.json` foram encontrados, orientando sobre `nxtl --init`. Exit code 1. |

---

## Formato das Mensagens de Erro em pt-BR

### Cenário: Nenhum arquivo encontrado
```text
Erro: Nenhum arquivo de configuração ('nxtl.json' ou 'package.json') foi encontrado no diretório atual.
Execute 'nxtl --init' para criar um arquivo inicial ou use 'nxtl --config <caminho>' para especificar outro arquivo.
```

### Cenário: Arquivo explícito inexistente
```text
Erro: O arquivo de configuração '<caminho>' não foi encontrado.
```

