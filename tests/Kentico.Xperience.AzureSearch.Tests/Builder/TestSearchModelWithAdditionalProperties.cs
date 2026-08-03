using System.Text.Json.Serialization;

using Azure.Search.Documents.Indexes;

using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests;

// Test search model with additional properties that don't have field attributes
internal class TestSearchModelWithAdditionalProperties : BaseAzureSearchModel
{
    [SearchableField]
    public string Title { get; set; } = string.Empty;


    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalFields { get; set; } = [];


    public DateTime IndexedAt { get; set; }
}
