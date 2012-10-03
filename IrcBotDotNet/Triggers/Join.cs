using System;
using System.Reflection;

using IrcDotNet;
using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	class JoinTrigger<T> : Trigger<T> where T : IrcClient
	{
		MethodInfo Method { get; set; }

		public JoinTrigger(IrcBotPlugin<T> plugin, MethodInfo method)
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

