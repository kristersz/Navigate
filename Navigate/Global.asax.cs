using MvcHtmlHelpers;
using Navigate.Models.Classifiers;
using Navigate.Quartz.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMatrix.WebData;

namespace Navigate
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            if (!WebSecurity.Initialized)
            {
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfiles", "UserId", "UserName", autoCreateTables: true);
            }
            ModelBinders.Binders.Add(typeof(DaysOfWeek), new EnumFlagsModelBinder());
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            // configure and start the Quartz.NET scheduler
            sched = ConfigureQuartzJobs();
        }

        public static IScheduler ConfigureQuartzJobs()
        {
            var properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = "ServerScheduler";

            // set thread pool info
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "5";
            properties["quartz.threadPool.threadPriority"] = "Normal";

            // set remoting expoter
            properties["quartz.scheduler.proxy"] = "true";
            properties["quartz.scheduler.proxy.address"] = "tcp://localhost:555/QuartzScheduler";
            // construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory(properties);

            // get a scheduler
            IScheduler sched = schedFact.GetScheduler();
            sched.Start();

            IJobDetail jobDetail = JobBuilder.Create<SimpleJob>()
                .WithIdentity("simpleJob", "simpleJobs")
                .RequestRecovery()
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("simpleTrigger", "simpleTriggers")
                .StartNow()
                .WithSimpleSchedule(x => x.WithRepeatCount(4).WithIntervalInSeconds(10))
                .Build();

            sched.ScheduleJob(jobDetail, trigger);

            return sched;
        }

        public static IScheduler sched;
    }
}