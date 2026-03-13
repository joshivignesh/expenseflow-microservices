using ExpenseFlow.Expense.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Expense.IntegrationTests.Tests;

[CollectionDefinition("ExpenseApi")]
public sealed class ExpenseApiCollection
    : ICollectionFixture<ExpenseApiFactory> { }
