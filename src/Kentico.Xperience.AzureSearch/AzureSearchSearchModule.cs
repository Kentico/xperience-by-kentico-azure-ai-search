using CMS;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.AzureSearch;
using Kentico.Xperience.AzureSearch.Indexing;
using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterModule(typeof(AzureSearchSearchModule))]

namespace Kentico.Xperience.AzureSearch;

/// <summary>
/// Initializes page event handlers, and ensures the thread queue workers for processing AzureSearch tasks.
/// </summary>
internal class AzureSearchSearchModule : Module
{
    private IAzureSearchTaskLogger azuresearchTaskLogger = null!;
    private IAppSettingsService appSettingsService = null!;
    private IConversionService conversionService = null!;

    private const string APP_SETTINGS_KEY_INDEXING_DISABLED = "CMSAzureSearchSearchDisableIndexing";

    private bool IndexingDisabled
    {
        get
        {
            if (appSettingsService[APP_SETTINGS_KEY_INDEXING_DISABLED] is string value1)
            {
                return conversionService.GetBoolean(value1, false);
            }
            return false;
        }
    }

    /// <inheritdoc/>
    public AzureSearchSearchModule() : base(nameof(AzureSearchSearchModule))
    {
    }

    /// <inheritdoc/>
    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        azuresearchTaskLogger = services.GetRequiredService<IAzureSearchTaskLogger>();
        appSettingsService = services.GetRequiredService<IAppSettingsService>();
        conversionService = services.GetRequiredService<IConversionService>();

        WebPageEvents.Publish.Execute += HandleEvent;
        WebPageEvents.Delete.Execute += HandleEvent;
        ContentItemEvents.Publish.Execute += HandleContentItemEvent;
        ContentItemEvents.Delete.Execute += HandleContentItemEvent;

        RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => AzureSearchQueueWorker.Current.EnsureRunningThread();
    }


    /// <summary>
    /// Called when a page is published. Logs an AzureSearch task to be processed later.
    /// </summary>
    private void HandleEvent(object? sender, CMSEventArgs e)
    {
        if (IndexingDisabled || e is not WebPageEventArgsBase publishedEvent)
        {
            return;
        }

        var indexedItemModel = new IndexEventWebPageItemModel(
            publishedEvent.ID,
            publishedEvent.Guid,
            publishedEvent.ContentLanguageName,
            publishedEvent.ContentTypeName,
            publishedEvent.Name,
            publishedEvent.IsSecured,
            publishedEvent.ContentTypeID,
            publishedEvent.ContentLanguageID,
            publishedEvent.WebsiteChannelName,
            publishedEvent.TreePath,
            publishedEvent.ParentID,
            publishedEvent.Order)
        { };

        azuresearchTaskLogger?.HandleEvent(indexedItemModel, e.CurrentHandler.Name).GetAwaiter().GetResult();
    }

    private void HandleContentItemEvent(object? sender, CMSEventArgs e)
    {
        if (IndexingDisabled || e is not ContentItemEventArgsBase publishedEvent)
        {
            return;
        }

        var indexedContentItemModel = new IndexEventReusableItemModel(
            publishedEvent.ID,
            publishedEvent.Guid,
            publishedEvent.ContentLanguageName,
            publishedEvent.ContentTypeName,
            publishedEvent.Name,
            publishedEvent.IsSecured,
            publishedEvent.ContentTypeID,
            publishedEvent.ContentLanguageID
        );

        azuresearchTaskLogger?.HandleReusableItemEvent(indexedContentItemModel, e.CurrentHandler.Name).GetAwaiter().GetResult();
    }
}
