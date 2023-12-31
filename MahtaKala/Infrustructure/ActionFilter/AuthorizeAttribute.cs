﻿using MahtaKala.Entities;
using MahtaKala.Infrustructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ActionFilter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        public AuthorizeAttribute() { }
        public AuthorizeAttribute(params UserType[] userType)
        {
            UserTypes = userType;
        }

        public UserType[] UserTypes { get; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor ad
                && (ad.MethodInfo?.CustomAttributes.Any(a => a.AttributeType == typeof(AllowAnonymousAttribute)) ?? false))
            {
                return;
            }

            var user = (User)context.HttpContext.Items["User"];
            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }
            else if (UserTypes != null && !UserTypes.Contains(user.Type) && user.Type != Entities.UserType.Admin)
            {
                if(user.Type == UserType.Customer)
                {
                    context.Result = new RedirectToRouteResult(
                                   new RouteValueDictionary
                                   {
                                       { "action", "Login" },
                                       { "controller", "Account" }
                                   });
                    base.OnActionExecuting(context);
                    return;
                }
                throw new AccessDeniedException();
            }
            base.OnActionExecuting(context);
        }
    }
}
