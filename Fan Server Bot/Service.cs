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

        public Service()
        {
            mainEventLog = new EventLog();
            if (!EventLog.SourceExists(EVENT_SOURCE))
                EventLog.CreateEventSource(EVENT_SOURCE, EVENT_LOG);
            mainEventLog.Source = EVENT_SOURCE;
            mainEventLog.Log = EVENT_LOG;

            ServiceName = "CJ Fan Server Bot";
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
