using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterObjectType(typeof(AzureSearchIndexLanguageItemInfo), AzureSearchIndexLanguageItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.AzureSearch.Admin;

/// <summary>
/// Data container class for <see cref="AzureSearchIndexLanguageItemInfo"/>.
/// </summary>
[Serializable]
public partial class AzureSearchIndexLanguageItemInfo : AbstractInfo<AzureSearchIndexLanguageItemInfo, IInfoProvider<AzureSearchIndexLanguageItemInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoazuresearch.azuresearchindexlanguageitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<AzureSearchIndexLanguageItemInfo>), OBJECT_TYPE, "KenticoAzureSearch.AzureSearchIndexLanguageItem", nameof(AzureSearchIndexLanguageItemID), null, nameof(AzureSearchIndexLanguageItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(AzureSearchIndexLanguageItemIndexItemId), AzureSearchIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// Indexed language id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIndexLanguageItemID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIndexLanguageItemID)), 0);
        set => SetValue(nameof(AzureSearchIndexLanguageItemID), value);
    }


    /// <summary>
    /// Indexed language id.
    /// </summary>
    [DatabaseField]
    public virtual Guid AzureSearchIndexLanguageItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AzureSearchIndexLanguageItemGuid)), default);
        set => SetValue(nameof(AzureSearchIndexLanguageItemGuid), value);
    }


    /// <summary>
    /// Code.
    /// </summary>
    [DatabaseField]
    public virtual string AzureSearchIndexLanguageItemName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AzureSearchIndexLanguageItemName)), String.Empty);
        set => SetValue(nameof(AzureSearchIndexLanguageItemName), value);
    }


    /// <summary>
    /// AzureSearch index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AzureSearchIndexLanguageItemIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AzureSearchIndexLanguageItemIndexItemId)), 0);
        set => SetValue(nameof(AzureSearchIndexLanguageItemIndexItemId), value);
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
    /// Creates an empty instance of the <see cref="AzureSearchIndexLanguageItemInfo"/> class.
    /// </summary>
    public AzureSearchIndexLanguageItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AzureSearchIndexLanguageItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AzureSearchIndexLanguageItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
