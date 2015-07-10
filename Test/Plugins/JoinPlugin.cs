using System;
using System.Linq;

using IrcDotNet;
using IrcDotNet.Bot;

namespace Test
{
	public class JoinPlugin<T> : IrcBotPlugin<T> where T : IrcClient
	{
		public AdminPlugin<T> AdminPlugin { get; protected set; }

		public JoinPlugin(AdminPlugin<T> plugin)
		{
			AdminPlugin = plugin;
		}

		[PreCommand]
		public bool AdminCheck(string nick)
		{
			return AdminPlugin.IsAdmin(nick);
		}

		[OnCommand("join (?<channel>#.+)")]
		public void Join(string channel)
		{
			Client.Channels.Join(channel);
		}

		[OnCommand(@"part (?<channel>#\w+?)( (?<comment>.+$)|($))")]
		public void Part(string channel, string comment)
		{
			Client.Channels.Leave(new string[] { channel }, comment);
		}
	}
}

