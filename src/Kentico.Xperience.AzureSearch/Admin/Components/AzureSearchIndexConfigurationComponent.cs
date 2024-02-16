using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.AzureSearch.Admin;

[assembly: RegisterFormComponent(
    identifier: AzureSearchIndexConfigurationComponent.IDENTIFIER,
    componentType: typeof(AzureSearchIndexConfigurationComponent),
    name: "AzureSearch Search Index Configuration")]

namespace Kentico.Xperience.AzureSearch.Admin;

#pragma warning disable S2094 // intentionally empty class
public class AzureSearchIndexConfigurationComponentProperties : FormComponentProperties
{
}
#pragma warning restore

public class AzureSearchIndexConfigurationComponentClientProperties : FormComponentClientProperties<IEnumerable<AzureSearchIndexIncludedPath>>
{
    public IEnumerable<string>? PossibleItems { get; set; }
}

public sealed class AzureSearchIndexConfigurationComponentAttribute : FormComponentAttribute
{
}

[ComponentAttribute(typeof(AzureSearchIndexConfigurationComponentAttribute))]
public class AzureSearchIndexConfigurationComponent : FormComponent<AzureSearchIndexConfigurationComponentProperties, AzureSearchIndexConfigurationComponentClientProperties, IEnumerable<AzureSearchIndexIncludedPath>>
{
    public const string IDENTIFIER = "kentico.xperience-integrations-azuresearch.azuresearch-index-configuration";

    internal List<AzureSearchIndexIncludedPath>? Value { get; set; }

    public override string ClientComponentName => "@kentico/xperience-integrations-azuresearch/AzureSearchIndexConfiguration";

    public override IEnumerable<AzureSearchIndexIncludedPath> GetValue() => Value ?? new();
    public override void SetValue(IEnumerable<AzureSearchIndexIncludedPath> value) => Value = value.ToList();

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> DeletePath(string path)
    {
        var toRemove = Value?.Find(x => Equals(x.AliasPath == path, StringComparison.OrdinalIgnoreCase));
        if (toRemove != null)
        {
            Value?.Remove(toRemove);
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
        return Task.FromResult(ResponseFrom(new RowActionResult(false)));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> SavePath(AzureSearchIndexIncludedPath path)
    {
        var value = Value?.SingleOrDefault(x => Equals(x.AliasPath == path.AliasPath, StringComparison.OrdinalIgnoreCase));

        if (value is not null)
        {
            Value?.Remove(value);
        }

        Value?.Add(path);

        return Task.FromResult(ResponseFrom(new RowActionResult(false)));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> AddPath(string path)
    {
        if (Value?.Exists(x => x.AliasPath == path) ?? false)
        {
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
        else
        {
            Value?.Add(new AzureSearchIndexIncludedPath(path));
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
    }

    protected override async Task ConfigureClientProperties(AzureSearchIndexConfigurationComponentClientProperties properties)
    {
        var allWebsiteContentTypes = await DataClassInfoProvider
            .GetClasses()
            .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), "Website")
            .Columns(nameof(DataClassInfo.ClassName))
            .GetEnumerableTypedResultAsync();

        properties.Value = Value ?? new();
        properties.PossibleItems = allWebsiteContentTypes.Select(x => x.ClassName).ToList();

        await base.ConfigureClientProperties(properties);
    }
}
