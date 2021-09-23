using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;

namespace WebApp.Infrastructure.Localization
{
    public sealed class ExtendedViewLocalizer : IViewLocalizer, IViewContextAware
    {
        private readonly IHtmlLocalizerFactory _localizerFactory;
        private readonly string _applicationName;
        private IHtmlLocalizer? _localizer;

        public ExtendedViewLocalizer(IHtmlLocalizerFactory localizerFactory, IWebHostEnvironment hostingEnvironment)
        {
            _localizerFactory = localizerFactory ?? throw new ArgumentNullException(nameof(localizerFactory));
            _applicationName = (hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment))).ApplicationName;
        }

        public LocalizedHtmlString this[string key] => _localizer![key];

        public LocalizedHtmlString this[string key, params object[] arguments] => _localizer![key, arguments];

        public LocalizedString GetString(string name) => _localizer!.GetString(name);

        public LocalizedString GetString(string name, params object[] values) => _localizer!.GetString(name, values);

#if !NET5_0_OR_GREATER
        [Obsolete("This method is obsolete. Use `CurrentCulture` and `CurrentUICulture` instead.")]
        public IHtmlLocalizer WithCulture(CultureInfo culture) => _localizer!.WithCulture(culture);
#endif

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _localizer!.GetAllStrings(includeParentCultures);

        public void Contextualize(ViewContext viewContext)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            // Given a view path "/Views/Home/Index.cshtml" we want a baseName like "MyApplication.Views.Home.Index"
            var path = viewContext.ExecutingFilePath;

            if (string.IsNullOrEmpty(path))
            {
                path = viewContext.View.Path;
            }

            Debug.Assert(!string.IsNullOrEmpty(path), "Couldn't determine a path for the view");
            
            var location =
                (viewContext.View is RazorView razorView ? AssociatedAssemblyNameAttribute.GetCachedFor(razorView.RazorPage.GetType())?.AssemblyName.Name : null) ??
                _applicationName;

            _localizer = _localizerFactory.Create(BuildBaseName(path, location), location);
        }

        private static string BuildBaseName(string path, string location)
        {
            var extension = Path.GetExtension(path);
            var startIndex = path[0] == '/' || path[0] == '\\' ? 1 : 0;
            var length = path.Length - startIndex - extension.Length;
            var capacity = length + location.Length + 1;
            var builder = new StringBuilder(path, startIndex, length, capacity);

            builder.Replace('/', '.').Replace('\\', '.');

            // Prepend the application name
            builder.Insert(0, '.');
            builder.Insert(0, location);

            return builder.ToString();
        }
    }
}
