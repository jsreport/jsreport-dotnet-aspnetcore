using jsreport.Types;
using Microsoft.AspNetCore.Http;

namespace jsreport.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static IJsReportFeature JsReportFeature(this HttpContext context)
        {
            return context.Features.Get<IJsReportFeature>();
        }        

        public static Template JsReportTemplate(this HttpContext context)
        {
            return context.Features.Get<IJsReportFeature>().RenderRequest.Template;
        }

        public static RenderRequest JsReportRequest(this HttpContext context)
        {
            return context.Features.Get<IJsReportFeature>().RenderRequest;
        }
    }
}
