using Discord;
using Discord.WebSocket;
using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
	public class Call : IDisposable
	{
		private CaptureCollection args;
		private CancellationToken cancelToken;
		private List<IDeletable> interactions;
		private List<IDeletable> information;

		private const int INFORMATION_DELETE_DELAY = 600000; // 10 minutes

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
			this.information = new List<IDeletable>();
			this.Message = message;
			this.interactions.Add(message);
			GC.SuppressFinalize(this.interactions);
			GC.SuppressFinalize(this.information);
		}

		public string GetArgString(int index) {
			return args[index].Value;
		}

		public int GetArgInteger(int index) {
			return int.Parse(GetArgString(index));
		}

		public void RegisterInteraction(IDeletable interaction)
		{
			this.interactions.Add(interaction);
		}

		public void RegisterInformation(IDeletable information)
		{
			this.information.Add(information);
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
				Task.Delay(INFORMATION_DELETE_DELAY).ContinueWith((_) => {
					foreach (IDeletable deletable in information)
					{
						deletable.DeleteAsync();
					}
					information = null;
				});
			}
		}
	}
}
