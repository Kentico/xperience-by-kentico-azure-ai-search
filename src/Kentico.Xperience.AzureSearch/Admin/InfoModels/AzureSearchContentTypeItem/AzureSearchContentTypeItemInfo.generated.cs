
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterObjectType(typeof(AzureSearchContentTypeItemInfo), AzureSearchContentTypeItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Data container class for <see cref="AzureSearchContentTypeItemInfo"/>.
/// </summary>
[Serializable]
public partial class AzureSearchContentTypeItemInfo : AbstractInfo<AzureSearchContentTypeItemInfo, IAzureSearchContentTypeItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoazuresearch.azuresearchcontenttypeitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(AzureSearchContentTypeItemInfoProvider), OBJECT_TYPE, "KenticoAzureSearch.AzureSearchContentTypeItem", nameof(AzureSearchContentTypeItemId), null, nameof(AzureSearchContentTypeItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(AzureSearchContentTypeItemIncludedPathItemId), AzureSearchIncludedPathItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            new(nameof(AzureSearchContentTypeItemIndexItemId), AzureSearchIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// AzureSearch content type item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchContentTypeItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchContentTypeItemId)), 0);
        set => SetValue(nameof(AzureSearchContentTypeItemId), value);
    }


    /// <summary>
    /// AzureSearch content type item guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AzureSearchContentTypeItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AzureSearchContentTypeItemGuid)), default);
        set => SetValue(nameof(AzureSearchContentTypeItemGuid), value);
    }


    /// <summary>
    /// Content type name.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchContentTypeItemContentTypeName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchContentTypeItemContentTypeName)), String.Empty);
        set => SetValue(nameof(AzureSearchContentTypeItemContentTypeName), value);
    }


    /// <summary>
    /// AzureSearch included path item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchContentTypeItemIncludedPathItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchContentTypeItemIncludedPathItemId)), 0);
        set => SetValue(nameof(AzureSearchContentTypeItemIncludedPathItemId), value);
    }


    /// <summary>
    /// AzureSearch index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchContentTypeItemIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchContentTypeItemIndexItemId)), 0);
        set => SetValue(nameof(AzureSearchContentTypeItemIndexItemId), value);
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
    protected AzureSearchContentTypeItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AzureSearchContentTypeItemInfo"/> class.
    /// </summary>
    public AzureSearchContentTypeItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AzureSearchContentTypeItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AzureSearchContentTypeItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
