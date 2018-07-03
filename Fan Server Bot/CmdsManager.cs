using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fan_Server_Bot
{
    class CmdsManager
    {
        private List<ICmd> cmds = new List<ICmd>();
        private BotConfig config = BotConfig.Instance;

        internal CmdsManager()
        {
            RegisterCmd(new HelpCmd.Cmd());
        }

        public async Task OnMessage(SocketMessage message)
        {
            if (message.Content.StartsWith(config.CmdPrefix))
            {
                try
                {
                    bool isOwner = false;
                    if (message.Author is IGuildUser guildUser)
                    {
                        isOwner = guildUser.RoleIds.Any((role) => role == config.OwnerRoleId);
                    }
                    ICmd found = (from cmd in cmds
                                  where (isOwner || !cmd.OwnerOnly) &&
                                      cmd.Pattern.IsMatch(message.Content, 1)
                                  select cmd).FirstOrDefault();
                    if (found != null)
                    {
                        CancellationTokenSource cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(config.CmdTimeout));
                        Task executeTask = found.Execute(message,
                            found.Pattern.Match(message.Content, 1).Captures,
                            cancelSource.Token);
                        await executeTask;
                        cancelSource.Dispose();
                        if (cancelSource.IsCancellationRequested)
                        {
                            throw new TimeoutException();
                        }
                        else if (executeTask.Exception != null)
                        {
                            Exception exc = executeTask.Exception;
                            while (exc.InnerException != null)
                                exc = exc.InnerException;
                            throw exc;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    Bot.MainEventLog.WriteEntry("A command ran out of time to respond.", EventLogEntryType.Warning);
                    await message.Channel.SendMessageAsync(
                        "The " + config.Name + " ran out of time to respond! Try again shortly.");
                }
                catch (Exception exc)
                {
                    StringBuilder excBuilder = new StringBuilder();
                    excBuilder.Append("A command failed to execute: ");
                    excBuilder.Append(exc.Message);
                    excBuilder.AppendLine();
                    excBuilder.AppendLine();
                    excBuilder.Append(exc.StackTrace);
                    Bot.MainEventLog.WriteEntry(excBuilder.ToString(), EventLogEntryType.Error);
                    await message.Channel.SendMessageAsync("A command failed to execute. Please contact the server owner.");
                }
            }
        }

        private void RegisterCmd(ICmd cmd)
        {
            cmds.Add(cmd);
        }
    }
}
