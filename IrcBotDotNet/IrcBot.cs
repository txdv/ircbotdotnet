using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using IrcDotNet;
using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	public abstract class IrcBotPlugin<T> where T : IrcClient
	{
		List<PreCommandTrigger<T>> PreCommands { get; set; }
		List<CommandTrigger<T>>    Commands    { get; set; }

		public string DefaultPrefix { get; set; }

		public IrcBot<T> Bot { get; internal set; }
		public T Client { get; internal set; }

		public IrcBotPlugin()
		{
			Commands = new List<CommandTrigger<T>>();
		}

		internal void Register()
		{
			var type = GetType();

			foreach (var member in type.GetMembers()) {
				foreach (object attribute in member.GetCustomAttributes(true)) {
					if (attribute is OnCommandAttribute) {
						if (member is MethodInfo) {
							Commands.Add(new MethodCommandTrigger<T>(this, attribute as OnCommandAttribute, member as MethodInfo));
						} else if (member is PropertyInfo) {
							Commands.Add(new PropertyCommandTrigger<T>(this, attribute as OnCommandAttribute, member as PropertyInfo));
						}
						/*
					} else if (attribute is OnJoinAttribute) {
						if (member is MethodInfo) {
							Joins.Add(new JoinTrigger(this, member as MethodInfo));
						}
						*/
					} else if (attribute is PreCommandAttribute) {
						if (member is MethodInfo) {
							PreCommands.Add(new MethodPreCommandTrigger<T>(this, attribute as PreCommandAttribute, member as MethodInfo));
						} else if (member is PropertyInfo) {
							PreCommands.Add(new PropertyPreCommandTrigger<T>(this, attribute as PreCommandAttribute, member as PropertyInfo));
						}
					}
				}
			}
		}

		internal void HandleMessageReceived(object sender, IrcMessageEventArgs e)
		{
			HandleMessageReceived(MessageType.Query, sender, e);
		}

		void HandleMessageReceived(MessageType type, object sender, IrcMessageEventArgs args)
		{
			int count = 0;
			bool allTrue = true;

			if (PreCommands.Count == 0) {
				ExecuteCommands(type, args);
				return;
			}

			for (int i = 0; i < PreCommands.Count; i++) {
				PreCommands[i].Handle(type, args, delegate (bool value) {

					if (!value) {
						allTrue = false;
					}

					count++;

					if (allTrue && count == PreCommands.Count) {
						ExecuteCommands(type, args);
					}
				});
			}
		}

		void ExecuteCommands(MessageType type, IrcMessageEventArgs args)
		{
			foreach (var command in Commands) {
				if (command.Handle(type, args)) {
					return;
				}
			}
		}
	}

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
		}

		protected virtual void HandleMessageReceived(object sender, IrcMessageEventArgs e)
		{
			Each((plugin) => plugin.HandleMessageReceived(sender, e));
		}

		private void Each(Action<IrcBotPlugin<T>> callback)
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

