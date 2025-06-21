using CMS;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;

using Kentico.Xperience.AzureSearch;
using Kentico.Xperience.AzureSearch.Indexing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: RegisterModule(typeof(AzureSearchSearchModule))]

namespace Kentico.Xperience.AzureSearch;

/// <summary>
/// Initializes page event handlers, and ensures the thread queue workers for processing AzureSearch tasks.
/// </summary>
internal class AzureSearchSearchModule : Module
{
    private IAzureSearchTaskLogger azureSearchTaskLogger = null!;

    /// <inheritdoc/>
    public AzureSearchSearchModule() : base(nameof(AzureSearchSearchModule))
    {
    }

    /// <inheritdoc/>
    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;
        var options = services.GetRequiredService<IOptions<AzureSearchOptions>>();

        if (!options.Value?.SearchServiceEnabled ?? false)
        {
            return;
        }

        azureSearchTaskLogger = services.GetRequiredService<IAzureSearchTaskLogger>();

        WebPageEvents.Publish.Execute += HandleEvent;
        WebPageEvents.Delete.Execute += HandleEvent;
        WebPageEvents.Unpublish.Execute += HandleEvent;
        ContentItemEvents.Unpublish.Execute += HandleContentItemEvent;
        ContentItemEvents.Publish.Execute += HandleContentItemEvent;
        ContentItemEvents.Delete.Execute += HandleContentItemEvent;

        RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => AzureSearchQueueWorker.Current.EnsureRunningThread();
    }

    /// <summary>
    /// Called when a page is published. Logs an AzureSearch task to be processed later.
    /// </summary>
    private void HandleEvent(object? sender, CMSEventArgs e)
    {
        if (e is not WebPageEventArgsBase publishedEvent)
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

        azureSearchTaskLogger?.HandleEvent(indexedItemModel, e.CurrentHandler.Name).GetAwaiter().GetResult();
    }

    private void HandleContentItemEvent(object? sender, CMSEventArgs e)
    {
        if (e is not ContentItemEventArgsBase publishedEvent)
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

        azureSearchTaskLogger?.HandleReusableItemEvent(indexedContentItemModel, e.CurrentHandler.Name).GetAwaiter().GetResult();
    }
}
