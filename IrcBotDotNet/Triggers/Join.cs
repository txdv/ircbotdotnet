using System;
using System.Reflection;

using IrcDotNet;
using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	/*
	class JoinTrigger : Trigger
	{
		MethodInfo Method { get; set; }

		public JoinTrigger(IrcBotPlugin plugin, MethodInfo method)
			: base(plugin)
		{
			Method = method;
		}

		public bool Handle(JoinEventArgs args)
		{
			Invoke(Method, GetValues(Method.GetParameters(), (info) => {
				return Process(info, args);
			}));
			return true;
		}
	}
	*/
}

