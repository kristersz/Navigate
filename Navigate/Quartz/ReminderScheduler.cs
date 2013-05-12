using Navigate.Models;
using Navigate.Models.Classifiers;
using Navigate.Quartz.Jobs;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;

namespace Navigate.Quartz
{
    public class ReminderScheduler
    {
        public IScheduler scheduler;

        public ReminderScheduler()
        {
            scheduler = MvcApplication.sched;
        }

        public void ScheduleReminder(WorkItem workItem)
        {
            var reminderDateTime = new DateTime();
            var dueDate = new DateTime();

            //determine the time for reminder
            if (workItem.WorkItemType == WorkItemType.Task)
            {
                reminderDateTime = workItem.EndDateTime;
                dueDate = workItem.EndDateTime;
            }
            else if (workItem.WorkItemType == WorkItemType.Appointment)
            {
                reminderDateTime = workItem.StartDateTime;
                dueDate = workItem.StartDateTime;
            }


            // construct job info
            IJobDetail jobDetail = JobBuilder.Create<SendReminderMailJob>()
                .WithIdentity("reminderFor" + workItem.Id.ToString(), "reminderJobs")
                .RequestRecovery()
                .Build();

            jobDetail.JobDataMap["Subject"] = workItem.Subject;
            jobDetail.JobDataMap["Body"] = workItem.Body;
            jobDetail.JobDataMap["Date"] = dueDate.ToString("dd.MM.yyyy HH:mm");

            // create a trigger that will trigger a job execution
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("triggerFor" + workItem.Id.ToString(), "reminderTriggers")
                .StartAt(DateBuilder.DateOf(reminderDateTime.Hour, reminderDateTime.Minute, reminderDateTime.Second))
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1))
                .Build();

            //associate the job with the trigger and schedule the job
            scheduler.ScheduleJob(jobDetail, trigger);
        }

        public static double GetDrivingDurationInMinutes(string origin, string destination)
        {
            var duration = 0;
            string url = @"http://maps.googleapis.com/maps/api/distancematrix/xml?origins=" +
              origin + "&destinations=" + destination +
              "&mode=driving&sensor=false&language=en-EN&units=metric";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader sreader = new StreamReader(dataStream);
            string responsereader = sreader.ReadToEnd();
            response.Close();

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(responsereader);

            if (xmldoc.GetElementsByTagName("status")[0].ChildNodes[0].InnerText == "OK")
            {
                XmlNodeList distance = xmldoc.GetElementsByTagName("duration");
                var durationString = distance[0].ChildNodes[0].ToString();
                duration = Convert.ToInt32(durationString);
            }

            return duration;
        }
    }
}