using Common.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Navigate.Quartz.Jobs
{
    public class HelloJob : IJob
    {

        public HelloJob()
        { 
        }

        public void Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("Hello at " + DateTime.Now.ToString());
        }
    }
}