using System;

using IrcDotNet;
using IrcDotNet.Bot;

namespace Test
{
	public class Greeter<T> : IrcBotPlugin<T> where T : IrcClient
	{
		[OnJoin]
		public void Greet(IrcChannelUserEventArgs args)
		{
			Client.LocalUser.SendMessage(args.ChannelUser.Channel.Name,
			                             string.Format("Hello {0}", args.ChannelUser.User.NickName));
		}
	}}

