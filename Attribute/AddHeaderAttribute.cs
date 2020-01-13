using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LMSystem.Resource;
namespace LMSystem.Attribute
{
    public class AddHeader : ActionFilterAttribute
    {
        //private readonly string _name;
        //private readonly string _value;

        //public AddHeader(string name, string value)
        //{
        //    _name = name;
        //    _value = value;
        //}

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add(
                "Url", context.RequestContext.HttpContext.Request.RawUrl);
            base.OnResultExecuting(context);
        }
    }
}