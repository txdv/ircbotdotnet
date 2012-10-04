using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	partial class Trigger<T> where T : IrcClient
	{
		protected object Process(ParameterInfo info, Match match)
		{
			if (info.ParameterType == typeof(Match)) {
				return match;
			} else {
				return Process(info, match.Groups);
			}
		}

		protected object Process(ParameterInfo info, GroupCollection groups)
		{
			if (info.ParameterType == typeof(GroupCollection)) {
				return groups;
			} else if (info.ParameterType == typeof(string)) {
				return groups.Get(info.Name);
			} else {
				if (HasTryParse(info.ParameterType)) {
					string str = groups.Get(info.Name);
					if (str == null) {
						return null;
					} else {
						object o;
						if (TryParse(info.ParameterType, str, out o)) {
							return o;
						} else {
							return null;
						}
					}
				}
				return null;
			}
		}

		protected object Process(ParameterInfo info, IrcMessageEventArgs args)
		{
			if (info.ParameterType == typeof(IrcMessageEventArgs)) {
				return args;
			} else if (info.ParameterType == typeof(Encoding)) {
				return args.Encoding;
			} else {
				switch (info.Name.ToLower()) {
				case "nick":
				case "target":
					return args.Source.Name;
				case "message":
				case "msg":
				case "text":
					return args.Text;
				}
			}
			return null;
		}

		protected object Process(ParameterInfo info, IrcChannelUserEventArgs args)
		{
			if (info.ParameterType == typeof(IrcChannelUserEventArgs)) {
				return args;
			} else if (info.ParameterType == typeof(string)) {
				switch (info.Name.ToLower()) {
				case "channel":
					return args.ChannelUser.Channel.Name;
				case "nickname":
				case "nick":
				case "name":
					return args.ChannelUser.User.NickName;
				case "realname":
					return args.ChannelUser.User.RealName;
				case "hostname":
				case "host":
					return args.ChannelUser.User.HostName;
				default:
					return null;
				}
			} else {
				return null;
			}
		}
	}
}

