using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using jsreport.Shared;
using jsreport.Types;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace jsreport.AspNetCore
{
	public class JsReportMVCService : IJsReportMVCService
	{
		private readonly IRenderService _renderService;

		public JsReportMVCService(IRenderService rs)
			=> _renderService = rs;

		public Task<Report> RenderAsync(RenderRequest request, CancellationToken ct = default)
			=> _renderService.RenderAsync(request, ct);

		public Task<Report> RenderAsync(string templateShortid, object data, CancellationToken ct = default)
			=> _renderService.RenderAsync(templateShortid, data, ct);

		public Task<Report> RenderAsync(string templateShortid, string jsonData, CancellationToken ct = default)
			=> _renderService.RenderAsync(templateShortid, jsonData, ct);

		public Task<Report> RenderAsync(object request, CancellationToken ct = default)
			=> _renderService.RenderAsync(request, ct);

		public Task<Report> RenderByNameAsync(string templateName, string jsonData, CancellationToken ct = default)
			=> _renderService.RenderByNameAsync(templateName, jsonData, ct);

		public Task<Report> RenderByNameAsync(string templateName, object data, CancellationToken ct = default)
			=> _renderService.RenderByNameAsync(templateName, data, ct);

		public async Task<Report> RenderViewAsync(HttpContext context, RenderRequest renderRequest, RouteData routeData, string viewName, object model)
		{
			renderRequest.Template.Content = await RenderViewToStringAsync(context, routeData, viewName, model);
			return await _renderService.RenderAsync(renderRequest);
		}

		public async Task<string> RenderViewToStringAsync(HttpContext context, RouteData routeData, string viewName, object model)
		{
			var actionContext = new ActionContext(context, routeData, new ActionDescriptor());

			using (var sw = new StringWriter())
			{
				if (!(context.RequestServices.GetService(typeof(IRazorViewEngine)) is IRazorViewEngine razorViewEngine))
				{
					throw new ArgumentNullException($"RazorViewEngine service not found. Add services.AddRazorPages() in ConfigureServices method.");
				}

				var viewResult = razorViewEngine.FindView(actionContext, viewName, false);

				if (viewResult.View == null)
				{
					var hostingEnv = context.RequestServices.GetService(typeof(IHostingEnvironment)) as IHostingEnvironment;
					viewResult = razorViewEngine.GetView(hostingEnv.WebRootPath, viewName, false);
				}

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
					new TempDataDictionary(actionContext.HttpContext, (ITempDataProvider)context.RequestServices.GetService(typeof(ITempDataProvider))),
					sw,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);

				return sw.ToString();
			}
		}
	}
}
