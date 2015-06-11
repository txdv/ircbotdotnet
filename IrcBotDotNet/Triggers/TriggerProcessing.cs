using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

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
			} else if (info.ParameterType == typeof(IIrcMessageSource)) {
				return args.Source;
			} else if (info.ParameterType == typeof(IIrcMessageTarget)) {
				return args.Targets.First();
			} else if (info.ParameterType == typeof(IList<IIrcMessageTarget>)) {
				return args.Targets;
			} else if (info.ParameterType == typeof(IEnumerable<IIrcMessageTarget>)) {
				return args.Targets;
			} else if (info.ParameterType == typeof(string)) {
				var target = args.Targets.First();
				switch (info.Name.ToLower()) {
				case "destination":
				case "dest":
					if (target is IrcUser) {
						return args.Source.Name;
					} else if (target is IrcChannel) {
						return target.Name;
					}
					break;
				case "target":
					return target.Name;
				case "nick":
				case "source":
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
				case "comment":
					return args.Comment;
				}
			}
			return Process (info, args.ChannelUser);;
		}

		protected object Process(ParameterInfo info, IrcChannelUser user)
		{
			// TODO: expose user mode?

			if (info.ParameterType == typeof(IrcChannelUser)) {
				return user;
			} else {
				return Process(info, user.Channel) ?? Process(info, user.User);
			}
		}

		protected object Process(ParameterInfo info, IrcChannel channel)
		{
			// TODO: expose channel mode

			if (info.ParameterType == typeof(IrcChannel)) {
				return channel;
			} else if (info.ParameterType == typeof(IrcChannelType)) {
				return channel.Type;
			} else if (info.ParameterType == typeof(string)) {
				switch (info.Name.ToLower()) {
				case "channel":
				case "chan":
					return channel.Name;
				case "topic":
					return channel.Topic;
				}
			}
			return null;
		}

		protected object Process(ParameterInfo info, IrcUser user)
		{
			if (info.ParameterType == typeof(IrcUser)) {
				return user;
			} else if (info.ParameterType == typeof(TimeSpan)) {
				switch (info.Name.ToLower()) {
				case "idleduration":
				case "idle":
					return user.IdleDuration;
				}
			} else if (info.ParameterType == typeof(bool)) {
				switch (info.Name.ToLower()) {
				case "isonline":
				case "online":
					return user.IsOnline;
				case "isaway":
				case "away":
					return user.IsAway;
				}
			} else if (info.ParameterType == typeof(string)) {
				switch (info.Name.ToLower()) {
				case "nickname":
				case "nick":
					return user.NickName;
				case "realname":
				case "name":
				case "real":
					return user.RealName;
				case "awaymessage":
				case "awaymsg":
					return user.AwayMessage;
				}
			}
			return null;
		}
	}
}

