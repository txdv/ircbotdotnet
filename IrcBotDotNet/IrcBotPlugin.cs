using System;

namespace IrcBotDotNet
{
	public abstract class IrcBotPlugin<T> where T : IrcClient
	{
		List<PreCommandTrigger<T>> PreCommands { get; set; }
		List<CommandTrigger<T>>    Commands    { get; set; }
		List<JoinTrigger<T>>       Joins       { get; set; }

		public string DefaultPrefix { get; set; }

		public IrcBot<T> Bot { get; internal set; }
		public T Client { get; internal set; }

		public IrcBotPlugin()
		{
			PreCommands = new List<PreCommandTrigger<T>>();
			Commands    = new List<CommandTrigger<T>>();
			Joins       = new List<JoinTrigger<T>>();
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
					} else if (attribute is OnJoinAttribute) {
						if (member is MethodInfo) {
							Joins.Add(new JoinTrigger<T>(this, member as MethodInfo));
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

		internal void HandleUserJoined(object sender, IrcChannelUserEventArgs e)
		{
			Joins.ForEach((join) => join.Handle(e));
		}
	}
}

