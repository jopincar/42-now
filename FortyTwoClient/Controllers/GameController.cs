using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FortyTwoClient.Models;
using FortyTwoLib;
using FortyTwoLib.Database;

namespace FortyTwoClient.Controllers
{
    public class GameController : BaseController
    {
		public GameController()
		{
		}

		protected override void OnException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);
			if ( UserId <= 0 ) return;
			var result = FortyTwoLib.Server.GetInstance().FindPlayer(UserId);
			if ( result.Table != null )
			{
				result.Table.OnException(filterContext.Exception);
			} else if ( result.Player != null )
			{
				result.Player.OnException(filterContext.Exception);
			}
		}

		private TableModel GetTable()
		{
			var server = FortyTwoLib.Server.GetInstance();
			var userId = UserId;
			var table = server.GetTableByPlayerId(userId, false);
			if ( table == null )
			{
				throw new InvalidOperationException(string.Format("Could not find table for user {0}.", userId));
			}
			return new TableModel(table, userId);
		}

		[NoCache]
		public ActionResult Index()
		{
			var server = FortyTwoLib.Server.GetInstance();
			var table = server.GetTableByPlayerId(UserId, false);
			if (table == null || table.FixedHand != null )
			{
				if (table != null) server.RemoveTable(table, true);
				server.CreateTestTableForPlayer(UserId, UserName, null);
			}
			var mtable = GetTable();
			if ( mtable == null ) throw new ArgumentNullException(string.Format("mtable is null for user {0}", UserId));
			return View("Table", mtable);
		}

		public ActionResult Hand()
		{
			var hand = new HandModel {PlayerId = UserId};
			return View(hand);
		}

		[HttpPost]
		public ActionResult Hand(HandModel model)
		{
			if ( ModelState.IsValid )
			{
				var error = model.Validate();
				new DataAccess().SaveHand(model.Hand, UserId);
				if ( error == null ) return RedirectToAction("PlayHand", new {hand = model.Hand.Replace(" ", "x")});
				ModelState.AddModelError("", error);
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		public ActionResult PlayHand(string hand)
		{
			hand = hand.Replace(" ", "x");
			var server = FortyTwoLib.Server.GetInstance();
			var table = server.GetTableByPlayerId(UserId, false);
			if (table != null) server.RemoveTable(table, true);
			server.CreateTestTableForPlayer(UserId, UserName, hand);
			return View("Table", GetTable());
		}

    	public FileContentResult GetDominoImage(string dots, int size, bool horizontal)
		{
			return File(new DominoImage().GetImage(dots, size, horizontal), "image/png");
		}

		[HttpPost]
		public ActionResult FlipDomino(string dots)
		{
			var table = GetTable();
			table.GetPlayerBySeat(0).Hand.Flip(dots);
			return null;
		}

		[HttpPost]
		public ActionResult MoveDomino(string dots, int x, int y)
		{
			Log.WriteVerbose(string.Format("{0} moved {1} to {2},{3}.", UserId, dots, x, y));
			var table = GetTable();
			try
			{
				table.GetPlayerBySeat(0).Hand.Move(dots, new Point(x, y));
			} catch(DominoNotFoundException)
			{
				// Intentionally eating this
			}
			return null;
		}

		[HttpPost]
		public ActionResult GetValidBidAmounts(bool isLow)
		{
			var table = GetTable();
			return Json(table.ValidBidAmounts(isLow));
		}

		[HttpPost]
		public ActionResult GetValidSuits(bool isLow)
		{
			var table = GetTable();
			return Json(table.Suits(isLow));
		}

		[HttpPost]
		public ActionResult Bid(Bid bid)
		{
			var table = GetTable();
			var result = table.GotBid(UserId, bid);
			return Json(result);
		}

		[HttpPost]
		public ActionResult PlayDomino(string dots)
		{
			var table = GetTable();
			var result = table.Play(UserId, dots);
			return Json(result);
		}

		[HttpPost]
		public ActionResult Poll()
		{
			var msg = MailBox.GetInstance().GetMessage(UserId.ToString());
			if ( msg != null ) Log.WriteVerbose(string.Format("Sent {0} {1}", UserId, msg.MessageType));
			return Json(msg);
		}

		[HttpPost]
		public ActionResult GetBidSuggestion()
		{
			var table = GetTable();
			table.Table.GotHint();
			var bid = new BidCalc().GetBid(table.GetPlayerBySeat(0), table.Table, Log);
			var msg = bid.Amount == 0 ? "Pass" : bid.Amount + " " + (bid.IsLow ? " low" : "") + " " + bid.Trump;
			return Json(msg);
		}

		[HttpPost]
		public ActionResult GetPlaySuggestion()
		{
			var table = GetTable();
			table.Table.GotHint();
			var play = new PlayCalc().GetPlay(table.GetPlayerBySeat(0), table.Table, Log);
			var msg = play.ToString();
			return Json(msg);
		}

		[HttpPost]
		public ActionResult SaveHand()
		{
			var table = GetTable();
			new DataAccess().SaveHand(table.GetPlayerBySeat(0).Hand.ToString(), UserId);
			return null;
		}

	    public ActionResult Reset()
	    {
		    return View(new ResetModel(UserId));
	    }

		public ActionResult RemoveTable()
		{
			var model = new ResetModel(UserId);
			if ( model.HasTable )
			{
				model.RemoveTable();
				return RedirectToAction("ResetSuccess");
			}
			return null;
	    }

	    public ActionResult ResetSuccess()
	    {
		    return View(new ResetModel(UserId));
	    }
    }
}
