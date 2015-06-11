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
		public void Check(string destination)
		{
			LocalUser.SendMessage(destination, "service is " + (On ? "on" : "off"));
		}

		[OnCommand(@"db set (?<key>(\w+)) (?<value>(.+))")]
		public void Set(string key, string value)
		{
			db[key] = value;
		}

		[OnCommand(@"db get (?<key>(\w+))")]
		public void Get(string destination, string key)
		{
			string val;
			if (!db.TryGetValue(key, out val)) {
				LocalUser.SendMessage(destination, "no such key");
			} else {
				LocalUser.SendMessage(destination, string.Format("{0}:{1}", key, db[key]));
			}
		}

		[OnCommand(@"db (remove|rm|del|delete) (?<key>(\w+))")]
		public void Delete(string destination, string key)
		{
			if (db.ContainsKey(key)) {
				LocalUser.SendMessage(destination, "deleted key " + key);
				db.Remove(key);
			} else {
				LocalUser.SendMessage(destination, "no such key");
			}
		}
	}
}

