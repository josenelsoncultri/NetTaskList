# NetTaskList (`nxtl`)

> Processador declarativo de tarefas e scripts com interface rica de terminal para o ecossistema .NET.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Native AOT](https://img.shields.io/badge/Native-AOT-brightgreen.svg)](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)

O **NetTaskList** (`nxtl`) é uma ferramenta de linha de comando desenvolvida em .NET 10 que permite cadastrar, organizar, validar e executar tarefas do seu projeto de forma declarativa através de arquivos JSON (`nxtl.json`), análogo ao que o `package.json` faz no Node.js, com suporte avançado a scripts C# file-scoped e interface interativa via [Spectre.Console](https://spectreconsole.net/).

---

## 🚀 Principais Recursos

- **Interface Rica e Interativa**: Listagem de tarefas em tabela estilizada e prompt de seleção navegável via teclado (setas `[Up]` e `[Down]`).
- **Scripts C# File-Scoped**: Suporte nativo para tarefas do tipo `csharp` (ex: `scripts/seed.cs`), inferindo automaticamente a execução com `dotnet run`.
- **Validação Semântica Estrita**: Intercepta duplicidades de nomes (sem distinção de maiúsculas/minúsculas) e erros de sintaxe com diagnósticos claros em português brasileiro (`pt-BR`).
- **Automação & CI/CD**: Execução direta sem prompt interativo através da flag `--run <nome>` (ou `-r <nome>`) e detecção inteligente de consoles não-interativos.
- **Native AOT & Alta Performance**: Compilado nativamente com Native AOT (`PublishAot=true`), garantindo inicialização sub-15ms e operando como binário autônomo (*self-contained*) sem necessidade de .NET Runtime instalado na máquina alvo.
- **IntelliSense no Editor**: Esquema formal JSON Schema incluso para auto-completar e validação no VS Code, Visual Studio e JetBrains Rider.

---

## 📦 Instalação

### Como Ferramenta Global do .NET

```bash
dotnet tool install -g NetTaskList
```

Após instalado, o comando `nxtl` fica disponível globalmente no seu terminal.

### Como Executável Nativo Independente (Native AOT)

Baixe o binário para o seu sistema operacional ou compile localmente:

```bash
dotnet publish src/NetTaskList/NetTaskList.csproj -c Release -r win-x64 --self-contained
```

---

## 🛠️ Começando Rápido

### 1. Inicializar a Configuração no Projeto

No diretório raiz do seu projeto, gere um arquivo `nxtl.json` modelo:

```bash
nxtl --init
```

Isso criará o arquivo `nxtl.json` com o esquema formal configurado e tarefas de exemplo.

### 2. Estrutura do Arquivo `nxtl.json`

```json
{
  "$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
  "scripts": [
    {
      "name": "build",
      "command": "dotnet build",
      "type": "tool",
      "description": "Compila os projetos da solução"
    },
    {
      "name": "test",
      "command": "dotnet test",
      "type": "tool",
      "description": "Executa todos os testes automatizados"
    },
    {
      "name": "seed",
      "command": "scripts/seed.cs",
      "type": "csharp",
      "description": "Script C# file-scoped para popular banco de dados"
    }
  ]
}
```

### 3. Listar e Navegar Interativamente

Execute no terminal:

```bash
nxtl
```

Você verá a tabela estilizada e poderá usar as setas do teclado para escolher a tarefa desejada.

### 4. Executar uma Tarefa Diretamente (CI/CD)

```bash
nxtl --run build
# ou
nxtl -r test
```

---

## 📖 Opções de Linha de Comando

| Opção | Atalho | Descrição |
|---|:---:|---|
| `--run <nome>` | `-r` | Seleciona e executa diretamente uma tarefa sem abrir prompt |
| `--config <caminho>` | `-c` | Especifica o caminho de um arquivo de configuração JSON customizado |
| `--init` | `-i` | Gera um arquivo padrão `nxtl.json` no diretório atual |
| `--help` | `-h` | Exibe a mensagem de ajuda com todas as opções |
| `--version` | `-v` | Exibe a versão instalada da ferramenta |

---

## 🧪 Testes Automatizados

O projeto possui cobertura completa de testes unitários desenvolvidos em xUnit:

```bash
dotnet test
```

---

## 📄 Licença

Distribuído sob a licença MIT. Consulte [LICENSE](LICENSE) para obter mais informações.
