# =============================================================================
# Stage 1 — base: lightweight ASP.NET runtime image
# Used by both the final image and as the restore cache layer anchor.
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# =============================================================================
# Stage 2 — build: SDK image with full toolchain
# Copies only .csproj files first so NuGet restore is cached independently.
# Code changes don't invalidate the restore cache layer.
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# --- Restore (cache-friendly layer) ------------------------------------------
COPY ["shared/ExpenseFlow.Shared/ExpenseFlow.Shared.csproj",                                                                       "shared/ExpenseFlow.Shared/"]
COPY ["src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.Domain/ExpenseFlow.Identity.Domain.csproj",                          "src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.Domain/"]
COPY ["src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.Application/ExpenseFlow.Identity.Application.csproj",                "src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.Application/"]
COPY ["src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.Infrastructure/ExpenseFlow.Identity.Infrastructure.csproj",          "src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.Infrastructure/"]
COPY ["src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.API/ExpenseFlow.Identity.API.csproj",                                "src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.API/"]

RUN dotnet restore "src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.API/ExpenseFlow.Identity.API.csproj"

# --- Build -------------------------------------------------------------------
COPY . .
RUN dotnet build "src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.API/ExpenseFlow.Identity.API.csproj" \
    -c Release -o /app/build

# =============================================================================
# Stage 3 — publish: produce self-contained release output
# =============================================================================
FROM build AS publish
RUN dotnet publish "src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.API/ExpenseFlow.Identity.API.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# =============================================================================
# Stage 4 — final: minimal runtime-only image shipped to registry
# No SDK, no source code, no secrets — just the compiled app.
# =============================================================================
FROM base AS final
WORKDIR /app

# Run as non-root for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpenseFlow.Identity.API.dll"]
