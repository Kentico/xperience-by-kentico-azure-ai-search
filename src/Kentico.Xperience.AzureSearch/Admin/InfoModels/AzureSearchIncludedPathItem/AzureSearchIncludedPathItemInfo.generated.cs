using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterObjectType(typeof(AzureSearchIncludedPathItemInfo), AzureSearchIncludedPathItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Data container class for <see cref="AzureSearchIncludedPathItemInfo"/>.
/// </summary>
[Serializable]
public partial class AzureSearchIncludedPathItemInfo : AbstractInfo<AzureSearchIncludedPathItemInfo, IAzureSearchIncludedPathItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoazuresearch.azuresearchincludedpathitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(AzureSearchIncludedPathItemInfoProvider), OBJECT_TYPE, "KenticoAzureSearch.AzureSearchIncludedPathItem", nameof(AzureSearchIncludedPathItemId), null, nameof(AzureSearchIncludedPathItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(AzureSearchIncludedPathItemIndexItemId), AzureSearchIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// AzureSearch included path item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIncludedPathItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIncludedPathItemId)), 0);
        set => SetValue(nameof(AzureSearchIncludedPathItemId), value);
    }

    /// <summary>
    /// AzureSearch included path item guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AzureSearchIncludedPathItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AzureSearchIncludedPathItemGuid)), default);
        set => SetValue(nameof(AzureSearchIncludedPathItemGuid), value);
    }


    /// <summary>
    /// Alias path.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchIncludedPathItemAliasPath
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchIncludedPathItemAliasPath)), String.Empty);
        set => SetValue(nameof(AzureSearchIncludedPathItemAliasPath), value);
    }


    /// <summary>
    /// AzureSearch index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIncludedPathItemIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIncludedPathItemIndexItemId)), 0);
        set => SetValue(nameof(AzureSearchIncludedPathItemIndexItemId), value);
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
    protected AzureSearchIncludedPathItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AzureSearchIncludedPathItemInfo"/> class.
    /// </summary>
    public AzureSearchIncludedPathItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AzureSearchIncludedPathItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AzureSearchIncludedPathItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
