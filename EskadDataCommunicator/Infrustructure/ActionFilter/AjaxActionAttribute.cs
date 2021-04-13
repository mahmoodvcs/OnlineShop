using MahtaKala.Infrustructure.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.ActionFilter
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AjaxActionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if(context.Exception != null && !(context.Exception is ApiException))
            {
                context.Exception = new ApiException(500, context.Exception);
            }
            base.OnActionExecuted(context);
        }
    }
}
