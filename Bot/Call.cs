using Discord.WebSocket;
using Microsoft.Azure;
using System.Text.RegularExpressions;
using System.Threading;

namespace Bot
{
	public class Call
	{
		private CaptureCollection args;
		private CancellationToken cancelToken;

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
			Message = message;
		}

		public string GetArgString(int index) {
			return args[index].Value;
		}

		public int GetArgInteger(int index) {
			return int.Parse(GetArgString(index));
		}
	}
}
