using Microsoft.Extensions.Options;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Validates <see cref="AzureSearchOptions"/> so that an invalid configuration fails on application startup.
/// </summary>
internal sealed class AzureSearchOptionsValidator : IValidateOptions<AzureSearchOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AzureSearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        string? prefixError = AzureSearchIndexNameResolver.GetPrefixValidationError(options.IndexNamePrefix);

        return prefixError is null
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(prefixError);
    }
}
