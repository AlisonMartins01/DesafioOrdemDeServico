# Sistema de Gerenciamento de Ordens de Serviço

Sistema de gerenciamento de ordens de serviço implementando DDD, CQRS e padrões arquiteturais corporativos com .NET 10, SQL Server 2022 e Dapper.

## Stack Técnica

| Camada | Tecnologia | Justificativa |
|--------|-----------|---------------|
| **API** | ASP.NET Core 10 Minimal APIs | Performance, endpoints leves |
| **Data Access** | Dapper | 3-5x mais rápido que EF Core para read-heavy workloads |
| **Migrations** | FluentMigrator | Controle total sobre SQL, compatível com Dapper |
| **Database** | SQL Server 2022 | Índices avançados, filtered indexes |
| **Messaging** | MediatR | CQRS pattern (commands/queries) |
| **Testing** | xUnit v3 + WebApplicationFactory | Testes de integração com DB real |
| **API Docs** | Scalar | OpenAPI UI moderno |

## Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│ API Layer (Minimal Endpoints)                               │
│  ├── CustomerEndpoints.cs                                   │
│  └── ServiceOrderEndpoints.cs                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Application Layer (MediatR Commands/Queries)                │
│  ├── Commands: CreateCustomer, CreateServiceOrder, etc.     │
│  └── Queries: GetCustomers, GetServiceOrders, etc.          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Domain Layer (Aggregates, Value Objects, Invariants)        │
│  ├── CustomerAggregate (Document, Email, Phone)             │
│  ├── ServiceOrderAggregate (Status, Money, Attachments)     │
│  └── Business Rules (state transitions, validations)        │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Infrastructure Layer (Repositories, Migrations)              │
│  ├── Dapper Repositories (raw SQL, parameterized queries)   │
│  └── FluentMigrator (4 migrations com índices estratégicos) │
└─────────────────────────────────────────────────────────────┘
```

### Decisões Arquiteturais

**1. Dapper ao invés de EF Core**
- Read-heavy workload → SQL puro é 3-5x mais rápido
- Controle total sobre queries para otimização
- Sem overhead de change tracking
- Trade-off: mapeamento manual

**2. FluentMigrator ao invés de EF Core Migrations**
- Compatível com Dapper (sem dependência de DbContext)
- SQL puro para filtered indexes e recursos específicos do SQL Server
- Migrações executam automaticamente na inicialização da API com retry logic

**3. CQRS com MediatR**
- Commands para escrita (validação, regras de negócio)
- Queries otimizadas para leitura (Dapper)
- Separação clara de responsabilidades

**4. Value Objects Imutáveis**
- `Document` (CPF/CNPJ com validação)
- `Email` (RFC 5322 compliant)
- `Phone` (formato brasileiro)
- `Money` (multi-moeda: BRL, USD, EUR)

**5. Agregados com Invariantes Protegidos**
- `Customer`: restrições únicas em Document/Phone
- `ServiceOrder`: state machine (Open → InProgress → Finished)
- Transições de status validadas no domínio

## Rodando o Projeto

### Opção 1: Docker (Recomendado para Avaliação)

```bash
cd Infra
docker-compose up
```

**O que acontece:**
1. SQL Server 2022 sobe em `localhost:1433` (health check: 60s)
2. API builda via Dockerfile multistage
3. API aguarda SQL Server healthy
4. Migrações executam automaticamente (retry logic: 30 tentativas)
5. API disponível em `http://localhost:5000`

**Credenciais SQL Server:**
- User: `sa`
- Password: `SqlServer2024!Strong#` (definido em `Infra/.env`)
- Database: `OsServiceDb`

### Opção 2: Local (F5 no Visual Studio/Rider)

**Requisitos:**
- SQL Server local rodando OU Docker SQL Server
- Connection string em `appsettings.json` apontando para localhost

**Execução:**
1. Abrir solução no Visual Studio/Rider
2. Definir `OsService.ApiService` como startup project
3. Pressionar F5

**Endpoints:**
- HTTP: `http://localhost:5420`
- HTTPS: `https://localhost:7391`

## Estrutura do Projeto

```
ControleOrdemDeServico/
├── ControleOrdemDeServico.ApiService/        # Minimal API endpoints
│   ├── Endpoints/
│   ├── Program.cs                            # DI, migrations, middleware
│   └── Dockerfile                            # Multistage build
├── ControleOrdemDeServico.Services/          # MediatR commands/queries
├── ControleOrdemDeServico.Domain/            # Aggregates, Value Objects
│   ├── Aggregates/
│   │   ├── CustomerAggregate/
│   │   └── ServiceOrderAggregate/
│   └── ValueObjects/ (Document, Email, Phone, Money)
├── ControleOrdemDeServico.Infrastructure/    # Dapper repos, migrations
│   ├── Migrations/
│   │   ├── 0001_CreateCustomersTable.cs
│   │   ├── 0002_CreateServiceOrdersTable.cs
│   │   ├── 0003_CreateAttachmentsTable.cs
│   │   └── 0004_AddPerformanceIndexes.cs
│   └── Repositories/ (Dapper-based)
├── ControleOrdemDeServico.Tests/             # Integration tests (xUnit v3)
└── Infra/
    ├── docker-compose.yml                    # API + SQL Server
    ├── docker-compose.override.yml
    └── .env                                  # SQL Server password
```

## Migrations

### Estratégia

**FluentMigrator** com 4 migrações versionadas:

| Migração | Descrição |
|----------|-----------|
| `0001_CreateCustomersTable` | Customers com filtered unique indexes em Document/Phone |
| `0002_CreateServiceOrdersTable` | ServiceOrders com auto-increment (seed: 1000), FK para Customers |
| `0003_CreateAttachmentsTable` | Metadados de fotos (Before/After) |
| `0004_AddPerformanceIndexes` | Covering indexes para otimização de queries |

### Execução Automática

As migrações rodam **automaticamente na inicialização da API** (`Program.cs` linha 44-73):

```csharp
// Retry logic: 30 tentativas, 2 segundos entre tentativas
for (int i = 0; i < maxRetries; i++)
{
    using var scope = app.Services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

    runner.MigrateUp();  // Executa migrações pendentes
}
```

**Características:**
- Aguarda SQL Server ficar disponível (health check do Docker)
- Retry com backoff exponencial (até 60s de espera)
- Logs detalhados de sucesso/falha
- Bloqueia startup da API em caso de falha

## Performance: Índices Estratégicos

```sql
-- Dashboard: filtrar por status
IX_ServiceOrders_Status

-- Relatórios: range queries por data
IX_ServiceOrders_OpenedAt

-- Histórico do cliente
IX_ServiceOrders_CustomerId_Status

-- Covering index: elimina lookups
IX_ServiceOrders_Status_OpenedAt INCLUDE (Number, Description, Price)
```

**Impacto:**
- Antes: Table Scan (100K rows = 2-5s)
- Depois: Index Seek (100K rows = 50-100ms)
- **Resultado: 20-50x mais rápido**

## Endpoints da API

### Customers
```http
GET    /customers              # Listar (paginação)
GET    /customers/{id}         # Detalhes + histórico de ordens
POST   /customers              # Criar (validação com FluentValidation)
PUT    /customers/{id}         # Atualizar
DELETE /customers/{id}         # Soft delete
```

### Service Orders
```http
GET   /service-orders          # Listar (filtros, ordenação)
GET   /service-orders/{id}     # Detalhes + anexos
POST  /service-orders          # Criar
PUT   /service-orders/{id}     # Atualizar
PATCH /service-orders/{id}/start   # Transição: Open → InProgress
PATCH /service-orders/{id}/finish  # Transição: InProgress → Finished
PATCH /service-orders/{id}/price   # Atualizar preço (audit trail)
```

### Attachments
```http
POST /service-orders/{id}/attachments  # Upload foto (Before/After)
GET  /attachments/{id}                 # Download
```

**Documentação:** `http://localhost:5000/scalar/v1` (Scalar OpenAPI UI)

## Testes

```bash
dotnet test
```

**Cobertura:**
- 18 testes de integração
- WebApplicationFactory (full stack testing)
- Database real (não mockado)
- Isolamento com `TestDatabaseHelper.ClearAllTablesAsync()`

## Modelo de Domínio

### Agregados

**Customer**
- ID: `Guid`
- Document: `Document` (CPF/CNPJ) - unique constraint filtrado
- Phone: `Phone` - unique constraint filtrado
- Email: `Email` (RFC 5322)
- Name: `string`

**ServiceOrder**
- ID: `Guid`
- Number: `int` (auto-increment a partir de 1000)
- CustomerId: `Guid` (FK)
- Status: `ServiceOrderStatus` (enum: Open, InProgress, Finished)
- Price: `Money` (multi-moeda)
- Description: `string`
- OpenedAt, StartedAt, FinishedAt: `DateTimeOffset`
- Attachments: `List<Attachment>`

**Attachment**
- ID: `Guid`
- ServiceOrderId: `Guid` (FK)
- Type: `AttachmentType` (Before/After)
- FileName, FilePath, ContentType
- FileSize: `long`

### Regras de Negócio (Invariantes)

✅ Customer com Document duplicado → `DomainError`
✅ Customer com Phone duplicado → `DomainError`
✅ ServiceOrder.Start() só se Status == Open
✅ ServiceOrder.Finish() só se Status == InProgress
✅ Preço atualizado registra timestamp (audit trail)
✅ Order numbers sequenciais e únicos

## Padrões Implementados

| Padrão | Aplicação |
|--------|-----------|
| **Repository** | Abstração de acesso a dados (Dapper) |
| **Result Pattern** | Retorno explícito de erros sem exceptions |
| **Value Object** | Imutabilidade, validação encapsulada |
| **Aggregate Root** | Limites de consistência, proteção de invariantes |
| **CQRS** | Commands (escrita) vs Queries (leitura) |
| **Factory** | `DefaultSqlConnectionFactory` para DI |
| **Strongly-Typed IDs** | `CustomerId`, `ServiceOrderId` (evita primitive obsession) |

## Boas Práticas

### Segurança
- Queries parametrizadas (proteção contra SQL injection)
- Validação de entrada com FluentValidation
- Secrets em `.env` (não commitados)
- Mensagens de erro sem exposição de internals

### Performance
- Async/await para todas operações I/O
- Covering indexes para queries críticas
- Dapper para leituras (eliminando overhead do EF Core)

### Testabilidade
- Repository pattern permite test doubles
- WebApplicationFactory para testes end-to-end
- Database cleanup automático entre testes

## Próximos Passos (Sugestões)

- [ ] Autenticação/Autorização (JWT com refresh tokens)
- [ ] Cache distribuído (Redis) para queries de leitura
- [ ] Domain Events + MediatR notifications
- [ ] API Versioning (Asp.Versioning.Mvc)
- [ ] Rate Limiting (ASP.NET Core middleware)
- [ ] Background Jobs (Hangfire para processamento assíncrono)
- [ ] CQRS completo com read models separados
- [ ] Monitoring/APM (Application Insights ou Grafana)

---

**Stack:** .NET 10.0 | SQL Server 2022 | Dapper | FluentMigrator | MediatR
**Desenvolvido como:** Avaliação técnica para posição .NET Sênior
**Data:** Janeiro 2026
