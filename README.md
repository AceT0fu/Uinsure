# Uinsure Technical Assessment

A REST API written in C# and ASP.NET Core for managing home insurance policies.

---

## Running the Application

### Prerequisites

- .NET 10 SDK

### Start the API

```bash
dotnet run --project Uinsure.Api
```

Swagger UI will be available at:

```text
https://localhost:<port>/swagger
```

---

## Running Tests

```bash
dotnet test
```

---

## Assumptions

The following assumptions were made during implementation:

- Policy renewals create a **new policy** rather than modifying the existing one, in order to keep transaction history.
- Renewed policies retain a reference to the previous policy.
- Policies that have been cancelled or renewed cannot be acted upon again.
- Requirements say PolicyHolders should be 16 and over, but I made this to include 16.

---

## Future Improvements

Given additional time, the following improvements could be made:

- Replace the in-memory repository with a database-backed implementation.
- Full API integration tests, from service to repository, without mocks, to validate full end-to-end behaviour with data stores.
- Introduce request/response DTOs for all endpoints.
- Extract policy creation into a dedicated mapper/helper class.
- Add structured logging and monitoring.
- Implement authentication and authorisation.

---
