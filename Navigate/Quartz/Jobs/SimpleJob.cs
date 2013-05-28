using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Navigate.Quartz.Jobs
{
    public class SimpleJob : IJob
    {
        public SimpleJob()
        { 

        }

        public void Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("I Executed at " + DateTime.Now.ToString());
        }
    }
}