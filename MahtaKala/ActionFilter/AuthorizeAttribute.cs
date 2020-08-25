using MahtaKala.Entities;
using MahtaKala.Infrustructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ActionFilter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public AuthorizeAttribute() { }
        public AuthorizeAttribute(UserType userType)
        {
            UserType = userType;
        }

        public UserType? UserType { get; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor ad
                && (ad.MethodInfo?.CustomAttributes.Any(a => a.AttributeType == typeof(AllowAnonymousAttribute)) ?? false))
            {
                return;
            }

            var user = (User)context.HttpContext.Items["User"];
            if (user == null)
            {
                // not logged in
                throw new UnauthorizedAccessException();
            }
            if (UserType != null && user.Type != UserType && user.Type != Entities.UserType.Admin)
            {
                throw new AccessDeniedException();
            }
        }
    }
}
