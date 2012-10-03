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
		public void AdminAdd(string target, string nick, string admin)
		{
			if (!IsAdmin(nick)) {
				return;
			}

			if (Add(admin)) {
				LocalUser.SendMessage(target, string.Format("added {0} to admins", admin));
			} else {
				LocalUser.SendMessage(target, string.Format("{0} is already an admin", admin));
			}
		}

		[OnCommand("admin del (?<admin>(.+))")]
		public void AdminDel(string target, string nick, string admin)
		{
			if (!IsAdmin(nick)) {
				return;
			}

			if (Delete(admin)) {
				LocalUser.SendMessage(target, string.Format("removed {0} from the admin list", admin));
			} else {
				LocalUser.SendMessage(target, string.Format("no such admin {0}", admin));
			}
		}

		[OnCommand("admin list$")]
		public void AdminList(string target, string nick)
		{
			if (!IsAdmin(nick)) {
				return;
			}

			LocalUser.SendMessage(target, admins.Count.ToString());
		}
	}
}

