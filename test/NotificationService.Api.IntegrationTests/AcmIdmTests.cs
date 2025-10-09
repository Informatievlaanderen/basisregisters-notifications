namespace NotificationService.Api.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

[Collection("DockerFixtureCollection")]
public class AcmIdmTests : IClassFixture<NotificationServiceTestFixture>
{
    private readonly NotificationServiceTestFixture _fixture;

    public AcmIdmTests(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static readonly Endpoint[] Endpoints =
    {
        new(HttpMethod.Delete, "v1/notificaties/{id}", "v1/notificaties/1", "dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen dv_wr_uitzonderingen"),
        new(HttpMethod.Post, "v1/notificaties", "v1/notificaties", "dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen dv_wr_uitzonderingen"),
        new(HttpMethod.Post, "v1/notificaties/{id}/acties/publiceren", "v1/notificaties/1/acties/publiceren", "dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen dv_wr_uitzonderingen"),
        new(HttpMethod.Post, "v1/notificaties/{id}/acties/intrekken", "v1/notificaties/1/acties/intrekken", "dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen dv_wr_uitzonderingen"),
        new(HttpMethod.Get, "v1/notificaties/{platform}", "v1/notificaties/lara", "dv_ar_adres_beheer"),
        new(HttpMethod.Get, "v1/notificaties", "v1/notificaties", "dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen dv_wr_uitzonderingen"),
    };
    private sealed record Endpoint(HttpMethod Method, string TemplateUrl, string ExampleUrl, string RequiredScopes);
    public static IEnumerable<object[]> EndpointsMemberData() => Endpoints.Select(x => new object[] { x.Method, x.ExampleUrl, x.RequiredScopes });

    [Fact]
    public void EnsureAllEndpointsAreTested()
    {
        var apiExplorer = _fixture.TestServer.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

        var apiDescriptions = apiExplorer.ApiDescriptionGroups.Items.SelectMany(apiDescriptionGroup => apiDescriptionGroup.Items).ToList();
        Assert.NotEmpty(apiDescriptions);

        var missingEndpoints = new List<ApiDescription>();

        foreach (var apiDescription in apiDescriptions)
        {
            var testEndpoint = Endpoints.SingleOrDefault(x =>
                x.Method.ToString().Equals(apiDescription.HttpMethod, StringComparison.InvariantCultureIgnoreCase)
                && x.TemplateUrl.Equals(apiDescription.RelativePath, StringComparison.InvariantCultureIgnoreCase));
            if (testEndpoint is null)
            {
                missingEndpoints.Add(apiDescription);
            }
        }

        if (missingEndpoints.Any())
        {
            Assert.Fail($"Endpoints have no matching test:{Environment.NewLine}{string.Join(Environment.NewLine, missingEndpoints.Select(x => $"{x.HttpMethod} {x.RelativePath}"))}");
        }
    }

    [Theory]
    [MemberData(nameof(EndpointsMemberData))]
    public Task ReturnsSuccess(HttpMethod method, string endpoint, string requiredScopes)
    {
        return RetryMultipleTimesUntilNoXunitException(async () =>
        {
            var client = _fixture.TestServer.CreateClient();
            if (!string.IsNullOrEmpty(requiredScopes))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(requiredScopes));
            }

            var response = await SendAsync(client, CreateRequestMessage(method, endpoint));
            Assert.NotNull(response);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        });
    }

    [Theory]
    [MemberData(nameof(EndpointsMemberData))]
    public Task ReturnsUnauthorized(HttpMethod method, string endpoint, string requiredScopes)
    {
        return RetryMultipleTimesUntilNoXunitException(async () =>
        {
            var client = _fixture.TestServer.CreateClient();

            var response = await SendAsync(client, CreateRequestMessage(method, endpoint));
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Theory]
    [MemberData(nameof(EndpointsMemberData))]
    public Task ReturnsForbidden(HttpMethod method, string endpoint, string requiredScopes)
    {
        return RetryMultipleTimesUntilNoXunitException(async () =>
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken());

            var response = await SendAsync(client, CreateRequestMessage(method, endpoint));
            Assert.NotNull(response);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        });
    }

    private static HttpRequestMessage CreateRequestMessage(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, new Uri(endpoint, UriKind.RelativeOrAbsolute));
        switch (method.ToString())
        {
            case nameof(HttpMethod.Post):
            case nameof(HttpMethod.Put):
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                break;
        }

        return request;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request)
    {
        var response = await client.SendAsync(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);

        return response;
    }

    private static async Task RetryMultipleTimesUntilNoXunitException(Func<Task> action, int retryTimes = 5)
    {
        for (var i = 0; i < retryTimes; i++)
        {
            try
            {
                await action();
                return;
            }
            catch (XunitException)
            {
                if (i == retryTimes - 1)
                {
                    throw;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
