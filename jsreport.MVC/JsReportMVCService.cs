using jsreport.Shared;
using jsreport.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace jsreport.MVC
{
    public class JsReportMVCService : IJsReportMVCService
    {
        private IRenderService _renderService;
        public JsReportMVCService(IRenderService rs)
        {
            _renderService = rs;
        }

        public Task<Report> RenderAsync(RenderRequest request, CancellationToken ct = default(CancellationToken))
        {
            return _renderService.RenderAsync(request);
        }

        public Task<Report> RenderAsync(string templateShortid, object data, CancellationToken ct = default(CancellationToken))
        {
            return _renderService.RenderAsync(templateShortid, data, ct);
        }

        public Task<Report> RenderAsync(string templateShortid, string jsonData, CancellationToken ct = default(CancellationToken))
        {
            return _renderService.RenderAsync(templateShortid, jsonData, ct);
        }

        public Task<Report> RenderAsync(object request, CancellationToken ct = default(CancellationToken))
        {
            return _renderService.RenderAsync(request, ct);
        }

        public Task<Report> RenderByNameAsync(string templateName, string jsonData, CancellationToken ct = default(CancellationToken))
        {
            return _renderService.RenderByNameAsync(templateName, jsonData, ct);
        }

        public Task<Report> RenderByNameAsync(string templateName, object data, CancellationToken ct = default(CancellationToken))
        {
            return _renderService.RenderByNameAsync(templateName, data, ct);
        }

        public async Task<Report> RenderViewAsync(HttpContext context, RouteData routeData, string viewName, object model, RenderRequest renderRequest)
        {
            var str = await RenderViewToStringAsync(context, routeData, viewName, model);
            renderRequest.Template.Content = str;
            return await _renderService.RenderAsync(renderRequest);
        }

        public async Task<string> RenderViewToStringAsync(HttpContext context, RouteData routeData, string viewName, object model)
        {           
            var actionContext = new ActionContext(context, routeData, new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = ((IRazorViewEngine)context.RequestServices.GetService(typeof(IRazorViewEngine))).FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };                

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, ((ITempDataProvider)context.RequestServices.GetService(typeof(ITempDataProvider)))),
                    sw,
                    new HtmlHelperOptions()
                );                               
                
                await viewResult.View.RenderAsync(viewContext);

                return sw.ToString();
            }
        }
    }
}
