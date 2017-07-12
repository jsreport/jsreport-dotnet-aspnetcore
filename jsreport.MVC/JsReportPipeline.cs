using Microsoft.AspNetCore.Builder;

namespace jsreport.MVC
{
    public class JsReportPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseJsReport();
        }
    }
}
