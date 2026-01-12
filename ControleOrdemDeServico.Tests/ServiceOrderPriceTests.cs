using System.Net;
using System.Net.Http.Json;

namespace OsService.Tests;

public class ServiceOrderPriceTests : ApiTestBase
{
    private async Task<Guid> CreateTestCustomer()
    {
        var customer = new { Name = "Test Customer" };
        var response = await PostAsync("/v1/customers", customer);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        return result!.Id;
    }

    private async Task<Guid> CreateTestServiceOrder()
    {
        var customerId = await CreateTestCustomer();
        var serviceOrder = new
        {
            CustomerId = customerId,
            Description = "Test Service Order"
        };
        var response = await PostAsync("/v1/service-orders", serviceOrder);
        var result = await response.Content.ReadFromJsonAsync<ServiceOrderCreateResponse>();
        return result!.Id;
    }

    [Fact]
    public async Task UpdatePrice_WithValidValue_Returns200OK()
    {
        var serviceOrderId = await CreateTestServiceOrder();
        var priceUpdate = new { Price = 250.50m };

        var response = await PutAsync($"/v1/service-orders/{serviceOrderId}/price", priceUpdate);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await Client.GetAsync($"/v1/service-orders/{serviceOrderId}");
        var serviceOrder = await getResponse.Content.ReadFromJsonAsync<ServiceOrderResponse>();
        Assert.NotNull(serviceOrder);
        Assert.Equal(250.50m, serviceOrder.Price);
        Assert.NotNull(serviceOrder.UpdatedPriceAt);
    }

    [Fact]
    public async Task UpdatePrice_WithNegativeValue_Returns400BadRequest()
    {
        var serviceOrderId = await CreateTestServiceOrder();
        var priceUpdate = new { Price = -100.00m };

        var response = await PutAsync($"/v1/service-orders/{serviceOrderId}/price", priceUpdate);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FinishServiceOrder_WithoutPrice_Returns409Conflict()
    {
        var serviceOrderId = await CreateTestServiceOrder();

        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 1 });

        var statusUpdate = new { Status = 2 }; // Finished = 2
        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePrice_AfterFinished_Returns409Conflict()
    {
        var serviceOrderId = await CreateTestServiceOrder();

        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 1 });

        await PutAsync($"/v1/service-orders/{serviceOrderId}/price", new { Price = 200.00m });

        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 2 });

        var priceUpdate = new { Price = 300.00m };
        var response = await PutAsync($"/v1/service-orders/{serviceOrderId}/price", priceUpdate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
