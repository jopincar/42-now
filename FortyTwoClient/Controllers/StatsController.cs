using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortyTwoClient.Models;

namespace FortyTwoClient.Controllers
{
    public class StatsController : BaseController
    {
        //
        // GET: /Stats/

        public ActionResult Index()
        {
            return View("TopPlayers", new StatsModel());
        }

    }
}
