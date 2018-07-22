using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Bot
{
	public class WorkerRole : RoleEntryPoint
	{
		private readonly Bot bot = new Bot();
		private bool runComplete = false;

		public override bool OnStart()
		{
			// For information on handling configuration changes
			// see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

			bool result = base.OnStart();

			Trace.TraceInformation("Bot has been started");

			return result;
		}

		public override void Run()
		{
			try
			{
				bot.StartAsync().Wait();
			}
			finally
			{
				runComplete = true;
			}
		}

		public override void OnStop()
		{
			if (!runComplete)
				bot.StopAsync();
			runComplete = true;

			base.OnStop();

			Trace.TraceInformation("Bot has stopped");
		}
	}
}
