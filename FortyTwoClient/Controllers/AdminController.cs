using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortyTwoClient.Models;
using FortyTwoLib;

namespace FortyTwoClient.Controllers
{
    public class AdminController : BaseController
    {
        //
        // GET: /Admin/

		[Authorize(Roles = "Admin")]
		public ActionResult Index()
        {
            return View(new AdminModel());
        }

		[Authorize(Roles = "Admin")]
		public ActionResult Reset()
		{
			Log.WriteInfo("Admin initiated reset.");
			FortyTwoLib.Server.GetInstance().Reset();
			MailBox.GetInstance().Reset();
			return View();
		}

    }
}
