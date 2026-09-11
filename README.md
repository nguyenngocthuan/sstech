# Partner Transaction API

A .NET 8 Web API for receiving partner transactions, verifying partners through an external API, and publishing accepted transactions to RabbitMQ asynchronously.

## Architecture

The application follows a layered architecture with clear separation of responsibilities:

Client
  │
  ▼
PartnerTransactionController
  │
  ▼
PartnerTransactionService
  │
  ├── PartnerVerificationService
  │       │
  │       ▼
  │   PartnerVerificationClient
  │       │
  │       ▼
  │   Mock Partner Verification API
  │
  └── RabbitMqMessageSender
          │
          ▼
       RabbitMQ
```

### Main components

* **Controllers**

  * Expose HTTP endpoints.
  * Handle HTTP-specific concerns such as request/response and authorization.

* **Validators**

  * FluentValidation is used to validate incoming transaction requests.
  * Validation includes required fields, positive amount, and supported currencies.

* **Services**

  * Contain the main business logic.
  * `PartnerTransactionService` coordinates partner verification and message publishing.
  * `PartnerVerificationService` handles partner verification and resilience policies.

* **Clients**

  * `PartnerVerificationClient` encapsulates HTTP communication with the partner verification API.

* **Messaging**

  * `TransactionMessageSender` abstracts message publishing.
  * RabbitMQ is used for asynchronous transaction processing.

* **Middleware**

  * A global exception handler provides consistent error responses without exposing internal exception details.

* **Dependency Injection**

  * Application services and dependencies are registered in `ServiceRegistration`.

## Resilience

Partner verification uses Polly resilience pipelines.

The verification request retries automatically when a `TimeoutException` occurs.

Configuration:

* Maximum retry attempts: 3
* Exponential backoff
* Jitter enabled

Therefore, one initial request can result in up to 4 total attempts.

The retry behavior is covered by unit tests using mocks, so tests are deterministic and do not depend on the random behavior of the mock verification API.

## Security

The transaction endpoint is protected using JWT Bearer Authentication.

Authorization requires the following scope:

```text
transactions.write
```

JWT validation includes:

* Issuer validation
* Audience validation
* Token lifetime validation
* Signing key validation

For demonstration purposes, the project provides:

```text
POST /api/v1/auth/token
```

which generates a development JWT.

In a production environment, tokens would be issued by a dedicated identity provider and secrets would be stored in a secure secret-management system.

## Prerequisites

* .NET 8 SDK
* Docker Desktop
* Docker Compose

## Run locally

Restore dependencies:

```bash
dotnet restore
```

Run the API:

```bash
dotnet run --project src/PartnerTransaction.Api
```

Swagger:

```text
https://localhost:7112/swagger
```

> The actual local URL may differ depending on the launch profile.

## Run with Docker Compose

The project includes Docker support for both the API and RabbitMQ.

Run:

```bash
docker compose up --build
```

Or run in the background:

```bash
docker compose up --build -d
```

Services:

| Service                | URL / Port                    |
| ---------------------- | ----------------------------- |
| API                    | http://localhost:7112         |
| Swagger                | http://localhost:7112/swagger |
| RabbitMQ AMQP          | localhost:5672                |
| RabbitMQ Management UI | http://localhost:15672        |

RabbitMQ default development credentials:

```text
Username: guest
Password: guest
```

Stop the services:

```bash
docker compose down
```

## Test the API

### 1. Generate a JWT

Call:

```http
POST /api/v1/auth/token
```

Copy the returned `accessToken`.

### 2. Authorize in Swagger

Click **Authorize** in Swagger and enter the JWT token.

Swagger will send:

```http
Authorization: Bearer <token>
```

### 3. Create a transaction

Call:

```http
POST /api/v1/partner/transactions
```

Example request:

```json
{
  "partnerId": "P-1001",
  "transactionReference": "TXN-99823",
  "amount": 250.00,
  "currency": "USD",
  "timestamp": "2024-05-10T14:30:00Z"
}
```

A successful request returns:

```http
202 Accepted
```

The transaction is then published asynchronously to RabbitMQ.

## Run tests

Run all tests:

```bash
dotnet test
```

Run with detailed output:

```bash
dotnet test --verbosity normal
```

The test project covers:

### Validation

* Valid request
* Missing PartnerId
* Missing TransactionReference
* Invalid amount
* Missing currency
* Unsupported currency
* Supported currencies

### Resilience

* Successful partner verification
* Retry after timeout
* Successful verification after retries
* Failure after all retries are exhausted

### Services

* Partner verification flow
* Transaction processing
* Message publishing behavior

Tests use **xUnit** and **Moq**.

## Project Structure

```text
PartnerTransaction.sln
│
├── docker-compose.yml
├── README.md
│
├── src
│   └── PartnerTransaction.Api
│       ├── Controllers
│       ├── Models
│       ├── Validators
│       ├── Constants
│       ├── Services
│       ├── Clients
│       ├── Messaging
│       ├── Middleware
│       ├── Exceptions
│       ├── DependencyInjection
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Dockerfile
│       └── PartnerTransaction.Api.csproj
│
└── tests
    └── PartnerTransaction.Tests
        ├── Validation
        ├── Resilience
        └── Services
```

## Design Decisions

### Why RabbitMQ?

The transaction endpoint should respond quickly without waiting for downstream processing. RabbitMQ provides asynchronous message delivery and decouples transaction acceptance from transaction processing.

### Why Polly?

Partner verification is an external dependency and may temporarily fail. Polly provides retry and resilience behavior without coupling retry logic directly to the business code.

### Why interfaces?

Interfaces such as `PartnerVerificationService` and `TransactionMessageSender` make dependencies explicit and allow external services to be mocked during unit testing.

### Why return HTTP 202?

The transaction is accepted for asynchronous processing rather than being fully processed during the HTTP request. Therefore, `202 Accepted` accurately represents the API behavior.

### Why Global Exception Handling?

Centralized exception handling keeps controllers clean and provides a consistent error response format while preventing internal implementation details from being exposed to clients.

## Notes

The partner verification API is intentionally implemented as a mock endpoint within the API project for the coding exercise. It randomly simulates a timeout approximately 30% of the time and returns a successful verification response otherwise.

For production, the mock endpoint would be replaced by the actual partner verification service, and secrets would be managed using a secure secret store rather than source-controlled configuration.
