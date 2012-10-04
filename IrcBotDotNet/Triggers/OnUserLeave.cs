using System;
using System.Reflection;

namespace IrcDotNet.Bot
{
	public class OnUserLeaveAttribute : IrcBotAttribute
	{
		public OnUserLeaveAttribute()
		{
		}

		public OnUserLeaveAttribute(string channel)
		{
			Channel = channel;
		}

		public string Channel { get; set; }
	}

	class UserLeaveTrigger<T> : Trigger<T> where T : IrcClient
	{
		MethodInfo Method { get; set; }

		public UserLeaveTrigger(IrcBotPlugin<T> plugin, MethodInfo method)
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

