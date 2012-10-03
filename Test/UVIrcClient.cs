using System;
using IrcDotNet;
using IrcDotNet.Bot;
using LibuvSharp;

namespace Test
{
	class UVIrcClient : IrcClient
	{
		public Loop Loop { get; private set; }
		Tcp Client { get; set; }

		public UVIrcClient()
			: this(Loop.Default)
		{
		}

		public override bool IsConnected {
			get {
				return true;
			}
		}

		public UVIrcClient(Loop loop)
			: base()
		{
			Loop = loop;
		}

		public void Connect(string ipAddress, IrcUserRegistrationInfo registrationInfo)
		{
			Connect(registrationInfo);

			if (Client == null) {
				Client = new Tcp(Loop);
			}

			Client.Connect(ipAddress, DefaultPort, (e) => {
				HandleClientConnected(registrationInfo);
				Client.Read(OnRead);
				Client.Resume();
			});

			HandleClientConnecting();
		}

		private void OnRead(ByteBuffer buffer)
		{
			int start = buffer.Start;
			for (int i = buffer.Start; i + 1 < buffer.End; i++) {
				if (buffer.Buffer[i] == 13 && buffer.Buffer[i + 1] == 10) {
					ParseMessage(TextEncoding.GetString(buffer.Buffer, start, i - start));
					i += 2;
					start = i;
				}
			}
		}

		protected override void WriteMessage(string line, object token)
		{
			Client.Write(TextEncoding, line + Environment.NewLine, (_) => {
				OnRawMessageSent(token as IrcRawMessageEventArgs);
			});
		}

		public override void Quit(int timeout, string comment)
		{
			base.Quit(timeout, comment);
			Client.Shutdown(HandleClientDisconnected);
		}

		public void Close()
		{
			Client.Close();
		}
	}
}

