using Discord;
using Discord.WebSocket;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot
{
	public static class MessageExtensions
	{
		internal static CmdsManager Cmds { private get; set; }
		
		public static Task<SocketMessage> RequestMessage(this SocketMessage original, Call call,
			string text, bool isTTS = false, Embed embed = null, RequestOptions options = null,
			Regex pattern = null)
		{
			if (call == null)
			{
				throw new ArgumentNullException("call");
			}

			MessageRequest request = new MessageRequest(call, original.Author,
				original.Channel, text, isTTS, embed, options,
				pattern);
			Cmds.RegisterRequests(request);
			return request.Task;
		}

		public static Task<SocketMessage[]> RequestMessages(this SocketMessage original, Call call,
			string[] text, bool[] isTTS = null, Embed[] embed = null, RequestOptions[] options = null,
			Regex[] pattern = null)
		{
			if (call == null)
			{
				throw new ArgumentNullException("call");
			}
			else if (text.Length < 2)
			{
				throw new ArgumentOutOfRangeException("text", "Use RequestMessage for singular requests.");
			}
			else if (isTTS != null && isTTS.Length != text.Length)
			{
				throw new ArgumentOutOfRangeException("isTTS", "Either pass in null or an equal number of text.");
			}
			else if (embed != null && embed.Length != text.Length)
			{
				throw new ArgumentOutOfRangeException("embed", "Either pass in null or an equal number of text.");
			}
			else if (options != null && options.Length != text.Length)
			{
				throw new ArgumentOutOfRangeException("options", "Either pass in null or an equal number of text.");
			}
			else if (pattern != null && pattern.Length != text.Length)
			{
				throw new ArgumentOutOfRangeException("pattern", "Either pass in null or an equal number of text.");
			}
			
			MessageRequest[] cluster = new MessageRequest[text.Length];
			Task<SocketMessage>[] tasks = new Task<SocketMessage>[text.Length];
			MessageRequest request;
			for (int position = 0; position < text.Length; position++)
			{
				request = new MessageRequest(call, original.Author,
					original.Channel, text[position], isTTS?[position] ?? false, embed?[position], options?[position],
					pattern?[position]);
				cluster[position] = request;
				tasks[position] = request.Task;
			}
			Cmds.RegisterRequests(cluster);
			return Task.WhenAll(tasks);
		}
	}

	internal class MessageRequest
	{
		private readonly SocketUser author;
		private readonly Regex pattern;
		private readonly ISocketMessageChannel channel;
		private readonly string text;
		private readonly bool isTTS;
		private readonly Embed embed;
		private readonly RequestOptions options;

		internal Call Call { get; private set; }
		internal Task<SocketMessage> Task { get; private set; }
		internal Action<SocketMessage> Resolve { get; private set; }
		internal Action Cancel { get; private set; }
		
		public MessageRequest(Call call, SocketUser author,
			ISocketMessageChannel channel, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null,
			Regex pattern = null)
		{
			TaskCompletionSource<SocketMessage> taskSource = new TaskCompletionSource<SocketMessage>();

			Call = call;
			Task = taskSource.Task;
			Resolve = taskSource.SetResult;
			Cancel = taskSource.SetCanceled;
			this.author = author;
			this.pattern = pattern ?? new Regex(".");
			this.channel = channel;
			this.text = text;
			this.isTTS = isTTS;
			this.embed = embed;
			this.options = options;
		}

		public async void Prompt()
		{
			Call.RegisterInteraction(await channel.SendMessageAsync(text, isTTS, embed, options));
		}

		public bool Matches(SocketMessage candidate)
		{
			return candidate.Author.Equals(author) && candidate.Channel.Equals(channel) && pattern.IsMatch(candidate.Content);
		}
	}
}
