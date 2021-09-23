using System;
using System.Globalization;
using System.Linq;
using Karambolo.Common.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IStringLocalizer<IndexModel> stringLocalizer, ILogger<IndexModel> logger)
        {
            // name the string localizer property "T" (or field "t"/"_t") to enable automatic text extraction by POTools
            T = stringLocalizer;
            _logger = logger;
        }

        protected IStringLocalizer<IndexModel> T { get; }

        public LocalizedString CurrentDate { get; set; } = null!;
        public LocalizedString CurrentTime { get; set; } = null!;
        public LocalizedString DaysUntilNextWeekend { get; set; } = null!;

        public void OnGet()
        {
            var utcNow = DateTime.UtcNow;

            CurrentDate = T["Today is {0:D}.", utcNow];

            CurrentTime = T["The time is {0:T} (GMT).", utcNow];

            var daysUntilNextWeekend = DayOfWeek.Saturday - utcNow.DayOfWeek;
            if (daysUntilNextWeekend <= 0)
                daysUntilNextWeekend += 7;

            DaysUntilNextWeekend = T["{0} day until next weekend.", Plural.From("{0} days until next weekend.", daysUntilNextWeekend)];
        }

        public IActionResult OnPostChangeLanguage(string culture, string returnUrl, [FromServices] IOptions<AppLocalizationOptions> options)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return BadRequest();

            if (options.Value.SupportedCultureInfos.Contains(new CultureInfo(culture)))
            {
                Response.Cookies.Append(
                   CookieRequestCultureProvider.DefaultCookieName,
                   CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                   // making cookie valid for the actual app root path (which is not necessarily "/" e.g. if we're behind a reverse proxy)
                   new CookieOptions { Path = Url.Content("~/"), Expires = DateTimeOffset.UtcNow.AddYears(1) });
            }

            return Redirect(returnUrl);
        }
    }
}
