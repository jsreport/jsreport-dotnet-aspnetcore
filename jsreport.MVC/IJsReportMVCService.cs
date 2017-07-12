using jsreport.Shared;
using jsreport.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace jsreport.MVC
{
    public interface IJsReportMVCService : IRenderService
    {
        Task<Report> RenderViewAsync(HttpContext context, RouteData routeData, string viewName, object model, RenderRequest renderRequest);
        Task<string> RenderViewToStringAsync(HttpContext context, RouteData routeData, string viewName, object model);
    }    
}
