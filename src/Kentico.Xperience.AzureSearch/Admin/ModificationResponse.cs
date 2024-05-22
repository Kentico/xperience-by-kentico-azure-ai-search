namespace Kentico.Xperience.AzureSearch.Admin;

public class ModificationResponse
{
    public ModificationResult ModificationResult { get; set; }
    public List<string>? ErrorMessages { get; set; }

    public ModificationResponse(ModificationResult result, List<string>? errorMessage = null)
    {
        ModificationResult = result;
        ErrorMessages = errorMessage;
    }
}
