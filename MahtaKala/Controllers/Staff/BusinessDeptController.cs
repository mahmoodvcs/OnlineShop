using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers.Staff
{
	public class BusinessDeptController : SiteControllerBase<BusinessDeptController>
	{
		public BusinessDeptController(
			DataContext context, 
			ILogger<BusinessDeptController> logger) : base(context, logger)
		{ }

		public IActionResult Index()
		{
			return View();
		}
	}
}
