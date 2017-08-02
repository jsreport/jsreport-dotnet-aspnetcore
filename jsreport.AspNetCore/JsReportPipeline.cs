using Microsoft.AspNetCore.Builder;

namespace jsreport.AspNetCore
{
    public class JsReportPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseJsReport();
        }
    }
}
