# Karambolo.PO

This class library enables parsing, building and generating GetText PO files on the .NET platform. (Target frameworks: .NET Framework 4 & 4.5, .NET Standard 1.0 & 2.0).

[![NuGet Release](https://img.shields.io/nuget/v/Karambolo.PO.svg)](https://www.nuget.org/packages/Karambolo.PO/)

The implementation is based on the PO file format specification of the [GNU gettext utilities documentation](https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html). All the parts relevant to .NET programming is covered including full support for

 - contexts for identical text disambiguation,
 - plural forms (including plural form selector expressions),
 - metadata comments,
 - proper formatting of long texts.

Where the documentation was not specific enough, compatibility with Poedit took precedence.

Only synchronous API is available, async I/O is not supported for the moment.

### Code samples

#### Parsing PO content
```
var parser = new POParser(new POParserSettings
{
    // parser options...
});

TextReader reader = ...;
var result = parser.Parse(reader);

if (result.Success)
{
    var catalog = result.Catalog;
    // process the parsed data...
}
else
{
    var diagnostics = result.Diagnostics;
    // examine diagnostics, display an error, etc...
}
```

##### Remarks:

 - As of version 1.1.0, convenience overloads accepting *String* or *Stream* are available for *POParser.Parse* method, as well.
 - The parser **instance can be re-used** but it's **not safe to use it concurrently** from multiple threads.

##### Options:

 - **ReadHeaderOnly**: parse only the metadata header item.
 - **SkipInfoHeaders**: parse only the relevant metadata headers (*Content-Transfer-Encoding*, *Content-Type*, *Language* and *Plural-Forms*) and ignore the rest.
 - **SkipComments**: parse no comments at all, not even the ones containing metadata.

#### Generating PO file content
```
POCatalog catalog = ...;

var generator = new POGenerator(new POGeneratorSettings {
{
    // generator options...
});

TextWriter writer = ...;
generator.Generate(writer, catalog);
```

##### Remarks:

 - As of version 1.1.0, convenience overloads accepting *StringBuilder* or *Stream* are available for *POGenerator.Generate* method, as well.
 - The generator **instance can be re-used** but it's **not safe to use it concurrently** from multiple threads.

##### Options:

 - **IgnoreEncoding**: don't check whether the text encoding of the writer and the text encoding set for the catalog match.
 - **IgnoreLineBreaks**: don't respect line breaks ("\n") when wrapping texts.
 - **IgnoreLongLines**: don't wrap long lines (lines longer than 80 characters).
 - **SkipInfoHeaders**: generate only the relevant metadata headers (*Content-Transfer-Encoding*, *Content-Type*, *Language* and *Plural-Forms*) and ignore the rest.
 - **SkipComments**: generate no comments

#### Building PO catalogs by code
```
var catalog = new POCatalog();

// setting comments for the header item
catalog.HeaderComments = new POComment[]
{
    new POTranslatorComment { Text = "Some header comment" }
};

// setting required headers
catalog.Encoding = "UTF-8";
catalog.PluralFormCount = 2;
catalog.PluralFormSelector = "(n != 1)";
catalog.Language = "en_US";

// setting custom headers
catalog.Headers = new Dictionary<string, string>
{
    { "POT-Creation-Date", "2018-08-01 12:34+0000" },
    { "Project-Id-Version", "Some Awesome App 1.0" },
    { "X-Generator", "My Awesome PO Generator Tool 1.0" },
};

// adding a plural entry with text context and all kinds of comments
var key = new POKey("{0} user", "{0} users", "/Views/Home/Index");
IPOEntry entry = new POPluralEntry(key)
{
    "Translation of {0} user",
    "Translation of {0} users",
};
entry.Comments = new POComment[]
{
    new POTranslatorComment { Text = "Some translator comment" },
    new POExtractedComment { Text = "Some extracted comment" },
    new POReferenceComment { References = new POSourceReference[] { new POSourceReference("/Views/Home/Index.cshtml", 8) } },
    new POFlagsComment { Flags = new HashSet<string> { "fuzzy" } },
    new POPreviousValueComment { IdKind = POIdKind.Id, Value = "{0} user logged in." },
    new POPreviousValueComment { IdKind = POIdKind.PluralId, Value = "{0} users logged in." },
};
catalog.Add(entry);

// adding a singular entry with multi-line text
key = new POKey($"Multi-line{Environment.NewLine}text.");
entry = new POSingularEntry(key)
{
    Translation = $"Translation of multi-line{Environment.NewLine}text."
};
catalog.Add(entry);
```

#### Retrieving translations from PO catalogs
```
// querying translation
var key = new POKey($"Multi-line{Environment.NewLine}text.");
var translation = catalog.GetTranslation(key);

// querying translation for the count of 5
key = new POKey("{0} user", "{0} users", "/Views/Home/Index");
translation.GetTranslation(key, 5);
```

##### Remarks:

 -  *POCatalog.GetTranslation(POKey key)* returns the default translation: *msgstr* for a singular entry and *msgstr[0]* for a plural entry, respectively.
 -  *POCatalog.GetTranslation(POKey key, int count)* returns the plural form translation corresponding the *count* argument. In the case of singular entries *count* is ignored and *msgstr* is returned always. In the case of plural entries, *count* is mapped to the corresponding index using the *POCatalog.PluralFormSelector* expression and *msgstr[index]* is returned. (The index is adjusted to the actual count of translations so no exception is thrown even if the index were out of bounds.)

### Real-world application

Localizing a .NET application is not as convenient as it could be. All that hassle with XML resources and satellite assemblies can be cumbersome sometimes.

PO, besides providing some additional features over .NET resources, is an excellent alternative as there are great and mature tools around which can aid and speed up the process of translating text resources and keeping them in sync.

However,  there are some pieces missing to use PO when developing .NET applications. This library was created with an intention to have a base on which these pieces can be built.

In my [sample ASP.NET project](https://github.com/adams85/aspnetskeleton) I show how to complete the puzzle to get a complete and efficient localization solution based on PO files. (The idea is not tied to ASP.NET at all, it can be applied to other types of .NET projects easily.)

The toolset consists of the following components:
 - **A localization API** which provides the translations for the application.
   -  In .NET Core this infrastructure is available out-of-the-box. The key interfaces are *IStringLocalizer*, *IHtmlLocalizer*, *IViewLocalizer*. Due to the modular design of .NET Core it's easy to provide a custom implementation which uses PO files as its source. A sample implementation can be found [here](https://github.com/adams85/aspnetskeleton/tree/NetCore/source/Web/UI/Infrastructure/Localization).
   - .NET Framework doesn't have such an API but it's not difficult to define something similar to what .NET Core provides. My approach to this is [available](https://github.com/adams85/aspnetskeleton/tree/NetCore/source/Web/UI/Infrastructure/Localization) in my utility library. The key interface here is *ITextLocalizer*. A sample implementation is [available](https://github.com/adams85/aspnetskeleton/tree/NetFramework/source/Web/UI/Infrastructure/Localization), as well.
 - **An extractor tool** which scans the source files (Razor views, code-behind files, etc.) of the application and extracts the texts to translate. With [Roslyn](https://github.com/dotnet/roslyn) on board implementing such a tool is not a tough job again **if** a naming convention is used consistently in the application source code. E.g. I always access the localizer object through a property named *T* (sample [here](https://github.com/adams85/aspnetskeleton/blob/NetCore/source/Web/UI/Controllers/AccountController.cs#L281)) what provides a terse syntax and what's more important, makes text extracting possible.
My sample ASP.NET project also includes a ready-to-use implementation of a tool of this kind: .NET Core version is available [here](https://github.com/adams85/aspnetskeleton/tree/NetCore/source/Tools/POTools) and .NET Framework version [here](https://github.com/adams85/aspnetskeleton/tree/NetFramework/source/Tools/POTools). (This is a command-line tool which can be built independently of the web application using the *Tools.sln* solution.) Its usage as simple as follows:
   ```
     dotnet potools.dll scan | dotnet potools.dll extract /o=project.pot
   ```
   (On .NET Framework use `potools.exe` instead of `dotnet potools.dll`.)
   It's important to change the working directory to your project's directory before you issue the command to get correct source reference paths. Alternatively, you can use the */p* optional arguments to set a base path other then the current directory.
 - **An editor tool** which enables editing the extracted PO templates. As the PO file format is easy for humans to read, it's even possible to use a simple text editor. However, there are much more productive tools. I recommend [Poedit](https://poedit.net/), which is available on multiple platforms, moreover, it has some essential features like merging different versions of a PO file.

### If you have questions or suggestions
Feel free to contact me at [Gitter](https://gitter.im/Karambolo-PO/Lobby?utm_source=share-link&utm_medium=link&utm_campaign=share-link).
