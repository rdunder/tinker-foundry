# TinkerFoundry

A long-term personal platform for keeping coding skills sharp — a modular monorepo of independently deployable .NET/C# Web API services, fronted by a Blazor UI and a gateway layer.

TinkerFoundry isn't built to ship a product. It's built to be continuously expanded and maintained over time, giving a low-stakes but realistic environment to practice service boundaries, contracts, auth, and eventually async messaging.
 
---

## Architecture

```
                     ┌────────────────┐
                     │  Blazor Client │
                     └───────┬────────┘
                             │
                     ┌───────▼────────┐
                     │  Gateway (YARP)│
                     └───────┬────────┘
              ┌──────────────┼──────────────┐
              │              │              │
        ┌─────▼────┐   ┌─────▼─────┐  ┌─────▼─────┐
        │   Auth   │   │  Account  │  │  Product  │
        │  Service │   │  Service  │  │ Domain(s) │
        └─────┬────┘   └─────┬─────┘  └─────┬─────┘
              │              │              │
          own DB         own DB         own DB
```

**Core principles:**

- **True services, not a monolith-as-classes.** Each service is its own Web API project with its own entry point, database, and port. Services talk over the network — never via in-memory method calls.
- **Gateway as the single entry point.** The Blazor frontend talks to one gateway (YARP), which routes to the backend services. Services are never called directly from the client.
- **DTOs stay local.** Each service owns its own DTOs/models. No shared Contracts library — if one is ever introduced, it will hold only network-crossing DTOs, never database entities.
- **Defer complexity until it's felt.** Kubernetes, a shared Contracts library, and other infrastructure layers are deliberately left out until their absence causes real friction.
---

## Services

| Service   | Status      | Responsibility                          |
|-----------|-------------|------------------------------------------|
| Gateway   | Planned     | Single entry point (YARP), routes to backend services |
| Auth      | In design   | Authentication, JWT issuance             |
| Account   | In design   | User account data                        |
| Product   | Not started | Core domain services (TBD)               |

Build order: **Auth → Account → Product domain → async messaging patterns**.
 
---

## Frontend

- **Blazor**, chosen for long-term stability (LTS release cadence tied to .NET, predictable upgrade path) and to lean into an existing .NET/C# background rather than fighting a fast-churning JS ecosystem.
- Frontend team-independence approach: starting as a **modular monolith** (class libraries per feature area), with true micro-frontends (separately deployed Blazor WASM modules) as a possible future step if needed.
---

## Tech stack

- **Backend:** .NET / C#, ASP.NET Core Web API per service
- **Gateway:** YARP
- **Frontend:** Blazor
- **Auth:** JWT-based
- **Databases:** one per service (engine TBD per service as needed)
---

## Status

Early design phase. API contracts for Auth and Account have been sketched (endpoints, DTOs, status codes, JWT claim structure). No services implemented yet.