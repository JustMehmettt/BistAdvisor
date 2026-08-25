using System.Net;
using System.Net.Http.Json;
using BistAdvisor.Application.Dtos;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BistAdvisor.Tests.Api;

public class StocksApiTests
{
    private static async Task SeedStocksAsync(CustomWebApplicationFactory factory, int count)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        for (var i = 1; i <= count; i++)
        {
            context.Stocks.Add(new Stock
            {
                Symbol = $"TST{i}",
                ProviderSymbol = $"TST{i}.IS",
                CompanyName = $"Test Company {i}",
                Sector = i % 2 == 0 ? "Bankacılık" : "İmalat",
                Market = "BIST",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetStocks_ReturnsCorrectPageSize()
    {
        using var factory = new CustomWebApplicationFactory();
        await SeedStocksAsync(factory, 15);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/stocks?page=1&pageSize=5");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<StockDto>>();

        Assert.NotNull(result);
        Assert.Equal(5, result!.Items.Count);
        Assert.Equal(15, result.TotalCount);
    }

    [Fact]
    public async Task GetStocks_FiltersBySector()
    {
        using var factory = new CustomWebApplicationFactory();
        await SeedStocksAsync(factory, 10);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/stocks?sector=Bankacılık&pageSize=50");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<StockDto>>();

        Assert.NotNull(result);
        Assert.All(result!.Items, item => Assert.Equal("Bankacılık", item.Sector));
    }

    [Fact]
    public async Task GetStockBySymbol_WithUnknownSymbol_ReturnsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/stocks/UNKNOWN123");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStockBySymbol_WithKnownSymbol_ReturnsOk()
    {
        using var factory = new CustomWebApplicationFactory();
        await SeedStocksAsync(factory, 1);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/stocks/TST1");

        response.EnsureSuccessStatusCode();
        var stock = await response.Content.ReadFromJsonAsync<StockDto>();

        Assert.NotNull(stock);
        Assert.Equal("TST1", stock!.Symbol);
    }
}