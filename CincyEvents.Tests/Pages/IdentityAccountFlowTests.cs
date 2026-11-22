using System.Net;
using CincyEvents.Tests.Infrastructure;
using Xunit;

namespace CincyEvents.Tests.Pages;

public sealed class IdentityAccountFlowTests
{
    [Fact]
    public async Task Registration_login_and_logout_use_the_standard_identity_pages()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var email = $"identity-{Guid.NewGuid():N}@example.com";

        using var registration = await IdentityPageTestClient.RegisterAsync(client, email);
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);
        using var savedEventsPage = await client.GetAsync("/SavedEvents");
        Assert.Equal(HttpStatusCode.OK, savedEventsPage.StatusCode);

        using var logout = await IdentityPageTestClient.LogoutAsync(client);
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        using var anonymousSavedEventsPage = await client.GetAsync("/SavedEvents");
        Assert.Equal(HttpStatusCode.Redirect, anonymousSavedEventsPage.StatusCode);
        Assert.Contains("/Identity/Account/Login", anonymousSavedEventsPage.Headers.Location?.ToString());

        using var loginResponse = await IdentityPageTestClient.PostFormAsync(
            client,
            "/Identity/Account/Login",
            "/Identity/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = "Incorrect1!"
            });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Contains(
            "Invalid login attempt",
            await loginResponse.Content.ReadAsStringAsync(),
            StringComparison.OrdinalIgnoreCase);

        using var validLogin = await IdentityPageTestClient.PostFormAsync(
            client,
            "/Identity/Account/Login",
            "/Identity/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = "Student1!"
            });
        Assert.Equal(HttpStatusCode.Redirect, validLogin.StatusCode);
        using var authenticatedPage = await client.GetAsync("/SavedEvents");
        Assert.Equal(HttpStatusCode.OK, authenticatedPage.StatusCode);
    }

    [Fact]
    public async Task Registration_rejects_invalid_input_with_validation_feedback()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        using var response = await IdentityPageTestClient.PostFormAsync(
            client,
            "/Identity/Account/Register",
            "/Identity/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.Email"] = "",
                ["Input.Password"] = "x",
                ["Input.ConfirmPassword"] = "x"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("required", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }
}
