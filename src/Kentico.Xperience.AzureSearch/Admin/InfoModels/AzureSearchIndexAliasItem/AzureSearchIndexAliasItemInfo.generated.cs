using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterObjectType(typeof(AzureSearchIndexAliasItemInfo), AzureSearchIndexAliasItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Data container class for <see cref="AzureSearchIndexAliasItemInfo"/>.
/// </summary>
[Serializable]
public partial class AzureSearchIndexAliasItemInfo : AbstractInfo<AzureSearchIndexAliasItemInfo, IInfoProvider<AzureSearchIndexAliasItemInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoazuresearch.azuresearchindexaliasitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<AzureSearchIndexAliasItemInfo>), OBJECT_TYPE, "KenticoAzureSearch.AzureSearchIndexAliasItem", nameof(AzureSearchIndexAliasItemId), null, nameof(AzureSearchIndexAliasItemGuid), nameof(AzureSearchIndexAliasItemIndexAliasName), null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// AzureSearch indexalias item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIndexAliasItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIndexAliasItemId)), 0);
        set => SetValue(nameof(AzureSearchIndexAliasItemId), value);
    }


    /// <summary>
    /// AzureSearch indexalias item Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AzureSearchIndexAliasItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AzureSearchIndexAliasItemGuid)), default);
        set => SetValue(nameof(AzureSearchIndexAliasItemGuid), value);
    }


    /// <summary>
    /// IndexAlias name.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchIndexAliasItemIndexAliasName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchIndexAliasItemIndexAliasName)), String.Empty);
        set => SetValue(nameof(AzureSearchIndexAliasItemIndexAliasName), value);
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
    protected AzureSearchIndexAliasItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AzureSearchIndexAliasItemInfo"/> class.
    /// </summary>
    public AzureSearchIndexAliasItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AzureSearchIndexAliasItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AzureSearchIndexAliasItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
