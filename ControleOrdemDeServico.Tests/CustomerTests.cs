using System.Net;
using System.Net.Http.Json;

namespace OsService.Tests;

public class CustomerTests : ApiTestBase
{
    [Fact]
    public async Task CreateCustomer_WithValidName_Returns201Created()
    {
        var customer = new
        {
            Name = "João Silva",
            Phone = "11999999999",
            Email = "joao@example.com",
            Document = "12345678900"
        };

        var response = await PostAsync("/v1/customers", customer);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);

        var location = response.Headers.Location;
        Assert.NotNull(location);
    }

    [Fact]
    public async Task CreateCustomer_WithoutName_Returns400BadRequest()
    {
        var customer = new
        {
            Name = "",
            Phone = "11999999999",
            Email = "joao@example.com"
        };

        var response = await PostAsync("/v1/customers", customer);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_Returns400BadRequest()
    {
        var customer = new
        {
            Name = "João Silva",
            Email = "invalid-email"
        };

        var response = await PostAsync("/v1/customers", customer);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithPhoneAndSearch_ReturnsConsistentData()
    {
        var customer = new
        {
            Name = "Maria Santos",
            Phone = "21988888888",
            Email = "maria@example.com"
        };

        var createResponse = await PostAsync("/v1/customers", customer);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(createdCustomer);

        var searchResponse = await Client.GetAsync($"/v1/customers/search?phone={customer.Phone}");
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        var foundCustomer = await searchResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.NotNull(foundCustomer);
        Assert.Equal(createdCustomer.Id, foundCustomer.Id);
        Assert.Equal(customer.Name, foundCustomer.Name);
        Assert.Equal(customer.Phone, foundCustomer.Phone);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateDocument_Returns409Conflict()
    {
        var document = "11122233344";
        var customer1 = new
        {
            Name = "Cliente 1",
            Document = document
        };
        var customer2 = new
        {
            Name = "Cliente 2",
            Document = document
        };

        var response1 = await PostAsync("/v1/customers", customer1);
        var response2 = await PostAsync("/v1/customers", customer2);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicatePhone_Returns409Conflict()
    {
        var phone = "11999887766";
        var customer1 = new
        {
            Name = "Cliente 1",
            Phone = phone
        };
        var customer2 = new
        {
            Name = "Cliente 2",
            Phone = phone
        };

        var response1 = await PostAsync("/v1/customers", customer1);
        var response2 = await PostAsync("/v1/customers", customer2);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }
}

public record CustomerResponse(Guid Id, string Name, string? Phone, string? Email, string? Document, DateTime CreatedAt);
