using System;
using System.Text;

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

			var adminPlugin = new AdminPlugin<UVIrcClient>("bentkus");

			bot.Plugin(adminPlugin);
			bot.Plugin(new Greeter<UVIrcClient>());
			bot.Plugin(new DatabasePlugin<UVIrcClient>(adminPlugin));

			UVTimer.Once(TimeSpan.FromSeconds(1), () => client.Channels.Join("#help"));

			var stdin = new TTY(0);
			stdin.Read(Encoding.Default, (line) => {
				line = line.Trim();
				switch (line) {
				case "quit":
					Loop.Default.Stop();
					break;
				default:
					break;
				}
			});
			stdin.Resume();

			Loop.Default.Run();
		}
	}
}
