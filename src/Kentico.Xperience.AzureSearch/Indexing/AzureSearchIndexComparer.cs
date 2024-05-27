using Azure.Search.Documents.Indexes.Models;

namespace Kentico.Xperience.AzureSearch.Indexing;

/// <summary>
/// Implements <see cref="IEqualityComparer{SearchField}"/> used to compare search fields of two indexes.
/// </summary>
internal class AzureSearchIndexComparer : IEqualityComparer<SearchField>
{
    public bool Equals(SearchField? x, SearchField? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if ((x is null) || (y is null))
        {
            return false;
        }

        return x!.IsKey == y!.IsKey &&
            x!.Name == y!.Name &&
            x!.IsSearchable == y!.IsSearchable &&
            x!.IsSortable == y!.IsSortable &&
            x!.Type == y!.Type &&
            x!.AnalyzerName == y!.AnalyzerName &&
            Equals(x!.Fields, y!.Fields) &&
            x!.IndexAnalyzerName == y!.IndexAnalyzerName &&
            x!.IsFacetable == y!.IsFacetable &&
            x!.IsFilterable == y!.IsFilterable &&
            x!.IsHidden == y!.IsHidden &&
            x!.SearchAnalyzerName == y!.SearchAnalyzerName &&
            Equals(x!.SynonymMapNames, y!.SynonymMapNames) &&
            x!.VectorSearchDimensions == y!.VectorSearchDimensions &&
            x!.VectorSearchProfileName == y!.VectorSearchProfileName;
    }

    public int GetHashCode(SearchField obj)
    {
        if (obj == null)
        {
            return 0;
        }

        return obj.Name.GetHashCode();
    }
}
