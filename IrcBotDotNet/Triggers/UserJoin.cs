using System;
using System.Reflection;

using IrcDotNet;
using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	public class OnUserJoinAttribute : IrcBotAttribute
	{
		public OnUserJoinAttribute()
		{
		}
		
		public OnUserJoinAttribute(string channel)
		{
			Channel = channel;
		}
		
		public string Channel { get; set; }
	}

	class UserJoinTrigger<T> : Trigger<T> where T : IrcClient
	{
		MethodInfo Method { get; set; }

		public UserJoinTrigger(IrcBotPlugin<T> plugin, MethodInfo method)
			: base(plugin)
		{
			Method = method;
		}

		public bool Handle(IrcChannelUserEventArgs args)
		{
			Invoke(Method, GetValues(Method.GetParameters(), (info) => {
				return Process(info, args);
			}));
			return true;
		}
	}
}

