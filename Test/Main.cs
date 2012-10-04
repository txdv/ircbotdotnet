using System;

using IrcDotNet;
using IrcDotNet.Bot;

using LibuvSharp;

namespace Test
{
	public class MainClass
	{
		public static void Main(string[] args)
		{
			var client = new UVIrcClient();
			var bot = new IrcBot<UVIrcClient>(client);
			bot.Client.Connect("127.0.0.1", new IrcUserRegistrationInfo() {
				NickName = "txdv-bot",
				UserName = "txdvbot",
				RealName = "txdv bot",
			});

			bot.Client.Connected += (_, __) => client.Channels.Join("#help");

			var adminPlugin = new AdminPlugin<UVIrcClient>("bentkus");

			bot.Plugin(adminPlugin);
			bot.Plugin(new Greeter<UVIrcClient>());
			bot.Plugin(new DatabasePlugin<UVIrcClient>(adminPlugin));

			var stdin = new Poll(0);
			stdin.Start(PollEvent.Read, (_) => {
				var line = Console.ReadLine();
				switch (line) {
				case "quit":
					client.Close();
					stdin.Close();
					break;
				default:
					break;
				}
			});

			Loop.Default.Run();
		}
	}
}
