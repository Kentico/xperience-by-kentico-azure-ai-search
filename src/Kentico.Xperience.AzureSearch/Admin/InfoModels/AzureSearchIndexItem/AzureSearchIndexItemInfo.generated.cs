using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterObjectType(typeof(AzureSearchIndexItemInfo), AzureSearchIndexItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Data container class for <see cref="AzureSearchIndexItemInfo"/>.
/// </summary>
[Serializable]
public partial class AzureSearchIndexItemInfo : AbstractInfo<AzureSearchIndexItemInfo, IInfoProvider<AzureSearchIndexItemInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoazuresearch.azuresearchindexitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<AzureSearchIndexItemInfo>), OBJECT_TYPE, "KenticoAzureSearch.AzureSearchIndexItem", nameof(AzureSearchIndexItemId), null, nameof(AzureSearchIndexItemGuid), nameof(AzureSearchIndexItemIndexName), null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// AzureSearch index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIndexItemId)), 0);
        set => SetValue(nameof(AzureSearchIndexItemId), value);
    }


    /// <summary>
    /// AzureSearch index item Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AzureSearchIndexItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AzureSearchIndexItemGuid)), default);
        set => SetValue(nameof(AzureSearchIndexItemGuid), value);
    }


    /// <summary>
    /// Index name.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchIndexItemIndexName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchIndexItemIndexName)), String.Empty);
        set => SetValue(nameof(AzureSearchIndexItemIndexName), value);
    }


    /// <summary>
    /// Channel name.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchIndexItemChannelName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchIndexItemChannelName)), String.Empty);
        set => SetValue(nameof(AzureSearchIndexItemChannelName), value);
    }


    /// <summary>
    /// Strategy name.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchIndexItemStrategyName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchIndexItemStrategyName)), String.Empty);
        set => SetValue(nameof(AzureSearchIndexItemStrategyName), value);
    }


    /// <summary>
    /// Rebuild hook.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchIndexItemRebuildHook
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchIndexItemRebuildHook)), String.Empty);
        set => SetValue(nameof(AzureSearchIndexItemRebuildHook), value, String.Empty);
    }


    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }


    /// <summary>
    /// Updates the object using appropriate provider.
    /// </summary>
    protected override void SetObject()
    {
        Provider.Set(this);
    }


    /// <summary>
    /// Constructor for de-serialization.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected AzureSearchIndexItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AzureSearchIndexItemInfo"/> class.
    /// </summary>
    public AzureSearchIndexItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AzureSearchIndexItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AzureSearchIndexItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
