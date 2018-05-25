# jsreport.AspNetCore
[![Build status](https://ci.appveyor.com/api/projects/status/4vyvsocrvn3en7os?svg=true)](https://ci.appveyor.com/project/pofider/jsreport-dotnet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/jsreport.AspNetCore.svg)](https://nuget.org/packages/jsreport.AspNetCore)

This package includes the asp.net core middleware and mvc filters enabling usage of Razor views for rendering reports.

You can find examples and documentation in the [jsreport sdk for .net home page](https://jsreport.net/learn/dotnet).
The general development and contribution instructions can be find in the [jsreport for .net  root repository](https://github.com/jsreport/jsreport-dotnet).

## Quick starters
### Set up the service
You can use jsreport .NET SDK if you are in .NET Core 2.0. This includes among other features filters to convert your existing razor views into pdf. 

1. Install nugets [jsreport.Binary](https://www.nuget.org/packages/jsreport.Binary/), [jsreport.Local](https://www.nuget.org/packages/jsreport.Local/) and [jsreport.AspNetCore](https://www.nuget.org/packages/jsreport.AspNetCore/)

2. In your Startup.cs configure it as the following

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();              
    services.AddJsReport(new LocalReporting()
        .UseBinary(JsReportBinary.GetBinary())
        .AsUtility()
        .Create());
}
```

### Use the service as a middleware

The easiest option is to use the embbeded middleware implementation. 
All you need to do is to add MiddlewareFilter attribute to the particular action and specify which conversion you want to use. 
In this case html to pdf conversion.

```c#
[MiddlewareFilter(typeof(JsReportPipeline))]
public IActionResult Invoice()
{
    HttpContext.JsReportFeature().Recipe(Recipe.PhantomPdf);
    return View();
}
```
You can reach bunch of other options for headers, footers or page layout on JsReportFeature(). Note that the same way you can also produce excel files from html. See more information in the [documentation](https://jsreport.net/learn/dotnet-aspnetcore).

### Use the service to generate a file

In some cases, you may need a more flexible way of handling the generated PDF data, such as sending the PDF file by email. 
In order to generate dynamic PDF files as MemoryStream or byte arrays, you may use the following.

1. Use the library to generate the PDF from a HTML string

```c#
/// Generate a PDF from a html string
async Task<(string ContentType, MemoryStream GeneratedFileStream)> GeneratePDFAsync(string htmlContent)
{
    IJsReportFeature feature = new JsReportFeature(HttpContext);
    feature.Recipe(Recipe.PhantomPdf);
    if (!feature.Enabled) return (null, null);
    feature.RenderRequest.Template.Content = htmlContent;
    var report = await _RenderService.RenderAsync(feature.RenderRequest);
    var contentType = report.Meta.ContentType;
    MemoryStream ms = new MemoryStream();
    report.Content.CopyTo(ms);
    return (contentType, ms);
}
```

2. Render a view to generate a HTML string

You need to render your view as a HTML string, you may use the [following service](https://gist.github.com/JeanCollas/22154325c6da339d5ac0060f91ea7d53) (which can be injected as a scoped service):


```c#
    public class ViewToStringRendererService: ViewExecutor
    {
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;

        public ViewToStringRendererService(
            IOptions<MvcViewOptions> viewOptions,
            IHttpResponseStreamWriterFactory writerFactory,
            ICompositeViewEngine viewEngine,
            ITempDataDictionaryFactory tempDataFactory,
            DiagnosticSource diagnosticSource,
            IModelMetadataProvider modelMetadataProvider,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
            : base(viewOptions, writerFactory, viewEngine, tempDataFactory, diagnosticSource, modelMetadataProvider)
        {
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var context = GetActionContext();

            if (context == null) throw new ArgumentNullException(nameof(context));

            var result = new ViewResult()
            {
                ViewData = new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                {
                    Model = model
                },
                TempData = new TempDataDictionary(
                        context.HttpContext,
                        _tempDataProvider),
                ViewName = viewName,
            };

            var viewEngineResult = FindView(context, result);
            viewEngineResult.EnsureSuccessful(originalLocations: null);

            var view = viewEngineResult.View;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    context,
                    view,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        context.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }
        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        /// <summary>
        /// Attempts to find the <see cref="IView"/> associated with <paramref name="viewResult"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/> associated with the current request.</param>
        /// <param name="viewResult">The <see cref="ViewResult"/>.</param>
        /// <returns>A <see cref="ViewEngineResult"/>.</returns>
        ViewEngineResult FindView(ActionContext actionContext, ViewResult viewResult)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (viewResult == null)
            {
                throw new ArgumentNullException(nameof(viewResult));
            }

            var viewEngine = viewResult.ViewEngine ?? ViewEngine;

            var viewName = viewResult.ViewName ?? GetActionName(actionContext);

            var result = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            var originalResult = result;
            if (!result.Success)
            {
                result = viewEngine.FindView(actionContext, viewName, isMainPage: true);
            }

            if (!result.Success)
            {
                if (originalResult.SearchedLocations.Any())
                {
                    if (result.SearchedLocations.Any())
                    {
                        // Return a new ViewEngineResult listing all searched locations.
                        var locations = new List<string>(originalResult.SearchedLocations);
                        locations.AddRange(result.SearchedLocations);
                        result = ViewEngineResult.NotFound(viewName, locations);
                    }
                    else
                    {
                        // GetView() searched locations but FindView() did not. Use first ViewEngineResult.
                        result = originalResult;
                    }
                }
            }

            if(!result.Success)
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", viewName));

            return result;
        }


        private const string ActionNameKey = "action";
        private static string GetActionName(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.RouteData.Values.TryGetValue(ActionNameKey, out var routeValue))
            {
                return null;
            }

            var actionDescriptor = context.ActionDescriptor;
            string normalizedValue = null;
            if (actionDescriptor.RouteValues.TryGetValue(ActionNameKey, out var value) &&
                !string.IsNullOrEmpty(value))
            {
                normalizedValue = value;
            }

            var stringRouteValue = routeValue?.ToString();
            if (string.Equals(normalizedValue, stringRouteValue, StringComparison.OrdinalIgnoreCase))
            {
                return normalizedValue;
            }

            return stringRouteValue;
        }

    }

```

3. Call the PDF generator, save/store the file and/or send it

In your controller, supposing the razor cshtml view template to be /Views/Home/PDFTemplate.cshtml you may use the following code. 
This is a not-complete sample as it requires a view, the associated viewmodel and some obvious variables:

```c#
var htmlContent = await _ViewToStringRendererService.RenderViewToStringAsync("/Views/Home/PDFTemplate.cshtml", viewModel);
(var contentType, var generatedFile) = await GeneratePDFAsync(htmlContent);
Response.Headers["Content-Disposition"] = $"attachment; filename=\"{System.Net.WebUtility.UrlEncode(fileName)}\"";

// You may save your file here
using (var fileStream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
{
   await generatedFile.CopyToAsync(fileStream);
}
// You may need this for re-use of the stream
generatedFile.Seek(0, SeekOrigin.Begin);

return File(generatedFile.ToArray(), "application/pdf", fileName);
```
