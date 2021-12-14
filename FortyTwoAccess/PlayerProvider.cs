using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Security;
using FortyTwoLib.Database;
using FortyTwoLib.Logging;
using Player = FortyTwoLib.Database.Player;

namespace FortyTwoAccess
{
	public class PlayerProviderx : MembershipProvider
	{
		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
				if ( db.Player.Any(p => p.Email == email) )
				{
					status = MembershipCreateStatus.DuplicateEmail;
					return null;
				}
				if ( db.Player.Any(p => p.PlayerName == username) )
				{
					status = MembershipCreateStatus.DuplicateUserName;
					return null;
				}
				if ( db.Player.Any(p => p.PlayerId == (int?) providerUserKey) )
				{
					status = MembershipCreateStatus.DuplicateProviderUserKey;
					return null;
				}

				try
				{
					var player = new Player() {
						PlayerName = username,
						Password = password,
						Email = email,
						PasswordQuestion = passwordQuestion,
						PasswordAnswer = passwordAnswer,
						IsApproved = isApproved,
						Comment = "",
						CreationDate = DateTime.Today,
						IsLockedOut = false,
						LastActivityDate = DateTime.Today,
					};
					db.Player.InsertOnSubmit(player);
					db.SubmitChanges();
					status = MembershipCreateStatus.Success;
					return GetMemberShipUser(player);
				}
				catch (Exception ex)
				{
					status = MembershipCreateStatus.ProviderError;
					return null;
				}
			}
		}

		private MembershipUser GetMemberShipUser(Player player)
		{
			if ( player == null ) return null;
			var ret = new MembershipUser("PlayerProvider", player.PlayerName, player.PlayerId, player.Email, player.PasswordQuestion, "", player.IsApproved,
				player.IsLockedOut, player.CreationDate, player.LastLoginDate.GetValueOrDefault(), player.LastActivityDate.GetValueOrDefault(), 
				player.LastPasswordChangeDate.GetValueOrDefault(), player.LastLockoutDate.GetValueOrDefault());
			return ret;
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override string ResetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}

		public override bool ValidateUser(string username, string password)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
				var query =
					from p in db.Player
					where p.PlayerName == username
						&& p.Password == password
					select p
					;
				return query.Any();
			}
		}

		public override bool UnlockUser(string userName)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
				var player = db.Player.SingleOrDefault(p => p.PlayerName == userName);
				if ( player == null ) return false;
				player.IsLockedOut = false;
				db.SubmitChanges();
				return true;
			}
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
				var player = db.Player.SingleOrDefault(p => p.PlayerId == (int?) providerUserKey);
				return GetMemberShipUser(player);
			}
		}

		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
				var player = db.Player.SingleOrDefault(p => p.PlayerName == username);
				return GetMemberShipUser(player);
			}
		}

		public override string GetUserNameByEmail(string email)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
				var player = db.Player.SingleOrDefault(p => p.Email == email);
				if ( player == null ) return null;
				return player.PlayerName;
			}
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override bool EnablePasswordRetrieval
		{
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordReset
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { throw new NotImplementedException(); }
		}


		public override string ApplicationName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { throw new NotImplementedException(); }
		}

		public override int PasswordAttemptWindow
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresUniqueEmail
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredPasswordLength
		{
			get { return 6; }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { return 0; }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { return ""; }
		}
	}
}
