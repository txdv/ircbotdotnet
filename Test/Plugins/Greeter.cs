using System;

using IrcDotNet;
using IrcDotNet.Bot;

namespace Test
{
	public class Greeter<T> : IrcBotPlugin<T> where T : IrcClient
	{
		[OnUserJoin]
		public void Greet(IrcChannelUserEventArgs args)
		{
			Reply("Hello {0}", args.ChannelUser.User.NickName);
		}
	}}

