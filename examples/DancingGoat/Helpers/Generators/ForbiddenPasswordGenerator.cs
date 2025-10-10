namespace DancingGoat.Helpers.Generators;

/// <summary>
/// Contains methods for generating forbidden passwords.
/// </summary>
public static class ForbiddenPasswordGenerator
{
    private static readonly List<char> specialChars = ['!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '.', '?', '-', '_', '=', '+', '[', ']', '{', '}', '\\', '|', ';', ':', '\'', '"', ',', '<', '>', '/', '~', '`'];
    private static readonly List<string> numbers = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];

    /// <summary>
    /// Generates forbidden passwords based on company-specific keywords and specific number combinations.
    /// </summary>
    /// <remarks>
    /// The forbidden passwords do not include keywords without special characters and numbers, since these are already blocked by the default password policy.
    /// </remarks>
    /// <param name="companySpecificKeywords">Company specific keywords</param>
    /// <param name="specificNumberCombinations">Specific number combinations</param>
    public static HashSet<string> Generate(List<string> companySpecificKeywords, List<string> specificNumberCombinations)
    {
        var numbersToUse = numbers.Concat(specificNumberCombinations);

        var forbiddenPasswords =
            from keyword in companySpecificKeywords
            from specialChar in specialChars
            from number in numbersToUse
            from forbiddenPassword in new[]
            {
                keyword + specialChar + number,
                keyword + number + specialChar,
                specialChar + keyword + number,
                number + keyword + specialChar
            }
            select forbiddenPassword;

        return [.. forbiddenPasswords];
    }
}
