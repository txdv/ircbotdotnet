using System.Linq;
using System.Text.RegularExpressions;

namespace IrcDotNet.Bot.Extensions
{
	public static class HelperExtensions
	{
		public static string Get(this GroupCollection groupCollection, string key)
		{
			var res = groupCollection[key];

			if (res == null) {
				return null;
			} else if (res.Value == null) {
				return null;
			} else if (res.Value.Length == 0) {
				return null;
			}

			return res.Value;
		}

		public static string GetDestination(this IrcMessageEventArgs e)
		{
			var target = e.Targets.First();
			if (target is IrcUser) {
				return e.Source.Name;
			} else if (target is IrcChannel) {
				return target.Name;
			} else {
				return null;
			}
		}

		public static string GetDestination(this IrcChannelUserEventArgs e)
		{
			return e.ChannelUser.Channel.Name;
		}
	}
}

