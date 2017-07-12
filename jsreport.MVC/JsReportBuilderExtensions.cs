using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jsreport.MVC
{
    public static class JsReportBuilderExtensions
    {
        public static IApplicationBuilder UseJsReport(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JsReportMiddleware>();
        }
    }

}
