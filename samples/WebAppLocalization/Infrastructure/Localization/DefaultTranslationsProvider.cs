using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Karambolo.PO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace WebApp.Infrastructure.Localization;

public class DefaultTranslationsProvider : ITranslationsProvider
{
    private const string FileNamePattern = "*.po";

    private static readonly POParserSettings s_parserSettings = new POParserSettings
    {
        SkipComments = true,
        SkipInfoHeaders = true,
        StringDecodingOptions = new POStringDecodingOptions { KeepKeyStringsPlatformIndependent = true }
    };

    private readonly ILogger _logger;

    private readonly string _translationsBasePath;
    private readonly IReadOnlyDictionary<(string Location, string Culture), POCatalog> _catalogs;

    public DefaultTranslationsProvider(ILogger<DefaultTranslationsProvider> logger)
    {
        _logger = logger ?? (ILogger)NullLogger.Instance;

        _translationsBasePath = Path.Combine(AppContext.BaseDirectory, "Translations");
        _catalogs = LoadFiles();
    }

    private Dictionary<(string Location, string Culture), POCatalog> LoadFiles()
    {
        return Directory.GetFiles(_translationsBasePath, FileNamePattern, SearchOption.AllDirectories)
            .Select(LoadFile)
            .Where(item => item != null)
            .ToDictionary(item => (item!.Value.Location, item.Value.Culture), item => item!.Value.Catalog);
    }

    private (POCatalog Catalog, string Location, string Culture)? LoadFile(string filePath)
    {
        var relativeFilePath = Path.GetRelativePath(_translationsBasePath, filePath);

        var culture = Path.GetDirectoryName(relativeFilePath);
        if (string.IsNullOrEmpty(culture) || !string.IsNullOrEmpty(Path.GetDirectoryName(culture)))
            return null;

        var location = Path.GetFileNameWithoutExtension(relativeFilePath);

        var catalog = LoadTranslations(filePath);
        if (catalog == null)
            return null;

        return (catalog, location, culture);
    }

    private POCatalog? LoadTranslations(string filePath)
    {
        FileStream fileStream;
        try { fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read); }
        catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException) 
        {
            _logger.LogWarning(ex, "Translation file \"{PATH}\" cannot be accessed.", filePath);
            return null; 
        }

        POParseResult parseResult;
        using (fileStream)
            parseResult = new POParser(s_parserSettings).Parse(fileStream);
        
        if (!parseResult.Success)
        {
            var diagnosticMessages = parseResult.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

            _logger.LogWarning("Translation file \"{PATH}\" has errors: {ERRORS}", filePath, string.Join(Environment.NewLine, diagnosticMessages));
            return null;
        }

        return parseResult.Catalog;
    }

    public IReadOnlyDictionary<(string Location, string Culture), POCatalog> GetCatalogs() => _catalogs;
}
