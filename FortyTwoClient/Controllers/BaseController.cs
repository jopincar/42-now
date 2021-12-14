using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Data.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FortyTwoLib;
using FortyTwoLib.Logging;

namespace FortyTwoClient.Controllers
{
	public class BaseController : Controller
	{
		private Log _log;
		public Log Log 
		{ 
			get
			{
				if ( _log == null )
				{
					_log = new Log(new FileLogSink());
				}
				return _log;
			}
		}

		protected string UserName
		{
			get
			{
				SetUserSessionData();
				return (string) Session["UserName"];
			}
		}

		protected int UserId
		{
			get
			{
				SetUserSessionData();
				return (int) Session["UserId"];
			}
		}

		protected void ClearUserSessionData()
		{
			Session["UserId"] = null;
			Session["UserName"] = null;
		}

		protected void SetUserSessionData()
		{
			if (Session["UserId"] == null)
			{
				var user = Membership.GetUser();
				if ( user == null )
				{
					Session["UserId"] = (9999999 + new Random().Next(9999999)) * -1;
					Session["UserName"] = "You";
					Log.WriteInfo(string.Format("Created session for unregistered user: {0}", Session["UserId"]));
				} else
				{
					Session["UserId"] = (int) user.ProviderUserKey;
					Session["UserName"] = user.UserName;
					Log.WriteInfo(string.Format("Created session for registered user: {0}", Session["UserId"]));
				}
			}
		}

		protected override void OnException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);
			string routeData = "";
			foreach ( var x in filterContext.RouteData.Values )
			{
				routeData += string.Format("{0}: {1}; ", x.Key, x.Value);
			}
			Log.WriteError(string.Format("Unhandled exception - PlayerId: {0}, ViewData: {1}", UserId, routeData), filterContext.Exception);
		}
	}
}