using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Security
{
	public class UsernameAuthorizationHandler : AuthorizationHandler<UsernameAuthorizationRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UsernameAuthorizationRequirement requirement)
		{
			var user = context.User;
			if (requirement.UserNames.Contains(user.Identity.Name.ToLower()))
			{
				context.Succeed(requirement);
			}
			return Task.CompletedTask;
		}
	}
}
