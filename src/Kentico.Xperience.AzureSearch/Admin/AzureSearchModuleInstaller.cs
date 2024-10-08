﻿using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace Kentico.Xperience.AzureSearch.Admin;

internal class AzureSearchModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> resourceProvider;

    public AzureSearchModuleInstaller(IInfoProvider<ResourceInfo> resourceProvider) => this.resourceProvider = resourceProvider;

    public void Install()
    {
        var resource = resourceProvider.Get("CMS.Integration.AzureSearch")
            ?? new ResourceInfo();

        InitializeResource(resource);
        InstallAzureSearchItemInfo(resource);
        InstallAzureSearchIndexAliasItemInfo(resource);
        InstallAzureSearchIndexAliasIndexItemInfo(resource);
        InstallAzureSearchLanguageInfo(resource);
        InstallAzureSearchIndexPathItemInfo(resource);
        InstallAzureSearchContentTypeItemInfo(resource);
        InstallAzureSearchReusableContentTypeItemInfo(resource);
    }

    public ResourceInfo InitializeResource(ResourceInfo resource)
    {
        resource.ResourceDisplayName = "Kentico Integration - AzureSearch";

        // Prefix ResourceName with "CMS" to prevent C# class generation
        // Classes are already available through the library itself
        resource.ResourceName = "CMS.Integration.AzureSearch";
        resource.ResourceDescription = "Kentico AzureSearch custom data";
        resource.ResourceIsInDevelopment = false;
        if (resource.HasChanged)
        {
            resourceProvider.Set(resource);
        }

        return resource;
    }

    public void InstallAzureSearchItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AzureSearchIndexItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AzureSearchIndexItemInfo.OBJECT_TYPE);

        info.ClassName = AzureSearchIndexItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AzureSearchIndexItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "AzureSearch Index Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemIndexName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemChannelName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemStrategyName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexItemInfo.AzureSearchIndexItemRebuildHook),
            AllowEmpty = true,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAzureSearchIndexAliasItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AzureSearchIndexAliasItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AzureSearchIndexAliasItemInfo.OBJECT_TYPE);

        info.ClassName = AzureSearchIndexAliasItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AzureSearchIndexAliasItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "AzureSearch Index Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexAliasItemInfo.AzureSearchIndexAliasItemIndexAliasName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAzureSearchIndexAliasIndexItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AzureSearchIndexAliasIndexItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AzureSearchIndexAliasIndexItemInfo.OBJECT_TYPE);

        info.ClassName = AzureSearchIndexAliasIndexItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AzureSearchIndexAliasIndexItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "AzureSearch Index Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexAliasId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "integer",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexAliasIndexItemInfo.AzureSearchIndexAliasIndexItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "integer",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAzureSearchIndexPathItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AzureSearchIncludedPathItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AzureSearchIncludedPathItemInfo.OBJECT_TYPE);

        info.ClassName = AzureSearchIncludedPathItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AzureSearchIncludedPathItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "AzureSearch Path Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemAliasPath),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIncludedPathItemInfo.AzureSearchIncludedPathItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AzureSearchIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAzureSearchLanguageInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AzureSearchIndexLanguageItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AzureSearchIndexLanguageItemInfo.OBJECT_TYPE);

        info.ClassName = AzureSearchIndexLanguageItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AzureSearchIndexLanguageItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "AzureSearch Indexed Language Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchIndexLanguageItemInfo.AzureSearchIndexLanguageItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AzureSearchIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAzureSearchContentTypeItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AzureSearchContentTypeItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AzureSearchContentTypeItemInfo.OBJECT_TYPE);

        info.ClassName = AzureSearchContentTypeItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AzureSearchContentTypeItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "AzureSearch Type Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemContentTypeName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
            IsUnique = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemIncludedPathItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AzureSearchIncludedPathItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemGuid),
            Enabled = true,
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchContentTypeItemInfo.AzureSearchContentTypeItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AzureSearchIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAzureSearchReusableContentTypeItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AzureSearchReusableContentTypeItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AzureSearchReusableContentTypeItemInfo.OBJECT_TYPE);

        info.ClassName = AzureSearchReusableContentTypeItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AzureSearchReusableContentTypeItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Azure Search Reusable Content Type Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemContentTypeName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
            IsUnique = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemGuid),
            Enabled = true,
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AzureSearchReusableContentTypeItemInfo.AzureSearchReusableContentTypeItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AzureSearchIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
