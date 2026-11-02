# Contract: Contrato do Arquivo de Configuração e Comando de Inicialização

**Feature**: `005-remove-schema-line` | **Date**: 2026-11-01

Este documento especifica o contrato externo do arquivo de configuração `nxtl.json` e o comportamento do comando de inicialização `nxtl --init`.

---

## 1. Contrato do Comando `nxtl --init` / `nxtl -i`

### Invocação
```bash
nxtl --init
# ou
nxtl -i
```

### Comportamento
1. Verifica se `nxtl.json` já existe no diretório alvo.
2. Se o arquivo já existir:
   - Saída de erro (código de saída 1): `Erro: O arquivo de configuração 'nxtl.json' já existe e não será sobrescrito.`
3. Se o arquivo não existir:
   - Cria o arquivo `nxtl.json` com o template padrão.
   - Saída de sucesso (código de saída 0): `Sucesso: Arquivo de configuração 'nxtl.json' criado com êxito!`
   - **Garantia Contratual**: O conteúdo gerado NÃO contém a propriedade `"$schema"`.

---

## 2. Contrato Estrutural do `nxtl.json`

### Gramática JSON
```json
{
  "scripts": [
    {
      "name": "<string: obrigatório, não-vazio>",
      "command": "<string: obrigatório, não-vazio>",
      "type": "<string: opcional, 'tool' | 'csharp'>",
      "description": "<string: opcional>"
    }
  ]
}
```

### Regras de Conformidade
1. A propriedade `"$schema"` NÃO é requerida para validação ou execução.
2. Caso a propriedade `"$schema"` esteja presente (arquivos legados), o leitor deve processá-la sem erro.
3. O array de tarefas pode ser declarado sob o nome `"scripts"` ou `"tasks"`, ou como array raiz direto `[...]`.
4. Cada tarefa deve possuir obrigatoriamente `"name"` e `"command"`.
5. Nomes de tarefas não podem se repetir (comparação não sensível a maiúsculas/minúsculas).
