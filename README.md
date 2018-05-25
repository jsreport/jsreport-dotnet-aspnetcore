# jsreport.AspNetCore
[![Build status](https://ci.appveyor.com/api/projects/status/4vyvsocrvn3en7os?svg=true)](https://ci.appveyor.com/project/pofider/jsreport-dotnet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/jsreport.AspNetCore.svg)](https://nuget.org/packages/jsreport.AspNetCore)

This package includes the asp.net core middleware and mvc filters enabling usage of Razor views for rendering reports.

You can find examples and documentation in the [jsreport sdk for .net home page](https://jsreport.net/learn/dotnet).
The general development and contribution instructions can be find in the [jsreport for .net  root repository](https://github.com/jsreport/jsreport-dotnet).

### Quick starters
#### The middleware
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

3. Then you need to add MiddlewareFilter attribute to the particular action and specify which conversion you want to use. In this case html to pdf conversion.

```c#
[MiddlewareFilter(typeof(JsReportPipeline))]
public IActionResult Invoice()
{
    HttpContext.JsReportFeature().Recipe(Recipe.PhantomPdf);
    return View();
}
```
You can reach bunch of other options for headers, footers or page layout on JsReportFeature(). Note that the same way you can also produce excel files from html. See more information in the [documentation](https://jsreport.net/learn/dotnet-aspnetcore).

#### Generate a file

In some cases, you may want more flexible way of handling the generated PDF data, such as sending the PDF file by email. 
In order to generate dynamic PDF files as byte arrays, you may use the following.

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

You need to render your view as a HTML string, you may use the following (which can be injected as a scoped service):


```c#
public class ViewToStringRendererService
{
    private IRazorViewEngine _viewEngine;
    private ITempDataProvider _tempDataProvider;
    private IServiceProvider _serviceProvider;

    public ViewToStringRendererService(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderViewToStringAsync<TModel>(string name, TModel model)
    {
        var actionContext = GetActionContext();
        var viewEngineResult = _viewEngine.FindView(actionContext, name, false);
        if (!viewEngineResult.Success)
        {
            throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
        }

        var view = viewEngineResult.View;
        using (var output = new StringWriter())
        {
            var viewContext = new ViewContext(
                actionContext,
                view,
                new ViewDataDictionary<TModel>(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: new ModelStateDictionary())
                {
                    Model = model
                },
                new TempDataDictionary(
                    actionContext.HttpContext,
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
}
```

3. Call the PDF generator, save/store the file and/or send it

In your controller, supposing the razor cshtml view template to be /Views/Home/PDFTemplate.cshtml you may use the following code.
Note the relative path and no .cshtml in the view name). 
This is a not-complete sample as it requires a view, the associated viewmodel and some obvious variables:

```c#
var htmlContent = await _ViewToStringRendererService.RenderViewToStringAsync("Home/PDFTemplate", viewModel);
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
