using MahtaKala.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{
	public class EskaadOrderFinilizedTodayViewComponent : ViewComponent
	{
		private readonly EskaadService eskaadService;

		public EskaadOrderFinilizedTodayViewComponent(EskaadService eskaadService)
		{
			this.eskaadService = eskaadService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var ordersAreSealed = await eskaadService.TodaysOrdersAreSealed();
			return View(ordersAreSealed);
		}
	}
}
