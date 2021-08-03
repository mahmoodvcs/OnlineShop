using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.ActionFilter
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AdminLogActionFilter : ActionFilterAttribute
    {
    }
}
