using System;
using System.Collections.Generic;

using IrcDotNet;
using IrcDotNet.Bot;

namespace Test
{
	public class AdminPlugin<T> : IrcBotPlugin<T> where T : IrcClient
	{
		List<string> admins = new List<string>();

		public AdminPlugin(string admin)
		{
			admins.Add(admin);
		}

		public bool IsAdmin(string nick)
		{
			return admins.Contains(nick);
		}

		public bool Add(string nick)
		{
			if (IsAdmin(nick)) {
				return false;
			} else {
				admins.Add(nick);
				return true;
			}
		}

		public bool Delete(string nick)
		{
			if (!IsAdmin(nick)) {
				return false;
			} else {
				admins.Remove(nick);
				return true;
			}
		}

		[OnCommand("admin add (?<admin>(.+))")]
		public void AdminAdd(string nick, string admin)
		{
			if (!IsAdmin(nick)) {
				return;
			}

			if (Add(admin)) {
				Reply("added {0} to admins", admin);
			} else {
				Reply("{0} is already an admin", admin);
			}
		}

		[OnCommand("admin del (?<admin>(.+))")]
		public void AdminDel(string nick, string admin)
		{
			if (!IsAdmin(nick)) {
				return;
			}

			if (Delete(admin)) {
				Reply("removed {0} from the admin list", admin);
			} else {
				Reply("no such admin {0}", admin);
			}
		}

		[OnCommand("admin list$")]
		public void AdminList(string destination, string nick)
		{
			if (!IsAdmin(nick)) {
				return;
			}

			LocalUser.SendMessage(destination, admins.Count.ToString());
		}
	}
}

