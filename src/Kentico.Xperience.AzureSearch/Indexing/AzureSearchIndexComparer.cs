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

        bool isKeyEqual = x.IsKey == y.IsKey;
        bool nameEqual = x.Name == y.Name;
        bool isSearchableEqual = x.IsSearchable == y.IsSearchable;
        bool isSortableEqual = x.IsSortable == y.IsSortable;
        bool typeEqual = x.Type == y.Type;
        bool analyzerNameEqual = x.AnalyzerName == y.AnalyzerName;
        bool fieldsEqual = (x.Fields == null && y.Fields == null) ||
                           (x.Fields != null && y.Fields != null && x.Fields.SequenceEqual(y.Fields, this));
        bool indexAnalyzerNameEqual = x.IndexAnalyzerName == y.IndexAnalyzerName;
        bool isFacetableEqual = x.IsFacetable == y.IsFacetable;
        bool isFilterableEqual = x.IsFilterable == y.IsFilterable;
        bool isHiddenEqual = x.IsHidden == y.IsHidden;
        bool searchAnalyzerNameEqual = x.SearchAnalyzerName == y.SearchAnalyzerName;
        bool synonymMapNamesEqual = (x.SynonymMapNames == null && y.SynonymMapNames == null) ||
                                    (x.SynonymMapNames != null && y.SynonymMapNames != null && x.SynonymMapNames.SequenceEqual(y.SynonymMapNames));
        bool vectorSearchDimensionsEqual = x.VectorSearchDimensions == y.VectorSearchDimensions;
        bool vectorSearchProfileNameEqual = x.VectorSearchProfileName == y.VectorSearchProfileName;

        return isKeyEqual &&
               nameEqual &&
               isSearchableEqual &&
               isSortableEqual &&
               typeEqual &&
               analyzerNameEqual &&
               fieldsEqual &&
               indexAnalyzerNameEqual &&
               isFacetableEqual &&
               isFilterableEqual &&
               isHiddenEqual &&
               searchAnalyzerNameEqual &&
               synonymMapNamesEqual &&
               vectorSearchDimensionsEqual &&
               vectorSearchProfileNameEqual;
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
