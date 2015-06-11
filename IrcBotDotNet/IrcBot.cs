using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using IrcDotNet;
using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	public class IrcBot<T> where T : IrcClient
	{
		private List<IrcBotPlugin<T>> plugins = new List<IrcBotPlugin<T>>();

		public T Client { get; private set; }
		public string DefaultPrefix { get; set; }

		public IrcBot(T client)
		{
			DefaultPrefix = "!";
			Client = client;

			Client.Connected += HandleConnected;
		}

		void HandleConnected(object sender, EventArgs e)
		{
			Client.LocalUser.MessageReceived += HandleMessageReceived;
			Client.LocalUser.JoinedChannel += HandleJoinedChannel;
			Client.LocalUser.LeftChannel += HandleLeftChannel;
		}

		void HandleJoinedChannel(object sender, IrcChannelEventArgs e)
		{
			e.Channel.UserJoined += HandleUserJoined;
			e.Channel.UserLeft += HandleUserLeft;

			e.Channel.MessageReceived += HandleMessageReceived;
		}

		void HandleLeftChannel(object sender, IrcChannelEventArgs e)
		{
			e.Channel.UserJoined -= HandleUserJoined;
			e.Channel.UserLeft -= HandleUserLeft;

			e.Channel.MessageReceived -= HandleMessageReceived;
		}

		void HandleUserJoined(object sender, IrcChannelUserEventArgs e)
		{
			Each(plugin => plugin.HandleUserJoined(sender, e));
		}

		void HandleUserLeft(object sender, IrcChannelUserEventArgs e)
		{
			Each(plugin => plugin.HandeUserLeft(sender, e));
		}

		void HandleMessageReceived(object sender, IrcMessageEventArgs e)
		{
			Each(plugin => plugin.HandleMessageReceived(sender, e));
		}

		void Each(Action<IrcBotPlugin<T>> callback)
		{
			foreach (var plugin in plugins) {
				callback(plugin);
			}
		}

		public bool Plugin(IrcBotPlugin<T> plugin)
		{
			if (!(plugin is IrcBotPlugin<T>)) {
				return false;
			}

			plugin.Bot = this;
			plugin.Client = Client;
			plugin.Register();
			plugins.Add(plugin);
			return true;
		}

		public void Plugin(IEnumerable plugins)
		{
			foreach (var plugin in plugins) {
				Plugin(plugin as IrcBotPlugin<T>);
			}
		}

		public void LoadTryParse(Assembly assembly)
		{
			Trigger<T>.Load(assembly);
		}

		public void LoadTryParse(Type type)
		{
			Trigger<T>.Load(type);
		}
	}
}

