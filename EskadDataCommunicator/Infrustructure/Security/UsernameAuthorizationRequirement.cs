using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Security
{
	public class UsernameAuthorizationRequirement : IAuthorizationRequirement
	{
		//public static readonly string[] EskaadAuthorizedUserNames = { "katouzian", "mosalli", "ali.d" };
		public string[] UserNames { get; set; }

		public UsernameAuthorizationRequirement(params string[] userNames)
		{
			UserNames = userNames.Select(x => x.ToLower()).ToArray();
		}
	}
}
