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

        public long Id;

        public WorkItemType WorkItemType;

        public Reminder Reminder;

        public DateTime? StartTime;

        public DateTime EndTime;

        public string Origin;

        public string Location;

        public string Subject;

        public string MailTo;

        public string Url;

        /// <summary>
        /// Instantiate the scheduler in the constructor
        /// </summary>
        public ReminderScheduler()
        {
            scheduler = MvcApplication.sched;
        }

        /// <summary>
        /// Schedule jobs for each work item to send reminder emails
        /// </summary>
        /// <param name="workItem">The work item</param>
        public void ScheduleReminder()
        {
            var reminderDateTime = new DateTime();
            var dueDate = new DateTime();

            if (StartTime != null)
            {
                dueDate = StartTime.Value;
            }
            else
            {
                dueDate = EndTime;
            }

            reminderDateTime = GetReminderDateTime(Reminder, dueDate, Origin, Location);

            // construct job info
            IJobDetail jobDetail = JobBuilder.Create<SendReminderMailJob>()
                .WithIdentity("reminderFor" + Id.ToString(), "reminderJobs")
                .RequestRecovery()
                .Build();

            // add values to the JobDataMap for the job to use
            // such as the subject and due date of the work item, which is used when constructing the email
            jobDetail.JobDataMap["Subject"] = Subject;
            jobDetail.JobDataMap["DueDate"] = dueDate.ToString("dd.MM.yyyy HH:mm");
            jobDetail.JobDataMap["MailTo"] = MailTo;
            jobDetail.JobDataMap["Url"] = Url;

            // create a trigger that will trigger a job execution
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("triggerFor" + Id.ToString(), "reminderTriggers")
                .StartAt(DateBuilder.DateOf(reminderDateTime.Hour, reminderDateTime.Minute, reminderDateTime.Second, reminderDateTime.Day, reminderDateTime.Month, reminderDateTime.Year))
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1))
                .Build();

            //associate the job with the trigger and schedule the job
            scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void RescheduleReminder()
        {
            JobKey key = new JobKey("reminderFor" + Id.ToString(), "reminderJobs");
            scheduler.DeleteJob(key);
            ScheduleReminder();
        }

        public void RemoveReminder(long workItemId)
        {
            JobKey key = new JobKey("reminderFor" + workItemId.ToString(), "reminderJobs");
            scheduler.DeleteJob(key);
        }

        public DateTime GetReminderDateTime(Reminder reminder, DateTime dueDate, string origin, string location)
        {
            var reminderDateTime = new DateTime();
            // determine the reminder datetime, which is the time when the trigger will fire the job that sends and email to the user
            switch (reminder)
            {
                case Reminder.None:
                    break;
                case Reminder.Driving:
                    var drivingDuration = GetTravelDurationInMinutes(origin, location, "driving");
                    reminderDateTime = dueDate.AddMinutes(-drivingDuration);
                    break;
                case Reminder.Walking:
                    var walkingDuration = GetTravelDurationInMinutes(origin, location, "walking");
                    reminderDateTime = dueDate.AddMinutes(-walkingDuration);
                    break;
                case Reminder.FifteenMinutes:
                    reminderDateTime = dueDate.AddMinutes(-15);
                    break;
                case Reminder.ThirtyMinutes:
                    reminderDateTime = dueDate.AddMinutes(-30);
                    break;
                case Reminder.OneHour:
                    reminderDateTime = dueDate.AddHours(-1);
                    break;
                case Reminder.TwoHours:
                    reminderDateTime = dueDate.AddHours(-2);
                    break;
                case Reminder.OneDay:
                    reminderDateTime = dueDate.AddDays(-1);
                    break;
                case Reminder.TwoDays:
                    reminderDateTime = dueDate.AddDays(-2);
                    break;
                default:
                    break;
            }

            return reminderDateTime;
        }

        /// <summary>
        /// Calculates the travel duration using Google`s Distance Matrix service
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="travelMode">The travel mode</param>
        /// <returns>A double representing travel duration in minutes</returns>
        public static double GetTravelDurationInMinutes(string origin, string destination, string travelMode)
        {
            double duration = 0;

            // make a request to the distance matrix API
            string url = @"http://maps.googleapis.com/maps/api/distancematrix/xml?origins=" +
              origin + "&destinations=" + destination +
              "&mode=" + travelMode + "&sensor=false&language=en-EN&units=metric";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            //parse the response, wchich is an XML document
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader sreader = new StreamReader(dataStream);
            string responsereader = sreader.ReadToEnd();
            response.Close();

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(responsereader);

            // if everything went well and the service could calculate the duration of the travel
            // read the duration value, convert it to double and return to the user
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