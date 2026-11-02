namespace NetTaskList.Infrastructure;

public static class TemplateConfigurationGenerator
{
    public static string GenerateDefaultJson()
    {
        return """
{
  "scripts": [
    {
      "name": "build",
      "command": "dotnet build",
      "type": "tool",
      "description": "Compila a solução do projeto"
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
      "description": "Executa o script de carga inicial em C#"
    }
  ]
}
""";
    }
}
