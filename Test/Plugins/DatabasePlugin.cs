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
		public void Check()
		{
			Reply("service is {0}", On ? "on" : "off");
		}

		[OnCommand(@"db set (?<key>(\w+)) (?<value>(.+))")]
		public void Set(string key, string value)
		{
			db[key] = value;
		}

		[OnCommand(@"db get (?<key>(\w+))")]
		public void Get(string key)
		{
			string val;
			if (!db.TryGetValue(key, out val)) {
				Reply("no such key");
			} else {
				Reply("{0}:{1}", key, db[key]);
			}
		}

		[OnCommand(@"db (remove|rm|del|delete) (?<key>(\w+))")]
		public void Delete(string key)
		{
			if (db.ContainsKey(key)) {
				Reply("deleted key " + key);
				db.Remove(key);
			} else {
				Reply("no such key");
			}
		}
	}
}

