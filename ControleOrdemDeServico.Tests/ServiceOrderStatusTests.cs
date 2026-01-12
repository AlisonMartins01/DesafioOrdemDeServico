using System.Net;
using System.Net.Http.Json;

namespace OsService.Tests;

public class ServiceOrderStatusTests : ApiTestBase
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
    public async Task UpdateStatus_FromOpenToInProgress_Returns200OK()
    {
        var serviceOrderId = await CreateTestServiceOrder();
        var statusUpdate = new { Status = 2 }; // InProgress = 1

        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await Client.GetAsync($"/v1/service-orders/{serviceOrderId}");
        var serviceOrder = await getResponse.Content.ReadFromJsonAsync<ServiceOrderResponse>();
        Assert.NotNull(serviceOrder);
        Assert.Equal(2, serviceOrder.Status);
        Assert.NotNull(serviceOrder.StartedAt); // StartedAt should be set
    }

    [Fact]
    public async Task UpdateStatus_FromInProgressToFinished_Returns200OK()
    {
        var serviceOrderId = await CreateTestServiceOrder();

        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 2 });

        await PutAsync($"/v1/service-orders/{serviceOrderId}/price", new { Price = 150.00m });

        var statusUpdate = new { Status = 3 }; // Finished = 3
        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await Client.GetAsync($"/v1/service-orders/{serviceOrderId}");
        var serviceOrder = await getResponse.Content.ReadFromJsonAsync<ServiceOrderResponse>();
        Assert.NotNull(serviceOrder);
        Assert.Equal(3, serviceOrder.Status);
        Assert.NotNull(serviceOrder.FinishedAt); // FinishedAt should be set
    }

    [Fact]
    public async Task UpdateStatus_FromFinishedToAny_Returns409Conflict()
    {
        var serviceOrderId = await CreateTestServiceOrder();

        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 2 });

        await PutAsync($"/v1/service-orders/{serviceOrderId}/price", new { Price = 150.00m });

        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 3 });

        var statusUpdate = new { Status = 1 };
        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_FromOpenToFinished_ReturnsError()
    {
        var serviceOrderId = await CreateTestServiceOrder();
        var statusUpdate = new { Status = 3 }; // Finished = 2

        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
