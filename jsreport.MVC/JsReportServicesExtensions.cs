using jsreport.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace jsreport.MVC
{
    public static class JsReportServicesExtensions
    {
        /// <summary>
        /// Add IRenderService and IJsReportMVCService to the DI services container
        /// </summary>   
        /// <example>
        /// public void ConfigureServices(IServiceCollection services)
        /// {        
        ///    services.AddJsReport(new LocalReporting().AsUtility().Create());
        /// }
        /// </example>
        public static IServiceCollection AddJsReport(this IServiceCollection services, IRenderService renderService)
        {
            return services
                .AddSingleton(renderService)
                .AddSingleton<IJsReportMVCService>(new JsReportMVCService(renderService));
        }
    }
}
