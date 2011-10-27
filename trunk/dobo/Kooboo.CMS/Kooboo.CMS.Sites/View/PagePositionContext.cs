using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Extension;
using System.Web.Mvc;

namespace Kooboo.CMS.Sites.View
{
    public class PagePositionContext
    {
        public PagePosition PagePosition { get; internal set; }

        public ViewDataDictionary ViewData { get; internal set; }

        public IEnumerable<Parameter> Parameters
        {
            get
            {
                IEnumerable<Parameter> parameters = new Parameter[0];
                if (PagePosition is ViewPosition)
                {
                    parameters = ((ViewPosition)PagePosition).Parameters ?? new List<Parameter>();
                }
                return parameters;
            }
        }
        public Parameter this[string parameterName]
        {
            get
            {
                return Parameters.Where(it => it.Name.EqualsOrNullEmpty(parameterName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            }
        }
        public T GetParameterValue<T>(string parameterName)
        {
            var parameter = this[parameterName];
            if (parameter != null)
            {
                return (T)parameter.Value;
            }
            return default(T);
        }
    }
}
