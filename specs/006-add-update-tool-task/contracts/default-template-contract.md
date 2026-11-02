# Contract: Contrato do Template Padrão e Tarefa de Atualização

**Feature**: `006-add-update-tool-task` | **Date**: 2026-11-01

Este documento especifica o contrato do arquivo modelo gerado por `nxtl --init` e a interface de execução da nova tarefa.

---

## 1. Contrato da Tarefa `update-nettasklist`

### Definição
- **Nome**: `update-nettasklist`
- **Comando**: `dotnet tool update -g nettasklist`
- **Tipo**: `tool`
- **Descrição**: `Atualiza a ferramenta nxtl de modo global`

### Invocação Direta via CLI
```bash
nxtl --run update-nettasklist
# ou
nxtl -r update-nettasklist
```

### Invocação Interativa
Ao rodar `nxtl` sem argumentos no terminal interativo, a opção `update-nettasklist` deve ser exibida no menu de seleção entre as opções disponíveis, exibindo a descrição `"Atualiza a ferramenta nxtl de modo global"`.

---

## 2. Contrato do Arquivo Gerado por `nxtl --init`

O arquivo `nxtl.json` gerado pelo comando `nxtl --init` deve conter estritamente:
1. Array `scripts` com 4 itens.
2. Nenhuma menção a `$schema`.
3. Validação garantida sem colisões de nomes ou comandos inválidos.
