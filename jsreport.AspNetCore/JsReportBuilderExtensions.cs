using Microsoft.AspNetCore.Builder;

namespace jsreport.AspNetCore
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
