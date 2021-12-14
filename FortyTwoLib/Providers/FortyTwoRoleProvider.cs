using System;
using System.Configuration;
using System.Linq;
using System.Web.Security;
using FortyTwoLib.Database;

namespace FortyTwoLib.Providers
{
	public class FortyTwoRoleProvider : RoleProvider
	{
		public override bool IsUserInRole(string username, string roleName)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
				return db.Player.Any(p => p.PlayerName == username && p.Role.RoleName == roleName);
			}
		}

		public override string[] GetRolesForUser(string username)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
				return new[] {db.Player.Single(p => p.PlayerName == username).Role.RoleName};
			}
		}

		public override void CreateRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			throw new NotImplementedException();
		}

		public override bool RoleExists(string roleName)
		{
			throw new NotImplementedException();
		}

		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public override string[] GetUsersInRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override string[] GetAllRoles()
		{
			throw new NotImplementedException();
		}

		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			throw new NotImplementedException();
		}

		public override string ApplicationName { get; set; }
	}
}
