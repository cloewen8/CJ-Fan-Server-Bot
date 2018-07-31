using Bot.res;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bot
{
	class RuleAgreement : ILoadable
	{
		private double refreshTime = 0;
		private RestUserMessage codeMessage;
		private string code;

		private const ulong APPROVED_ROLE = 459849869339787277;
		private const ulong RULES_CHANNEL = 465662927064793108;
		private DateTime EPOCH = new DateTime(1970, 1, 1);
		private const double REFRESH_DELAY = 30000;
		private const int CODE_LENGTH = 8;

		public async void Load(DiscordSocketClient client)
		{
			if (!Bot.Config.IsDevelopment)
			{
				SocketTextChannel rulesChannel = (SocketTextChannel)client.GetChannel(RULES_CHANNEL);
				try
				{
					foreach (IMessage message in await rulesChannel.GetMessagesAsync(2).FlattenAsync())
					{
						if (message.Author.Id.Equals(client.CurrentUser.Id))
						{
							codeMessage = (RestUserMessage) message;
							break;
						}
					}
				}
				catch (Exception exc)
				{
					Trace.WriteLine(exc);
				}
				if (codeMessage == null)
				{
					codeMessage = await rulesChannel.SendMessageAsync(getCodeMessage(getCurrentTime()));
				}
				else
				{
					await codeMessage.ModifyAsync((messageProps) =>
					{
						messageProps.Content = getCodeMessage(getCurrentTime());
					}, new RequestOptions()
					{
						RetryMode = RetryMode.RetryTimeouts | RetryMode.Retry502
					});
				}

				client.UserJoined += OnUserJoined;
				client.MessageReceived += OnMessage;
				client.UserVoiceStateUpdated += OnVoiceUpdated;
			}
		}

		private async Task OnUserJoined(SocketGuildUser user)
		{
			try
			{
				await (await user.GetOrCreateDMChannelAsync())
					.SendMessageAsync(strings.AgreementNotice);
			}
			catch
			{
			}
		}

		private async Task OnMessage(SocketMessage message)
		{
			IGuildUser user = (IGuildUser) message.Author;
			if (!user.IsBot && !user.RoleIds.Any((id) => id == APPROVED_ROLE))
			{
				if (message.Content.Equals(code))
				{
					await user.AddRoleAsync(user.Guild.Roles.First((role) => role.Id == APPROVED_ROLE));
					await UpdateMessage();
					try
					{
						await (await user.GetOrCreateDMChannelAsync())
							.SendMessageAsync(strings.AgreementSuccess);
					}
					catch
					{
					}
				}
				await message.DeleteAsync(new RequestOptions() { RetryMode = RetryMode.AlwaysRetry });
			}
		}

		private async Task OnVoiceUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
		{
			if (newState.VoiceChannel != null)
			{
				await newState.VoiceChannel.DisconnectAsync();
			}
		}

		private double getCurrentTime()
		{
			return (DateTime.UtcNow - EPOCH).TotalMilliseconds;
		}

		private string getCodeMessage(double currentTime)
		{
			code = Convert.ToInt64(currentTime).ToString("X").Substring(0, CODE_LENGTH);
			return strings.AgreementPrompt + code;
		}

		private async Task UpdateMessage()
		{
			double currentTime = getCurrentTime();
			if (currentTime > refreshTime)
			{
				refreshTime = int.MaxValue;
				await codeMessage.ModifyAsync((messageProps) =>
				{
					messageProps.Content = getCodeMessage(currentTime);
				}, new RequestOptions()
				{
					RetryMode = RetryMode.RetryTimeouts | RetryMode.Retry502
				});
				refreshTime = currentTime + REFRESH_DELAY;
			}
		}
	}
}
