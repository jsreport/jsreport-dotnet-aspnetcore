using jsreport.Types;
using Microsoft.AspNetCore.Http;
using System;

namespace jsreport.AspNetCore
{
    public interface IJsReportFeature
    {
        RenderRequest RenderRequest { get; set; }
        bool Enabled { get; set; }
        IJsReportFeature Configure(Action<RenderRequest> req);
        HttpContext Context { get; set; }        
        IJsReportFeature DebugLogsToResponse();
        IJsReportFeature NoBaseTag();
        IJsReportFeature Engine(Engine engine);
        IJsReportFeature Recipe(Recipe recipe);        
        Action<Report> AfterRender { get; set; }
        IJsReportFeature OnAfterRender(Action<Report> action);
    }

    public class JsReportFeature : IJsReportFeature
    {
        public JsReportFeature(HttpContext context)
        {
            RenderRequest = new RenderRequest();
            RenderRequest.Template.Engine = Types.Engine.None;            
            Context = context;
            RenderRequest.Options.Base = $"{Context.Request.Scheme}://{Context.Request.Host}";
            Enabled = true;
        }


        public RenderRequest RenderRequest { get; set; }
        public bool Enabled { get; set; }
        public HttpContext Context { get; set; }        
        public Action<Report> AfterRender { get; set; }

        public IJsReportFeature OnAfterRender(Action<Report> action)
        {
            AfterRender = action;
            return this;
        }

        public IJsReportFeature Engine(Engine engine)
        {
            RenderRequest.Template.Engine = engine;
            return this;
        }

        public IJsReportFeature Recipe(Recipe recipe)
        {
            RenderRequest.Template.Recipe = recipe;
            return this;
        }

        public IJsReportFeature NoBaseTag()
        {
            RenderRequest.Options.Base = null;
            return this;
        }        

        public IJsReportFeature DebugLogsToResponse()
        {
            RenderRequest.Options.Debug.LogsToResponse = true;
            return this;
        }

        public IJsReportFeature Configure(Action<RenderRequest> req)
        {
            req.Invoke(RenderRequest);
            return this;
        }
    }
}
