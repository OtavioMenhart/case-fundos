# Case Fundos — Itaú

API RESTful para gerenciamento de fundos de investimento, desenvolvida como case técnico com foco em arquitetura limpa, observabilidade e boas práticas de desenvolvimento .NET.

---

## 🏗️ Arquitetura

O projeto segue os princípios de **Domain-Driven Design (DDD)**, organizado em camadas com dependências unidirecionais:

```
CaseItau.API          → Camada de apresentação (Controllers, Filtros)
CaseItau.Application  → Camada de aplicação (Services, DTOs, Validators)
CaseItau.Domain       → Camada de domínio (Entidades, Value Objects, Constantes, Exceções)
CaseItau.Infra        → Camada de infraestrutura (EF, Repositórios, Migrations)
CaseItau.Tests        → Testes unitários e de integração
```

**Fluxo de dependências:**

```
API → Application → Domain
Infra → Domain
Tests → qualquer camada sob teste
```

---

## 🛠️ Stack Tecnológica

| Categoria | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| Banco de dados | SQL Server 2025 |
| ORM | Entity Framework Core 10 |
| Validação | FluentValidation 12 |
| Logging | Serilog + Serilog.Sinks.Seq |
| Observabilidade | OpenTelemetry (OTLP → Seq) |
| Documentação | Swagger / Swashbuckle |
| Cache | `IMemoryCache` (in-process) |
| Testes unitários | xUnit v3, Moq, Bogus |
| Testes de integração | Testcontainers (SQL Server), `WebApplicationFactory` |
| Containerização | Docker + Docker Compose |
| Gerenciamento de pacotes | Central Package Management (`Directory.Packages.props`) |

---

## 📦 Estrutura do Projeto

### `CaseItau.Domain`
Coração da aplicação — sem dependências externas.

- **`Entities/Fundo`** — entidade principal mapeada para a tabela `FUNDO`
- **`ValueObjects/Cnpj`** — value object que valida e encapsula um CNPJ
- **`ValueObjects/TipoFundo`** — value object mapeado para a tabela `TIPO_FUNDO`
- **`Constants/FundoConstants`** — limites e padrões usados por entidades e validadores
- **`Exceptions/DomainException`** — exceção de domínio para violações de regras de negócio

### `CaseItau.Application`
Lógica de aplicação e orquestração dos casos de uso.

- **`Services/FundoService`** — implementa toda a movimentação de fundos (CRUD + atualização de patrimônio), com validações de duplicidade de `Codigo` e `CNPJ`, verificação de `CodigoTipo` e persistência via Unit of Work
- **`Services/TipoFundoCacheService`** — carrega e armazena em `IMemoryCache` os registros da tabela `TIPO_FUNDO` (tabela estática), evitando consultas repetidas ao banco a cada request
- **`DTOs/`** — `CreateFundoDto`, `UpdateFundoDto` e `FundoDto` (resposta)
- **`Validators/`** — `CreateFundoDtoValidator` e `UpdateFundoDtoValidator` com FluentValidation
- **`Constants/ValidationMessages`** — todas as mensagens de erro de validação centralizadas em constantes
- **`Extensions/ServiceCollectionExtensions`** — registro de serviços, cache e validators da camada

### `CaseItau.Infra`
Integração com recursos externos (apenas SQL Server neste projeto).

- **`Data/AppDbContext`** — contexto do EF com mapeamento das entidades
- **`Repositories/FundoRepository`** — operações de banco específicas de `Fundo` (busca com include, verificação de chaves duplicadas)
- **`Repositories/BaseRepository<T>`** — repositório genérico com `AddAsync`, `Update`, `Delete` e `GetAllAsync`
- **`Repositories/UnitOfWork`** — abstração de `SaveChangesAsync` para controle transacional
- **`Migrations/`** — migration inicial que cria as tabelas `TIPO_FUNDO` e `FUNDO` com seed de tipos
- **`Extensions/ServiceCollectionExtensions`** — registro do DbContext, repositórios, OpenTelemetry e migration automática na inicialização

### `CaseItau.API`
Camada de apresentação.

- **`Controllers/FundoController`** — expõe os 6 endpoints REST consumindo `IFundoService`; todos os status HTTP possíveis estão documentados via `[ProducesResponseType]`
- **`Filters/GlobalExceptionFilter`** — intercepta `DomainException` (→ 422) e exceções genéricas (→ 500), retornando `ProblemDetails`
- **`Program.cs`** — configura Serilog, OpenTelemetry, Swagger, DI de todas as camadas e migration automática (desabilitada no ambiente `Testing`)

---

## 🚀 Como Executar

### Pré-requisitos

- [Docker](https://www.docker.com/) com Docker Compose

### Subindo o ambiente completo

```bash
docker compose up --build
```

Esse comando sobe três containers:

| Container | Porta | Descrição |
|---|---|---|
| `sqlserver` | `1433` | SQL Server 2025 |
| `seq` | `8081` (UI) / `5341` (ingest) | Visualizador de logs e traces |
| `caseitau-api` | `8080` | A API |

A migration é aplicada automaticamente na inicialização da API.

### Acessando os serviços

| Serviço | URL |
|---|---|
| Swagger UI | http://localhost:8080/swagger |
| Seq (logs & traces) | http://localhost:8081 |

> **Credenciais padrão do Seq:** usuário `admin`, senha `PwdSeq123`

---

## 🔌 Endpoints

Base URL: `http://localhost:8080/api/Fundo`

| Método | Rota | Descrição | Status de Retorno |
|---|---|---|---|
| `GET` | `/api/Fundo` | Lista todos os fundos | `200`, `404`, `500` |
| `GET` | `/api/Fundo/{codigo}` | Busca fundo pelo código | `200`, `404`, `500` |
| `POST` | `/api/Fundo` | Cria um novo fundo | `201`, `400`, `422`, `500` |
| `PUT` | `/api/Fundo/{codigo}` | Atualiza nome e/ou tipo do fundo e/ou CNPJ | `200`, `400`, `404`, `422`, `500` |
| `DELETE` | `/api/Fundo/{codigo}` | Remove um fundo | `200`, `404`, `500` |
| `PUT` | `/api/Fundo/{codigo}/patrimonio` | Atualiza o patrimônio líquido | `200`, `404`, `422`, `500` |

---

## ✅ Testes

### Tipos de testes

| Tipo | Localização | Ferramentas |
|---|---|---|
| Unitários — Services | `Tests/UnitTests/Services/` | xUnit, Moq, Bogus |
| Unitários — Repositories | `Tests/UnitTests/Repositories/` | xUnit, Moq, MockQueryable |
| Unitários — Controllers | `Tests/UnitTests/Controllers/` | xUnit, Moq |
| Integração | `Tests/IntegrationTests/` | xUnit, Testcontainers (SQL Server real), `WebApplicationFactory` |

> Os testes de integração sobem automaticamente um container SQL Server via **Testcontainers**, aplicam as migrations e executam os requests HTTP contra a API real — sem mocks de banco.

---

## ⚙️ Configuração

As configurações são lidas de `appsettings.json` e sobrepostas por variáveis de ambiente (padrão .NET). Ao rodar via Docker Compose, as variáveis já estão definidas no `docker-compose.yaml`.

| Variável de Ambiente | Descrição |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string do SQL Server |
| `OpenTelemetry__OtlpEndpoint` | Endpoint OTLP para envio de traces |
| `Serilog__WriteTo__1__Args__serverUrl` | URL do Seq para envio de logs |

---

## 📁 Gerenciamento Central de Pacotes

Todas as versões de pacotes NuGet são definidas em um único arquivo [`Directory.Packages.props`](./Directory.Packages.props) na raiz da solução, garantindo consistência de versões entre todos os projetos.
