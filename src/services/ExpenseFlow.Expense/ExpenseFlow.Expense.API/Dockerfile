# =============================================================================
# Stage 1 — base
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# =============================================================================
# Stage 2 — build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# --- Restore (cache-friendly layer) ------------------------------------------
COPY ["shared/ExpenseFlow.Shared/ExpenseFlow.Shared.csproj",                                                                    "shared/ExpenseFlow.Shared/"]
COPY ["src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.Domain/ExpenseFlow.Expense.Domain.csproj",                          "src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.Domain/"]
COPY ["src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.Application/ExpenseFlow.Expense.Application.csproj",                "src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.Application/"]
COPY ["src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.Infrastructure/ExpenseFlow.Expense.Infrastructure.csproj",          "src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.Infrastructure/"]
COPY ["src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.API/ExpenseFlow.Expense.API.csproj",                                "src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.API/"]

RUN dotnet restore "src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.API/ExpenseFlow.Expense.API.csproj"

# --- Build -------------------------------------------------------------------
COPY . .
RUN dotnet build "src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.API/ExpenseFlow.Expense.API.csproj" \
    -c Release -o /app/build

# =============================================================================
# Stage 3 — publish
# =============================================================================
FROM build AS publish
RUN dotnet publish "src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.API/ExpenseFlow.Expense.API.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# =============================================================================
# Stage 4 — final
# =============================================================================
FROM base AS final
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpenseFlow.Expense.API.dll"]
