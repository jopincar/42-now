using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortyTwoClient.Models;

namespace FortyTwoClient.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View(new StatsModel());
		}

		public ActionResult About()
		{
			return View();
		}
	}
}
