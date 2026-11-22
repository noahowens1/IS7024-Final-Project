using System.ComponentModel.DataAnnotations;

namespace CincyEvents.Models;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is null
            || value is string url
                && Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && !string.IsNullOrWhiteSpace(uri.Host)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be an absolute HTTP or HTTPS URL.";
    }
}
