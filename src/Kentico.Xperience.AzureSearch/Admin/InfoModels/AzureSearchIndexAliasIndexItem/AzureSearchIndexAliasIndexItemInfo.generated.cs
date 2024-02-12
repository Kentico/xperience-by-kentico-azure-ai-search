using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterObjectType(typeof(AzureSearchIndexAliasIndexItemInfo), AzureSearchIndexAliasIndexItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Data container class for <see cref="AzureSearchIndexAliasIndexItemInfo"/>.
/// </summary>
[Serializable]
public partial class AzureSearchIndexAliasIndexItemInfo : AbstractInfo<AzureSearchIndexAliasIndexItemInfo, IAzureSearchIndexAliasIndexItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoazuresearch.azuresearchindexaliasindexitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(AzureSearchIndexAliasIndexItemInfoProvider), OBJECT_TYPE, "KenticoAzureSearch.AzureSearchIndexAliasIndexItem", nameof(AzureSearchIndexAliasIndexItemId), null, nameof(AzureSearchIndexAliasIndexItemGuid), nameof(AzureSearchIndexAliasIndexItemIndexAliasId), null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(AzureSearchIndexAliasIndexItemIndexAliasId), AzureSearchIndexAliasItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            new(nameof(AzureSearchIndexAliasIndexItemIndexItemId), AzureSearchIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        }
    };


    /// <summary>
    /// AzureSearch indexalias item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIndexAliasIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIndexAliasIndexItemId)), 0);
        set => SetValue(nameof(AzureSearchIndexAliasIndexItemId), value);
    }


    /// <summary>
    /// AzureSearch indexalias item Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AzureSearchIndexAliasIndexItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AzureSearchIndexAliasIndexItemGuid)), default);
        set => SetValue(nameof(AzureSearchIndexAliasIndexItemGuid), value);
    }


    /// <summary>
    /// IndexAlias name.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIndexAliasIndexItemIndexAliasId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIndexAliasIndexItemIndexAliasId)), 0);
        set => SetValue(nameof(AzureSearchIndexAliasIndexItemIndexAliasId), value);
    }


    /// <summary>
    /// Strategy name.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIndexAliasIndexItemIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIndexAliasIndexItemIndexItemId)), 0);
        set => SetValue(nameof(AzureSearchIndexAliasIndexItemIndexItemId), value);
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
    protected AzureSearchIndexAliasIndexItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AzureSearchIndexAliasIndexItemInfo"/> class.
    /// </summary>
    public AzureSearchIndexAliasIndexItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AzureSearchIndexAliasIndexItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AzureSearchIndexAliasIndexItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
