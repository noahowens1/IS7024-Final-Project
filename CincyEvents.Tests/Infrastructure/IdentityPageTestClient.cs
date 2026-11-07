using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using Xunit;

namespace CincyEvents.Tests.Infrastructure;

public static partial class IdentityPageTestClient
{
    public static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string pagePath)
    {
        using var response = await client.GetAsync(pagePath);
        var html = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"GET {pagePath} returned {(int)response.StatusCode}: {html}");
        var input = InputTagRegex().Matches(html)
            .Select(match => match.Value)
            .FirstOrDefault(tag => NameAttributeRegex().Match(tag) is { Success: true } nameMatch
                && nameMatch.Groups[1].Value == "__RequestVerificationToken");
        Assert.NotNull(input);

        var value = ValueAttributeRegex().Match(input!);
        Assert.True(value.Success, "The antiforgery input did not include a value.");
        return HttpUtility.HtmlDecode(value.Groups[1].Value);
    }

    public static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client,
        string requestPath,
        string antiforgeryPagePath,
        IDictionary<string, string> fields)
    {
        var values = new Dictionary<string, string>(fields)
        {
            ["__RequestVerificationToken"] =
                await GetAntiforgeryTokenAsync(client, antiforgeryPagePath)
        };
        return await client.PostAsync(requestPath, new FormUrlEncodedContent(values));
    }

    public static async Task<HttpResponseMessage> RegisterAsync(
        HttpClient client,
        string email,
        string password = "Student1!")
    {
        return await PostFormAsync(
            client,
            "/Identity/Account/Register",
            "/Identity/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = password,
                ["Input.ConfirmPassword"] = password
            });
    }

    public static async Task<HttpResponseMessage> LogoutAsync(HttpClient client)
    {
        return await PostFormAsync(
            client,
            "/Identity/Account/Logout?returnUrl=%2F",
            "/Events",
            new Dictionary<string, string>());
    }

    [GeneratedRegex("<input\\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex InputTagRegex();

    [GeneratedRegex("\\bname=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NameAttributeRegex();

    [GeneratedRegex("\\bvalue=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ValueAttributeRegex();
}
