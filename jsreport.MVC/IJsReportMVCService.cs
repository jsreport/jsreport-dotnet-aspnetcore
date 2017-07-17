using jsreport.Shared;
using jsreport.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace jsreport.MVC
{
    /// <summary>
    /// jsreport rendering service which additionally provides methods to render particular view.
    /// This is usefull for rendering pdf headers mainly
    /// </summary>
    public interface IJsReportMVCService : IRenderService
    {
        /// <summary>
        /// jsreport agnostic helper function to render particular view to string
        /// </summary>                
        Task<string> RenderViewToStringAsync(HttpContext context, RouteData routeData, string viewName, object model);

        /// <summary>
        /// Render particular view to string and then use jsreport to covert the result based on the specified RenderRequest
        /// </summary>        
        Task<Report> RenderViewAsync(HttpContext context, RenderRequest renderRequest, RouteData routeData, string viewName, object model);        
    }    
}
