using Discord;
using Discord.WebSocket;
using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Bot
{
	public class Call : IDisposable
	{
		private CaptureCollection args;
		private CancellationToken cancelToken;
		// todo: Implement prompt interactions (deleted if the same message is sent, enough other messages are sent, or enough time passes).
		// todo: Implement information interactions (deleted when enough time passes).
		private List<IDeletable> interactions;

		public SocketMessage Message { get; private set; }
		public bool IsCancellationRequested {
			get {
				return cancelToken.IsCancellationRequested;
			}
		}
		public static string Prefix {
			get {
				return CloudConfigurationManager.GetSetting("Bot.CmdsManager.Prefix");
			}
		}

		internal Call(SocketMessage message, CaptureCollection args, CancellationToken cancelToken)
		{
			this.args = args;
			this.cancelToken = cancelToken;
			this.interactions = new List<IDeletable>();
			this.Message = message;
			this.interactions.Add(message);
			GC.SuppressFinalize(this.interactions);
		}

		public string GetArgString(int index) {
			return args[index].Value;
		}

		public int GetArgInteger(int index) {
			return int.Parse(GetArgString(index));
		}

		public void RegisterInteraction(IDeletable interaction)
		{
			interactions.Add(interaction);
		}

		public void Dispose()
		{
			if (interactions != null)
			{
				foreach (IDeletable deletable in interactions)
				{
					deletable.DeleteAsync();
				}
				interactions = null;
			}
		}

		~Call() {
			Dispose();
		}
	}
}
