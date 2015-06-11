using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using IrcDotNet;
using IrcDotNet.Bot;
using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	public abstract class IrcBotPlugin<T> where T : IrcClient
	{
		List<PreCommandTrigger<T>> PreCommands { get; set; } = new List<PreCommandTrigger<T>>();
		List<CommandTrigger<T>>    Commands    { get; set; } = new List<CommandTrigger<T>>();
		List<UserJoinTrigger<T>>   UserJoins   { get; set; } = new List<UserJoinTrigger<T>>();
		List<UserLeaveTrigger<T>>  UserLeaves  { get; set; } = new List<UserLeaveTrigger<T>>();

		public string DefaultPrefix { get; set; }

		public IrcBot<T> Bot { get; internal set; }
		public T Client { get; internal set; }
		public IrcLocalUser LocalUser {
			get {
				return Client.LocalUser;
			}
		}

		protected string destination;
		protected bool Reply(string text, params string[] values)
		{
			if (!string.IsNullOrEmpty(destination)) {
				LocalUser.SendMessage(destination, string.Format(text, values));
				return true;
			}
			return false;
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
					} else if (attribute is OnUserJoinAttribute) {
						if (member is MethodInfo) {
							UserJoins.Add(new UserJoinTrigger<T>(this, member as MethodInfo));
						}
					} else if (attribute is OnUserLeaveAttribute) {
						if (member is MethodInfo) {
							UserLeaves.Add(new UserLeaveTrigger<T>(this, member as MethodInfo));
						}
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

		void ExecuteCommands(MessageType type, IrcMessageEventArgs args)
		{
			foreach (var command in Commands) {
				if (command.Handle(type, args)) {
					return;
				}
			}
		}

		internal void HandleMessageReceived(object sender, IrcMessageEventArgs e)
		{
			destination = e.GetDestination();
			HandleMessageReceived(MessageType.Query, sender, e);
			destination = null;
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
				PreCommands[i].Handle(type, args, (value) => {

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

		internal void HandleUserJoined(object sender, IrcChannelUserEventArgs e)
		{
			destination = e.GetDestination();
			UserJoins.ForEach(join => join.Handle(e));
			destination = null;
		}

		internal void HandeUserLeft(object sender, IrcChannelUserEventArgs e)
		{
			destination = e.GetDestination();
			UserLeaves.ForEach(leave => leave.Handle(e));
			destination = null;
		}
	}
}

