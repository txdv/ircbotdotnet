using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using IrcDotNet;
using IrcDotNet.Bot.Extensions;

namespace IrcDotNet.Bot
{
	partial class Trigger<T> where T : IrcClient
	{
		private static Dictionary<Type, MethodInfo> tryParseMethods = new Dictionary<Type, MethodInfo>();

		static Trigger()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				Load(assembly);
			}
		}

		public static void Load(Assembly assembly)
		{
				foreach (var type in assembly.GetTypes()) {
					Load(type);
				}
		}

		public static void Load(Type type)
		{
			foreach (var tuple in GetTryParseMethod(type)) {
				if (!tryParseMethods.ContainsKey(tuple.Item1)) {
					tryParseMethods[tuple.Item1] = tuple.Item2;
				}
			}
		}

		static IEnumerable<Tuple<Type, MethodInfo>> GetTryParseMethod(Type type)
		{
			foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
				var param = method.GetParameters();
				if ((method.ReturnType == typeof(bool)) && (method.Name == "TryParse") && (param.Length == 2)) {
					if (param[0].ParameterType == typeof(string) && param[1].IsOut) {
						yield return Tuple.Create<Type, MethodInfo>(param[1].ParameterType.GetElementType(), method);
					}
				}
			}
			yield break;
		}

		protected bool TryParse(Type type, string text, out object obj)
		{
			obj = null;
			var tryParse = GetTryParse(type);

			if (tryParse == null) {
				return false;
			}

			var args = new object[] { text, null };
			tryParse.Invoke(null, args);
			obj = args[1];
			return true;
		}


		protected bool HasTryParse(Type type)
		{
			return GetTryParseMethod(type) != null;
		}

		protected MethodInfo GetTryParse(Type type)
		{
			MethodInfo mi;
			if (tryParseMethods.TryGetValue(type, out mi)) {
				return mi;
			}
			return null;
		}

		public IrcBotPlugin<T> Plugin { get; protected set; }

		public Trigger(IrcBotPlugin<T> plugin)
		{
			Plugin = plugin;
		}

		protected object[] GetValues(ParameterInfo[] parameters, Func<ParameterInfo, object> callback)
		{
			object[] values = new object[parameters.Length];

			for (int i = 0; i < parameters.Length; i++) {
				object o = callback(parameters[i]);

				if (o != null) {
					values[i] = o;
				}

				if (values[i] == null) {
					values[i] = null;
				}
			}
			return values;
		}

		protected void Debug(object[] parameters)
		{
			for (int i = 0; i < parameters.Length; i++) {
				object o = parameters[i];
				Console.WriteLine("{0}:{1}:{1}", i, parameters[i].GetType(), (o == null ? "(null)" : o));
			}
		}

		protected object Invoke(MethodInfo method, object[] parameters)
		{
			return method.Invoke(Plugin, parameters);
		}

		protected bool? GetBool(string text)
		{
			switch (text) {
			case "1":
			case "on":
			case "true":
				return true;
			case "0":
			case "off":
			case "false":
				return false;
			case null:
				return null;
			default:
				return null;
			}
		}
	}
}

