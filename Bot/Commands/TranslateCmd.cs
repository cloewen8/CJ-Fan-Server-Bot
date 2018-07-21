using Discord;
using Discord.WebSocket;
using Bot;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TranslateCmd
{
	class Cmd : ICmd
	{
		public Regex Pattern => new Regex("translate");

		public bool OwnerOnly => false;

		public Task Execute(SocketMessage message, CaptureCollection args, CancellationToken cancelToken)
		{
			EmbedBuilder builder = new EmbedBuilder
			{
				Title = "User Requested Translation"
			};
			EmbedFooterBuilder footer = new EmbedFooterBuilder
			{
				Text = $"Submitted by: {message.Author.Username} (${message.Author.Id})"
			};
			builder.Footer = footer;

			Task.Run(() =>
			{
				// todo: Use a clustered request.
				// todo: Handle a cancellation.
				Task<SocketMessage> originalRequest = message.RequestMessage("What was the original text?");
				Task<SocketMessage> translateRequest = message.RequestMessage("What is the text translated?");
				Task<SocketMessage> localeRequest = message.RequestMessage("What language did you translate it to?");
				
				builder.AddField("Original", originalRequest.Result.Content);
				builder.AddField("Translated", translateRequest.Result);
				builder.AddField("Locale", localeRequest.Result);
				message.Channel.SendMessageAsync("", false, builder);
			});

			return Task.CompletedTask;
		}
	}
}
