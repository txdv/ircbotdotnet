using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using IrcDotNet;
using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	abstract class PreCommandTrigger<T> : Trigger<T> where T : IrcClient
	{
		public PreCommandAttribute Attribute { get; set; }

		public PreCommandTrigger(IrcBotPlugin<T> plugin, PreCommandAttribute attribute)
			: base(plugin)
		{
			Attribute = attribute;
		}

		public abstract void Handle(MessageType type, IrcMessageEventArgs args, Action<bool> callback);
	}

	class MethodPreCommandTrigger<T> : PreCommandTrigger<T> where T : IrcClient
	{
		MethodInfo Method { get; set; }
		bool ReturnsBool { get; set; }

		public MethodPreCommandTrigger(IrcBotPlugin<T> plugin, PreCommandAttribute attribute, MethodInfo method)
			: base(plugin, attribute)
		{
			Method = method;
			if (method.ReturnType == typeof(bool)) {
				ReturnsBool = true;
			} else if (method.ReturnType != typeof(void)) {
				throw new Exception("method has to return either void or bool");
			}
		}

		public override void Handle(MessageType type, IrcMessageEventArgs args, Action<bool> callback)
		{
			if (ReturnsBool) {
				bool result = (bool)Invoke(Method, GetValues(Method.GetParameters(), (info) => {
					return Process(info, args);
				}));
				callback(result);
			} else {
				Invoke(Method, GetValues(Method.GetParameters(), (info) => {
					if (info.ParameterType == typeof(Action<bool>)) {
						return TimeoutCall(Attribute.Timeout, delegate (bool res) {
							callback(res);
						}, Attribute.DefaultValue);
					} else {
						return Process(info, args);
					}
				}));
			}
		}

		Action<bool> TimeoutCall(TimeSpan span, Action<bool> callback, bool defaultValue = false)
		{
			/*
			var timer = Context.CreateTimerWatcher(span, delegate {
				callback(defaultValue);
			});

			Action<bool> ret = delegate (bool res) {
				if (timer.IsRunning) {
					timer.Stop();
					callback(res);
				}
			};

			timer.Start();

*/
			return (res) => { };
		}
	}

	class PropertyPreCommandTrigger<T> : PreCommandTrigger<T> where T : IrcClient
	{
		PropertyInfo Property { get; set; }

		public PropertyPreCommandTrigger(IrcBotPlugin<T> plugin, PreCommandAttribute attribute, PropertyInfo property)
			: base(plugin, attribute)
		{
			Property = property;
		}

		public override void Handle(MessageType type, IrcMessageEventArgs args, Action<bool> callback)
		{
			if (Property.PropertyType == typeof(string)) {
				string val = Property.GetValue(Plugin, null) as string;
				var b = GetBool(val);
				if (b.HasValue) {
					callback(b.Value);
				} else {
					throw new Exception(string.Format("string property value not supported: {0}", val));
				}
			} else if (Property.PropertyType == typeof(bool)) {
				callback((bool)Property.GetValue(Plugin, null));
			} else {
				throw new Exception("Property type not supported");
			}
		}
	}

	abstract class CommandTrigger<T> : Trigger<T> where T : IrcClient
	{
		public OnCommandAttribute Attribute { get; set; }

		public string Prefix {
			get {
				return Attribute.Prefix ?? Plugin.DefaultPrefix ?? Plugin.Bot.DefaultPrefix;
			}
		}

		public CommandTrigger(IrcBotPlugin<T> plugin, OnCommandAttribute attribute)
			: base(plugin)
		{
			Attribute = attribute;
		}

		public abstract bool Handle(MessageType type, IrcMessageEventArgs args);

		protected Match GetMatch(MessageType type, IrcMessageEventArgs args)
		{
			string msg = args.Text;

			if (!msg.StartsWith(Prefix)) {
				return null;
			}

			msg = msg.Substring(Prefix.Length);

			var match = Attribute.Regex.Match(msg);

			if (Attribute.Type != MessageType.All && Attribute.Type != type) {
				return null;
			}

			if (!args.Text.StartsWith(Prefix)) {
				return null;
			}

			if (!match.Success) {
				return null;
			}

			return match;
		}
	}

	class MethodCommandTrigger<T> : CommandTrigger<T> where T : IrcClient
	{
		public MethodInfo Method { get; set; }

		public MethodCommandTrigger(IrcBotPlugin<T> plugin, OnCommandAttribute attribute, MethodInfo method)
			: base(plugin, attribute)
		{
			Method = method;
		}

		public override bool Handle(MessageType type, IrcMessageEventArgs args)
		{
			var match = GetMatch(type, args);
			if (match == null) {
				return false;
			}

			Invoke(Method, GetValues(Method.GetParameters(), (info) => {
				return Process(info, match) ?? Process(info, args);
			}));

			return true;
		}
	}

	class PropertyCommandTrigger<T> : CommandTrigger<T> where T : IrcClient
	{

		public PropertyInfo Property { get; set; }

		public PropertyCommandTrigger(IrcBotPlugin<T> plugin, OnCommandAttribute attribute, PropertyInfo property)
			: base(plugin, attribute)
		{
			Property = property;
		}

		public bool GenericSetValue(string text)
		{
			var type = Property.PropertyType;

			object obj = null;
			if (TryParse(type, text, out obj)) {
				SetValue(obj);
				return true;
			} else {
				return false;
			}
		}

		public override bool Handle(MessageType type, IrcMessageEventArgs args)
		{
			var match = GetMatch(type, args);
			if (match == null) {
				return false;
			}

			string stringValue = match.Groups.Get(Property.Name);

			if (stringValue == null) {
				return false;
			}

			var t = Property.PropertyType;
			if (t == typeof(bool)) {
				bool? value = GetBool(stringValue);
				if (!value.HasValue) {
					return false;
				}

				return SetValue(value.Value);
			} else if (t == typeof(int)) {
				int value = 0;
				if (!int.TryParse(stringValue, out value)) {
					return false;
				}

				return SetValue(value);
			} else if (HasTryParse(t)) {
				return GenericSetValue(stringValue);
			} else {
				return false;
			}
		}

		bool SetValue(object value)
		{
			if (!Property.CanWrite) {
				return false;
			}

			Property.SetValue(Plugin as object, value, null);
			return true;
		}
	}
}

