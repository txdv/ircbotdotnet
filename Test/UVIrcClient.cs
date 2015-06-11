using System;
using System.Net;
using IrcDotNet;
using IrcDotNet.Bot;
using LibuvSharp;

namespace Test
{
	class UVIrcClient : IrcClient
	{
		public Loop Loop { get; private set; }
		Tcp Client { get; set; }

		bool isConnected = false;
		public override bool IsConnected {
			get {
				return isConnected;
			}
		}

		public UVIrcClient()
			: this(Loop.Default)
		{
		}

		public UVIrcClient(Loop loop)
			: base()
		{
			Loop = loop;
		}

		public void Connect(string ipAddress, IrcUserRegistrationInfo registrationInfo)
		{
			Connect(IPAddress.Parse(ipAddress), registrationInfo);
		}

		public void Connect(string ipAddress, int port, IrcUserRegistrationInfo registrationInfo)
		{
			Connect(IPAddress.Parse(ipAddress), port, registrationInfo);
		}

		public void Connect(IPAddress ipAddress, IrcUserRegistrationInfo registrationInfo)
		{
			Connect(ipAddress, DefaultPort, registrationInfo);
		}

		public void Connect(IPAddress ipAddress, int port, IrcUserRegistrationInfo registrationInfo)
		{
			Connect(new IPEndPoint(ipAddress, port), registrationInfo);
		}

		public void Connect(IPEndPoint ipEndPoint, IrcUserRegistrationInfo registrationInfo)
		{
			Connect(registrationInfo);

			if (Client == null) {
				Client = new Tcp(Loop);
			}

			Client.Connect(ipEndPoint, (ex) => {
				isConnected = true;
				HandleClientConnected(registrationInfo);
				Client.Data += OnRead;
				Client.Resume();
			});

			HandleClientConnecting();
		}

		private void OnRead(ArraySegment<byte> buffer)
		{
			int start = buffer.Offset;
			for (int i = buffer.Offset; i + 1 < buffer.Count; i++) {
				if (buffer.Array[i] == 13 && buffer.Array[i + 1] == 10) {
					ParseMessage(TextEncoding.GetString(buffer.Array, start, i - start));
					i += 2;
					start = i;
				}
			}
		}

		protected override void WriteMessage(string line, object token)
		{
			Client.Write(TextEncoding, line + Environment.NewLine, (ex) => {
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

