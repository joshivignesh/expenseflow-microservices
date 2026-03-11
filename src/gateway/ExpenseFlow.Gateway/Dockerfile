# Stage 1 — base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2 — build (cache NuGet restore separately)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/gateway/ExpenseFlow.Gateway/ExpenseFlow.Gateway.csproj", "src/gateway/ExpenseFlow.Gateway/"]
RUN dotnet restore "src/gateway/ExpenseFlow.Gateway/ExpenseFlow.Gateway.csproj"

COPY . .
RUN dotnet build "src/gateway/ExpenseFlow.Gateway/ExpenseFlow.Gateway.csproj" \
    -c Release -o /app/build

# Stage 3 — publish
FROM build AS publish
RUN dotnet publish "src/gateway/ExpenseFlow.Gateway/ExpenseFlow.Gateway.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# Stage 4 — final (runtime only, non-root user)
FROM base AS final
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpenseFlow.Gateway.dll"]
