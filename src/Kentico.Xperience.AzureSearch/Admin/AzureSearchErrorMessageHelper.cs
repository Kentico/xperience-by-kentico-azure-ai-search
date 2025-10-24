using Azure;

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Helper class for generating user-friendly error messages from Azure Search service exceptions.
/// </summary>
internal static class AzureSearchErrorMessageHelper
{
    /// <summary>
    /// Generates a user-friendly error message based on the Azure Search service exception status code.
    /// </summary>
    /// <param name="ex">The RequestFailedException from Azure Search service.</param>
    /// <param name="indexName">The name of the index that was being operated on.</param>
    /// <param name="operation">The operation that was being performed (e.g., "rebuilding", "deleting").</param>
    public static string GetErrorMessage(RequestFailedException ex, string indexName, string operation = "operating on")
    => ex.Status switch
    {
        404 => string.Format("Index '{0}' was not found in Azure Search.", indexName),
        403 => string.Format("Access denied when {0} index '{1}'. Please check your Azure Search service permissions.", operation, indexName),
        409 => string.Format("Index '{0}' is currently being modified by another operation. Please try again later.", indexName),
        412 => string.Format("Index '{0}' has been modified since the {1} operation started. Please try again.", indexName, operation),
        429 => string.Format("Azure Search service is currently throttling requests. Please try {0} index '{1}' again later.", operation, indexName),
        500 => string.Format("Azure Search service encountered an internal error while {0} index '{1}'. Please try again later.", operation, indexName),
        503 => string.Format("Azure Search service is temporarily unavailable. Please try {0} index '{1}' again later.", operation, indexName),
        _ => string.Format("Failed to {0} index '{1}' due to an Azure Search service error (Status: {2}). Please check the Event Log for details.", operation, indexName, ex.Status)
    };
}
