using System.Net;
using System.Net.Http.Json;

namespace OsService.Tests;

public class ServiceOrderTests : ApiTestBase
{
    private async Task<Guid> CreateTestCustomer()
    {
        var customer = new { Name = "Test Customer" };
        var response = await PostAsync("/v1/customers", customer);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        return result!.Id;
    }

    [Fact]
    public async Task OpenServiceOrder_ForExistingCustomer_Returns201Created()
    {
        var customerId = await CreateTestCustomer();
        var serviceOrder = new
        {
            CustomerId = customerId,
            Description = "Instalação de ar condicionado"
        };

        var response = await PostAsync("/v1/service-orders", serviceOrder);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ServiceOrderCreateResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.Number >= 1000); // Number starts at 1000
    }

    [Fact]
    public async Task OpenServiceOrder_ForNonExistentCustomer_Returns404NotFound()
    {
        var serviceOrder = new
        {
            CustomerId = Guid.NewGuid(),
            Description = "Instalação de ar condicionado"
        };

        var response = await PostAsync("/v1/service-orders", serviceOrder);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OpenServiceOrder_WithEmptyDescription_Returns400BadRequest()
    {
        var customerId = await CreateTestCustomer();
        var serviceOrder = new
        {
            CustomerId = customerId,
            Description = ""
        };

        var response = await PostAsync("/v1/service-orders", serviceOrder);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetServiceOrder_AfterCreation_ReturnsStatusOpen()
    {
        var customerId = await CreateTestCustomer();
        var serviceOrder = new
        {
            CustomerId = customerId,
            Description = "Manutenção preventiva"
        };

        var createResponse = await PostAsync("/v1/service-orders", serviceOrder);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceOrderCreateResponse>();

        var getResponse = await Client.GetAsync($"/v1/service-orders/{created!.Id}");
        var retrieved = await getResponse.Content.ReadFromJsonAsync<ServiceOrderResponse>();

        Assert.NotNull(retrieved);
        Assert.Equal(1, retrieved.Status); // Open = 0
        Assert.Equal(serviceOrder.Description, retrieved.Description);
    }
}

public record ServiceOrderCreateResponse(Guid Id, int Number);
public record ServiceOrderResponse(
    Guid Id,
    int Number,
    Guid CustomerId,
    string Description,
    int Status,
    DateTime OpenedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    decimal? Price,
    string? Coin,
    DateTime? UpdatedPriceAt
);
