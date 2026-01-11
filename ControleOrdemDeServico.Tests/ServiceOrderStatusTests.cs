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
        // Arrange
        var serviceOrderId = await CreateTestServiceOrder();
        var statusUpdate = new { Status = 1 }; // InProgress = 1

        // Act
        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify status changed
        var getResponse = await Client.GetAsync($"/v1/service-orders/{serviceOrderId}");
        var serviceOrder = await getResponse.Content.ReadFromJsonAsync<ServiceOrderResponse>();
        Assert.NotNull(serviceOrder);
        Assert.Equal(1, serviceOrder.Status);
        Assert.NotNull(serviceOrder.StartedAt); // StartedAt should be set
    }

    [Fact]
    public async Task UpdateStatus_FromInProgressToFinished_Returns200OK()
    {
        // Arrange
        var serviceOrderId = await CreateTestServiceOrder();

        // First, move to InProgress
        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 1 });

        // Set price (required for finishing)
        await PutAsync($"/v1/service-orders/{serviceOrderId}/price", new { Price = 150.00m });

        // Act - Move to Finished
        var statusUpdate = new { Status = 2 }; // Finished = 2
        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify status changed
        var getResponse = await Client.GetAsync($"/v1/service-orders/{serviceOrderId}");
        var serviceOrder = await getResponse.Content.ReadFromJsonAsync<ServiceOrderResponse>();
        Assert.NotNull(serviceOrder);
        Assert.Equal(2, serviceOrder.Status);
        Assert.NotNull(serviceOrder.FinishedAt); // FinishedAt should be set
    }

    [Fact]
    public async Task UpdateStatus_FromFinishedToAny_Returns409Conflict()
    {
        // Arrange
        var serviceOrderId = await CreateTestServiceOrder();

        // Move to InProgress
        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 1 });

        // Set price
        await PutAsync($"/v1/service-orders/{serviceOrderId}/price", new { Price = 150.00m });

        // Move to Finished
        await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", new { Status = 2 });

        // Act - Try to move back to InProgress
        var statusUpdate = new { Status = 1 };
        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_FromOpenToFinished_ReturnsError()
    {
        // Arrange
        var serviceOrderId = await CreateTestServiceOrder();
        var statusUpdate = new { Status = 2 }; // Finished = 2

        // Act - Try to move directly from Open to Finished
        var response = await PatchAsync($"/v1/service-orders/{serviceOrderId}/status", statusUpdate);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
