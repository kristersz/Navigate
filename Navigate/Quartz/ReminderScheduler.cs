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

            switch (workItem.Reminder)
            {
                case Reminder.None:
                    break;
                case Reminder.Driving:
                    var drivingDuration = GetTravelDurationInMinutes(workItem.CreatedBy.BaseLocation, workItem.Location, "driving");
                    if (workItem.WorkItemType == WorkItemType.Task) reminderDateTime = workItem.EndDateTime.AddMinutes(-drivingDuration);
                    else reminderDateTime = workItem.StartDateTime.Value.AddMinutes(-drivingDuration);
                    break;
                case Reminder.Walking:
                    var walkingDuration = GetTravelDurationInMinutes(workItem.CreatedBy.BaseLocation, workItem.Location, "walking");
                    if (workItem.WorkItemType == WorkItemType.Task) reminderDateTime = workItem.EndDateTime.AddMinutes(-walkingDuration);
                    else reminderDateTime = workItem.StartDateTime.Value.AddMinutes(-walkingDuration);
                    break;
                case Reminder.FifteenMinutes:
                    if (workItem.WorkItemType == WorkItemType.Task) reminderDateTime = workItem.EndDateTime.AddMinutes(-15);
                    else reminderDateTime = workItem.StartDateTime.Value.AddMinutes(-15);
                    break;
                case Reminder.ThirtyMinutes:
                    if (workItem.WorkItemType == WorkItemType.Task) reminderDateTime = workItem.EndDateTime.AddMinutes(-30);
                    else reminderDateTime = workItem.StartDateTime.Value.AddMinutes(-30);
                    break;
                case Reminder.OneHour:
                    if (workItem.WorkItemType == WorkItemType.Task) reminderDateTime = workItem.EndDateTime.AddHours(-1);
                    else reminderDateTime = workItem.StartDateTime.Value.AddHours(-1);
                    break;
                case Reminder.TwoHours:
                    if (workItem.WorkItemType == WorkItemType.Task) reminderDateTime = workItem.EndDateTime.AddHours(-2);
                    else reminderDateTime = workItem.StartDateTime.Value.AddHours(-2);
                    break;
                case Reminder.OneDay:
                    if (workItem.WorkItemType == WorkItemType.Task) reminderDateTime = workItem.EndDateTime.AddDays(-1);
                    else reminderDateTime = workItem.StartDateTime.Value.AddDays(-1);
                    break;
                case Reminder.TwoDays:
                    if (workItem.WorkItemType == WorkItemType.Task) reminderDateTime = workItem.EndDateTime.AddDays(-2);
                    else reminderDateTime = workItem.StartDateTime.Value.AddDays(-2);
                    break;
                default:
                    break;
            }

            if (workItem.WorkItemType == WorkItemType.Task) dueDate = workItem.EndDateTime;
            else dueDate = workItem.StartDateTime.Value;

            // construct job info
            IJobDetail jobDetail = JobBuilder.Create<SendReminderMailJob>()
                .WithIdentity("reminderFor" + workItem.Id.ToString(), "reminderJobs")
                .RequestRecovery()
                .Build();

            jobDetail.JobDataMap["Subject"] = workItem.Subject;
            jobDetail.JobDataMap["DueDate"] = dueDate.ToString("dd.MM.yyyy HH:mm");
            jobDetail.JobDataMap["MailTo"] = workItem.CreatedBy.Email;

            // create a trigger that will trigger a job execution
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("triggerFor" + workItem.Id.ToString(), "reminderTriggers")
                .StartAt(DateBuilder.DateOf(reminderDateTime.Hour, reminderDateTime.Minute, reminderDateTime.Second, reminderDateTime.Day, reminderDateTime.Month, reminderDateTime.Year))
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1))
                .Build();

            //associate the job with the trigger and schedule the job
            scheduler.ScheduleJob(jobDetail, trigger);
        }

        public static double GetTravelDurationInMinutes(string origin, string destination, string mode)
        {
            double duration = 0;
            string url = @"http://maps.googleapis.com/maps/api/distancematrix/xml?origins=" +
              origin + "&destinations=" + destination +
              "&mode=" + mode + "&sensor=false&language=en-EN&units=metric";

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
                XmlNodeList durationNode = xmldoc.GetElementsByTagName("duration");
                var durationString = durationNode[0].ChildNodes[0].InnerText;
                duration = Convert.ToDouble(durationString);
            }

            return duration / 60;
        }
    }
}