using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Security;
using FortyTwoLib.Database;

namespace FortyTwoAccess
{
	public class FortyTwoRoleProviderx : RoleProvider
	{
		public override bool IsUserInRole(string username, string roleName)
		{
			using ( var db = new FortyTwoDataContext(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString) )
			{
			}
		}

		public override string[] GetRolesForUser(string username)
		{
			throw new NotImplementedException();
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
