using Azure.Search.Documents.Indexes.Models;

using CMS;
using CMS.Core;
using CMS.DataEngine;

using DancingGoat.Search;
using DancingGoat.Search.Models;

using Kentico.Xperience.AzureSearch.Indexing;

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

        // Confirms that the index contains the 'Title' field and its weight is not set yet in the scoring profile
        // Note: The 'Title' field must be configured as 'searchable'
        if (indexFields.ContainsKey(nameof(DancingGoatSearchModel.Title)) && !scoringProfile.TextWeights.Weights.ContainsKey(nameof(DancingGoatSearchModel.Title)))
        {
            // Increases the scoring weight to '3' for search items with matches in the 'Title' field
            scoringProfile.TextWeights.Weights.Add(nameof(DancingGoatSearchModel.Title), 3);
        }

        // If a new scoring profile was created and is not empty, adds it to the index
        if (newScoringProfile && scoringProfile.TextWeights.Weights.Count > 0)
        {
            index.ScoringProfiles.Add(scoringProfile);
        }
    }
}
