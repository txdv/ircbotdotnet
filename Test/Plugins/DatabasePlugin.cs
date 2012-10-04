using System;
using System.Collections.Generic;

using IrcDotNet;
using IrcDotNet.Bot;

namespace Test
{
	public class DatabasePlugin<T> : IrcBotPlugin<T> where T : IrcClient
	{
		public AdminPlugin<T> AdminPlugin { get; protected set; }

		Dictionary<string, string> db = new Dictionary<string, string>();

		public DatabasePlugin(AdminPlugin<T> plugin)
		{
			AdminPlugin = plugin;
			On = true;
		}

		[PreCommand]
		public bool AdminCheck(string nick)
		{
			return AdminPlugin.IsAdmin(nick);
		}

		[OnCommand("db (?<On>(on|off))$")]
		[PreCommand]
		public bool On { get; set; }

		[OnCommand("db$")]
		public void Check(string target)
		{
			LocalUser.SendMessage(target, "service is " + (On ? "on" : "off"));
		}

		[OnCommand(@"db set (?<key>(\w+)) (?<value>(.+))")]
		public void Set(string key, string value)
		{
			db[key] = value;
		}

		[OnCommand(@"db get (?<key>(\w+))")]
		public void Get(string target, string key)
		{
			string val;
			if (!db.TryGetValue(key, out val)) {
				LocalUser.SendMessage(target, "no such key");
			} else {
				LocalUser.SendMessage(target, string.Format("{0}:{1}", key, db[key]));
			}
		}

		[OnCommand(@"db (remove|rm|del|delete) (?<key>(\w+))")]
		public void Delete(string target, string key)
		{
			if (db.ContainsKey(key)) {
				LocalUser.SendMessage(target, "deleted key " + key);
				db.Remove(key);
			} else {
				LocalUser.SendMessage(target, "no such key");
			}
		}
	}
}

