<!-- OPENSPEC:START -->
# OpenSpec Instructions

These instructions are for AI assistants working in this project.

Always open `@/openspec/AGENTS.md` when the request:
- Mentions planning or proposals (words like proposal, spec, change, plan)
- Introduces new capabilities, breaking changes, architecture shifts, or big performance/security work
- Sounds ambiguous and you need the authoritative spec before coding

Use `@/openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guidelines

Keep this managed block so 'openspec update' can refresh the instructions.

<!-- OPENSPEC:END -->

# Agent Guide (Signal Management)
- Build: `dotnet build VGT.Galaxy.Backend.SignalManagement.sln`
- Format/lint: `dotnet format` (style+analyzers); respect `.editorconfig` defaults
- Test all: `dotnet test Test/VGT.Galaxy.Backend.Services.SignalManagement.Test.csproj`
- Run single test (MSTest): `dotnet test --filter "FullyQualifiedName~Namespace.ClassName.TestMethod"`
- Projects target `net9.0`, `ImplicitUsings=enable`, `Nullable=enable`; prefer explicit types over `var` when clarity helps
- Imports: place `System.*` first, then third‑party, then local; remove unused usings
- Naming: PascalCase for public types/members; camelCase for locals/fields; interfaces start with `I`; async methods end with `Async`
- Error handling: throw domain exceptions (`Domain/Exceptions`) and let `Api/GlobalExceptionHandler.cs` translate to HTTP; avoid broad catch; use `ProblemDetails`
- Controllers: validate input, return proper status codes; don’t expose internal exceptions
- Services/Repositories: use dependency injection via `Application/Persistence/DependencyInjectionExtensions.cs`; avoid static singletons
- Entities vs Models: `Persistence/Models` map to DB; `Domain/Models` for API contracts; avoid leaking EF types to API
- Testing: use MSTest; prefer deterministic tests; isolate external resources (Testcontainers already configured)
- Configuration: appsettings.* for environment; don’t hardcode secrets; use options binding
- Cursor/Copilot rules: none found (`.cursor/rules`, `.cursorrules`, `.github/copilot-instructions.md`) — no extra constraints
- Migrations: use `dotnet ef migrations add Name` and `dotnet ef database update`; keep migrations scoped to persistence changes
