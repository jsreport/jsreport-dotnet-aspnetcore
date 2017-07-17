using Microsoft.AspNetCore.Builder;

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
