using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Kooboo.CMS.Sites.Models;

namespace Kooboo.CMS.Sites.Extension.Module
{
    public class ModuleHttpContext : HttpContextWrapper
    {
        HttpRequestBase _httpRequest;
        HttpResponseBase _httpResponse;

        public ModuleHttpContext(HttpContext httpContext, HttpRequestBase httpRequest, HttpResponseBase httpResponse, ModulePosition modulePosition)
            : base(httpContext)
        {
            this._httpRequest = httpRequest;
            this._httpResponse = httpResponse;
            this.ModulePosition = modulePosition;
        }

        public ModulePosition ModulePosition { get; private set; }


        public override HttpRequestBase Request
        {
            get
            {
                return this._httpRequest;
            }
        }

        public override HttpResponseBase Response
        {
            get
            {
                return this._httpResponse;
            }
        }
    }
}
