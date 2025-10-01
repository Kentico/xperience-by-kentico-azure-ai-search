# GitHub Copilot Instructions for Xperience by Kentico Azure AI Search

## Project Overview

This project is an integration library for Xperience by Kentico that enables Azure AI Search indexing capabilities. It provides a code-first approach to create and manage Azure Search indexes for Kentico content.

## Technology Stack

- **Backend**: .NET 8.0, ASP.NET Core
- **Frontend**: React, TypeScript, Webpack
- **Testing**: xUnit
- **Database**: SQL Server 2019+
- **CMS**: Xperience by Kentico (v28.2.0+)
- **Cloud**: Azure AI Search

## Code Style and Standards

### C# Code

- Follow the `.editorconfig` rules defined in the repository
- Use 4 spaces for indentation
- Use CRLF line endings for C# files
- Always run `dotnet format` before committing (VS Code task: `.NET: format (AzureSearch)`)
- Follow naming conventions consistent with Xperience by Kentico patterns
- Use XML documentation comments for public APIs
- Prefer dependency injection over static classes

### TypeScript/JavaScript Code

- Follow the ESLint configuration in `src/Kentico.Xperience.AzureSearch/Admin/Client/eslint.config.mjs`
- Use single quotes for strings (except JSX attributes which use double quotes)
- Use PascalCase for React components
- Use camelCase for variables and functions
- No console.log or debugger statements in production code
- Always define explicit types (no `any`)

## Project Structure

```
├── src/Kentico.Xperience.AzureSearch/       # Main library code
│   ├── Admin/Client/                         # React admin UI
│   ├── Indexing/                             # Indexing strategies and logic
│   └── Services/                             # Core services
├── examples/DancingGoat/                     # Sample application
├── tests/Kentico.Xperience.AzureSearch.Tests/ # Unit tests
└── docs/                                     # Documentation
```

## Development Workflow

### Branch Naming

- `feat/` - for new functionality
- `refactor/` - for restructuring existing features
- `fix/` - for bugfixes

### Commit Messages

- Preferably follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) convention
- Examples: `feat: add semantic ranking`, `fix: resolve indexing delay issue`

### Before Submitting PR

1. Run `dotnet format` to ensure code formatting
2. Run `dotnet build` to ensure no build errors
3. Run `dotnet test` to ensure all tests pass
4. For Admin UI changes:
   - Run `npm install` in `src/Kentico.Xperience.AzureSearch/Admin/Client/`
   - Run `npm run lint` to check for linting issues
5. Include screenshots or videos for UX/UI updates
6. Update documentation if adding new features

## Building and Testing

### Build Commands

```bash
# Restore dependencies (use locked mode in CI)
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release
```

### Admin UI Development

For local development of the Admin UI:

1. Add to User Secrets:
   ```json
   "CMSAdminClientModuleSettings": {
     "kentico-xperience-integrations-azuresearch": {
       "Mode": "Proxy",
       "Port": 3009
     }
   }
   ```

2. Start the dev server:
   ```bash
   cd src/Kentico.Xperience.AzureSearch/Admin/Client/
   npm install
   npm start
   ```

## Key Concepts

### Indexing Strategy

- Custom indexing strategies must inherit from `BaseAzureSearchIndexingStrategy<TSearchModel>`
- Strategies define how content pages/items are processed for indexing
- See `docs/Custom-index-strategy.md` for details

### Configuration

- Azure Search configuration goes in `appsettings.json` under `CMSAzureSearch`
- Required settings: `SearchServiceEndPoint`, `SearchServiceAdminApiKey`, `SearchServiceQueryApiKey`
- Optional: `IndexItemDelay` for throttling indexing operations

### Testing Approach

- Use xUnit for unit tests
- Mock Xperience by Kentico dependencies using appropriate interfaces
- Test files follow pattern: `*Tests.cs`
- Focus on testing indexing strategies and service logic

## Common Patterns

### Dependency Injection

```csharp
// Register services in Program.cs
services.AddKenticoAzureSearch(builder =>
{
    builder.RegisterStrategy<GlobalAzureSearchStrategy, GlobalSearchModel>("DefaultStrategy");
}, configuration);
```

### Error Handling

- Use exceptions for exceptional conditions
- Log errors appropriately
- Provide meaningful error messages for administrators

## Documentation

- Main README.md provides quick start guide
- Detailed documentation in `docs/` directory
- Update relevant docs when adding features:
  - `docs/Usage-Guide.md` - main user guide
  - `docs/Contributing-Setup.md` - contributor setup
  - Individual feature docs (e.g., `docs/Custom-index-strategy.md`)

## Integration with Xperience

- This is an **integration library** for Xperience by Kentico
- Must maintain compatibility with specified Xperience versions (see Library Version Matrix in README)
- Follow Xperience patterns for:
  - Admin UI modules
  - Content type handling
  - Database interactions through info providers

## CI/CD

- GitHub Actions workflow in `.github/workflows/ci.yml`
- Runs on push/PR to main branch
- Checks:
  1. `dotnet format` verification
  2. Build (Release configuration)
  3. Test execution
- Do not exclude examples from format check when creating new examples

## Support Policy

- This project has **Full support by 7-day bug-fix policy**
- See [SUPPORT.md](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support)
- Security issues: see [SECURITY.md](https://github.com/Kentico/.github/blob/main/SECURITY.md)

## When Generating Code

1. **Minimize changes**: Make the smallest possible modifications to achieve the goal
2. **Follow existing patterns**: Look at similar code in the repository
3. **Maintain consistency**: Match the style and structure of surrounding code
4. **Test thoroughly**: Ensure changes don't break existing functionality
5. **Document**: Add XML comments for public APIs, update docs for new features
6. **Consider compatibility**: Ensure changes work across supported Xperience versions

## Resources

- [Xperience by Kentico Documentation](https://docs.xperience.io/)
- [Azure AI Search Documentation](https://learn.microsoft.com/en-us/azure/search/)
- [Kentico Contributing Guidelines](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md)
- [Kentico Code of Conduct](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md)
