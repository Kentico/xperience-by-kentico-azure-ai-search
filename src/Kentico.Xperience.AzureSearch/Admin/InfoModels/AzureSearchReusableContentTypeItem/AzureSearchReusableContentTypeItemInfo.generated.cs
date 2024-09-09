using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using CMS;
using CMS.DataEngine;

using CMS.Helpers;

using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterObjectType(typeof(AzureSearchReusableContentTypeItemInfo), AzureSearchReusableContentTypeItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Data container class for <see cref="AzureSearchReusableContentTypeItemInfo"/>.
/// </summary>
[Serializable]
public class AzureSearchReusableContentTypeItemInfo : AbstractInfo<AzureSearchReusableContentTypeItemInfo, IInfoProvider<AzureSearchReusableContentTypeItemInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoazuresearch.azuresearchreusablecontenttypeitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<AzureSearchReusableContentTypeItemInfo>), OBJECT_TYPE, "KenticoAzureSearch.AzureSearchReusableContentTypeItem", nameof(AzureSearchReusableContentTypeItemId), null, nameof(AzureSearchReusableContentTypeItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(AzureSearchReusableContentTypeItemIndexItemId), AzureSearchIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// Lucene reusable content type item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchReusableContentTypeItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchReusableContentTypeItemId)), 0);
        set => SetValue(nameof(AzureSearchReusableContentTypeItemId), value);
    }


    /// <summary>
    /// Lucene reusable content type item guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AzureSearchReusableContentTypeItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AzureSearchReusableContentTypeItemGuid)), default);
        set => SetValue(nameof(AzureSearchReusableContentTypeItemGuid), value);
    }


    /// <summary>
    /// Reusable content type name.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchReusableContentTypeItemContentTypeName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchReusableContentTypeItemContentTypeName)), String.Empty);
        set => SetValue(nameof(AzureSearchReusableContentTypeItemContentTypeName), value);
    }


    /// <summary>
    /// Lucene index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchReusableContentTypeItemIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchReusableContentTypeItemIndexItemId)), 0);
        set => SetValue(nameof(AzureSearchReusableContentTypeItemIndexItemId), value);
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
    protected AzureSearchReusableContentTypeItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AzureSearchReusableContentTypeItemInfo"/> class.
    /// </summary>
    public AzureSearchReusableContentTypeItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AzureSearchReusableContentTypeItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AzureSearchReusableContentTypeItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}

