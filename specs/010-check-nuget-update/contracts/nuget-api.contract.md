# NuGet API Contract: Package Versions Query

**Endpoint**: `GET https://api.nuget.org/v3-flatcontainer/nettasklist/index.json`  
**Protocol**: NuGet Flat Container API (v3)  
**Consumer**: `NetTaskList.Infrastructure.NugetVersionChecker`

---

## 1. Request Contract

- **HTTP Method**: `GET`
- **Request Headers**:
  - `Accept`: `application/json`
  - `User-Agent`: `nxtl/{CurrentVersion}`
- **Timeout**: Estrito de 2 segundos (via `CancellationToken`).

---

## 2. Response Contract

### 2.1 Success Response (`200 OK`)
- **Content-Type**: `application/json`
- **Body Schema**:
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["versions"],
  "properties": {
    "versions": {
      "type": "array",
      "items": {
        "type": "string"
      }
    }
  }
}
```

### Exemplo de Resposta:
```json
{
  "versions": [
    "1.0.0",
    "1.2.0",
    "1.2.1",
    "1.6.0",
    "1.6.1",
    "1.7.0"
  ]
}
```

---

## 3. Error & Edge Case Handling Contract

| Cenário | Resposta / Condição | Comportamento do Cliente (`nxtl`) |
|---|---|---|
| Pacote não encontrado | `404 Not Found` | Falha silenciosa: retorna `HasUpdate = false`, sem exceção. |
| Erro interno do NuGet | `500` / `502` / `503` | Falha silenciosa: retorna `HasUpdate = false`, sem exceção. |
| Timeout de conexão | Tempo > 2 segundos | Cancelamento imediato via token: retorna `HasUpdate = false`. |
| Falha de DNS / Sem Internet | `SocketException` / `HttpRequestException` | Falha silenciosa: retorna `HasUpdate = false`. |
| Corpo JSON malformado | `JsonException` | Falha silenciosa: retorna `HasUpdate = false`. |
