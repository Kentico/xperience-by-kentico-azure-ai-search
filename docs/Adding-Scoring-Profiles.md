# Adding scoring profiles to Azure Search indexes

Azure Search computes a search score for every item returned in search results. The score indicates an item’s relevance in the context of the given search operation and determines the order of the item in the set of search results.

You can adjust the default scoring for a search index by adding a scoring profile. Scoring profiles allow you to increase the scoring weight of fields, or boost items based on specific field values and calculations.

For detailed information, refer to the [Add scoring profiles to a search index](https://docs.microsoft.com/en-us/rest/api/searchservice/add-scoring-profiles-to-a-search-index) article.

To add a scoring profile for an Azure Search index managed by Xperience, you need to customize the functionality that Xperience uses to build the indexes:

1. Open your Xperience solution in Visual Studio.
2. Create a custom module class.
3. Override the module’s `OnInit` method and assign a handler to the `AzureSearchIndexingEvents.BeforeCreatingOrUpdatingIndex.Execute` event.
4. Perform the following in the event’s handler method:
    - a. Access the `Azure.Search.Documents.Indexes.Models.SearchIndex` object representing the processed index via the `SearchIndex` property of the handler’s `OnBeforeCreatingOrUpdatingIndexEventArgs` parameter.

    - b. Write conditions to assign different scoring profiles for specific indexes.

    - c. Prepare a `Azure.Search.Documents.Indexes.Models.ScoringProfile` object according to your requirements and add it to the `ScoringProfiles` list of the processed index.

5. Open the the Search application added by this library. Newly created or edited indexes will use your custom module logic. Already existing indexes for which you want to assign a scoring profile need to be edited in the administration. It is enough to open each index edit page and saving without doing any changes. The system creates the customized Azure AI Search indexes with the specified scoring profile. You can see the profile when viewing index details in the Microsoft Azure portal.

## Example

The following example demonstrates how to create a basic scoring profile for an Azure Search index. The sample scoring profile increases the weight of the `Title` field.

Define a custom `Module` implementation which creates a `titleprofile` scoring profile on index named `advanced`.

```csharp
[assembly: RegisterModule(typeof(CustomAzureScoringProfileModule))]

namespace DancingGoat.Search;

public class CustomAzureScoringProfileModule : Module
{
    public const string TITLE_SCORING_PROFILE_NAME = "titleprofile";

    public CustomAzureScoringProfileModule() : base(nameof(CustomAzureScoringProfileModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        AzureSearchIndexingEvents.BeforeCreatingOrUpdatingIndex.Execute += AddScoringProfile;
    }

    private void AddScoringProfile(object sender, OnBeforeCreatingOrUpdatingIndexEventArgs e)
    {
        var index = e.SearchIndex;

        // Ends the handler method if the index name is not 'advanced'
        if (!index.Name.Equals("advanced", StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        // Creates a dictionary containing the index's fields
        var indexFields = index.Fields.ToDictionary(f => f.Name, StringComparer.InvariantCultureIgnoreCase);

        // Used to determine whether a new scoring profile was created and needs to be added to the index
        bool newScoringProfile = false;

        // Checks whether the index already contains a scoring profile named 'titleprofile'
        ScoringProfile scoringProfile = index.ScoringProfiles.FirstOrDefault(sp => sp.Name == TITLE_SCORING_PROFILE_NAME);

        // Creates a new scoring profile if it does not exist
        if (scoringProfile == null)
        {
            scoringProfile = new ScoringProfile(TITLE_SCORING_PROFILE_NAME)
            {
                FunctionAggregation = ScoringFunctionAggregation.Sum,
                TextWeights = new TextWeights(new Dictionary<string, double>())
            };
            // A new scoring profile was created, it needs to be added to the index
            newScoringProfile = true;
        }

        // Confirms that the index contains the 'skuname' field and its weight is not set yet in the scoring profile
        // Note: The 'skuname' field must be configured as 'searchable'
        if (indexFields.ContainsKey(nameof(DancingGoatSearchModel.Title)) && !scoringProfile.TextWeights.Weights.ContainsKey(nameof(DancingGoatSearchModel.Title)))
        {
            // Increases the scoring weight to '3' for search items with matches in the 'skuname' field
            scoringProfile.TextWeights.Weights.Add(nameof(DancingGoatSearchModel.Title), 3);
        }

        // If a new scoring profile was created and is not empty, adds it to the index
        if (newScoringProfile && scoringProfile.TextWeights.Weights.Count > 0)
        {
            index.ScoringProfiles.Add(scoringProfile);
        }
    }
}
```

The `advanced` Azure Search index now contains the `titleprofile` scoring profile with the specified parameters. You can use the profile to adjust the relevance of search results – specify the profile name in the `SearchOptions` of your search requests.

```csharp
public class DancingGoatSearchService
{
    // ...

    public async Task<DancingGoatSearchViewModel> GlobalSearch(
        string indexName,
        string searchText,
        int page = 1,
        int pageSize = 10)
    {

        // ...

        var options = new SearchOptions()
        {
            IncludeTotalCount = true,
            Size = pageSize,
            Skip = (page - 1) * pageSize
        };

        // Optionally use the custom scoring profile for title boosting
        if (indexName == "advanced")
        {
            options.ScoringProfile = CustomAzureScoringProfileModule.TITLE_SCORING_PROFILE_NAME;
        }

        // ...
    }

    // ...
}
```