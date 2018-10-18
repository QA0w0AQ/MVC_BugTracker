using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_BugTraker.Helper
{
    public static class HtmlHelpersExtensions
    {
        public static IHtmlString DisplayDate(this HtmlHelper helper, DateTimeOffset? date)
        {
            var returnData = "";

            if (!date.HasValue)
            {
                returnData = "-";
            }
            else
            {
                returnData = date.Value.ToString("yyyy-MM-dd HH:mm");
            }
            return new HtmlString(returnData);
        }
    }
}