using jsreport.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace jsreport.MVC
{
    public static class JsReportServicesExtensions
    {
        public static IServiceCollection AddJsReport(this IServiceCollection services, IRenderService renderService)
        {
            return services
                .AddSingleton<IRenderService>(renderService)
                .AddSingleton<IJsReportMVCService>(new JsReportMVCService(renderService));
        }
    }
}
