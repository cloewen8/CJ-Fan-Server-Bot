using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Fan_Server_Bot
{
	public partial class Service : ServiceBase
	{
		private const string EVENT_SOURCE = "Fan Server";
		private const string EVENT_LOG = "Chat Bot";
		private EventLog mainEventLog;
		private Bot bot;
		private Task starting;

		public Service()
		{
			mainEventLog = new EventLog();
			if (!EventLog.SourceExists(EVENT_SOURCE))
				EventLog.CreateEventSource(EVENT_SOURCE, EVENT_LOG);
			mainEventLog.Source = EVENT_SOURCE;
			mainEventLog.Log = EVENT_LOG;
			Bot.MainEventLog = mainEventLog;

			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			bot = new Bot();
			starting = bot.StartAsync(this);
			starting.ContinueWith((task) =>
			{
				if (task.Exception != null)
				{
					Exception exc = task.Exception;
					while (exc.InnerException != null)
						exc = exc.InnerException;
					StringBuilder excBuilder = new StringBuilder();
					excBuilder.Append("An unexpected exception occured: ");
					excBuilder.Append(exc.Message);
					excBuilder.AppendLine();
					excBuilder.AppendLine();
					excBuilder.Append(exc.StackTrace);
					mainEventLog.WriteEntry(excBuilder.ToString(), EventLogEntryType.Error);
					Stop();
				}
			});
		}

		protected override void OnStop()
		{
			starting.ContinueWith(task => bot.StopAsync());
		}

		private void InitializeComponent()
		{
			// 
			// Service
			// 
			this.ServiceName = "CJ Fan Server Bot";

		}
	}
}
