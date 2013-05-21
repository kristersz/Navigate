using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace Navigate.Quartz.Jobs
{
    public class SendReminderMailJob : IJob
    {
        public SendReminderMailJob()
        {
 
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var smtpHost = System.Configuration.ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = System.Configuration.ConfigurationManager.AppSettings["SmtpPort"];
            var senderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"];
            var senderPassword = System.Configuration.ConfigurationManager.AppSettings["SenderPassword"];
            var port = 0;
            try
            {
                string subject = dataMap.GetString("Subject");
                string dueDate = dataMap.GetString("DueDate");
                string mailToAddress = dataMap.GetString("MailTo");
                port = Convert.ToInt32(smtpPort);

                SmtpClient smtp = new SmtpClient(smtpHost, port);
                smtp.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
                smtp.EnableSsl = true;

                MailMessage mail = new MailMessage();
                mail.To.Add(mailToAddress);
                mail.Subject = string.Format("Uzdevums {0} ir jāpabeidz līdz {1}", subject, dueDate);
                mail.Body = string.Format("Uzdevumu varat aplūkot nospiežot uz sekojošās saites: ");
                mail.BodyEncoding = Encoding.Unicode;
                mail.From = new MailAddress(senderEmail);

                smtp.Send(mail);
                Debug.WriteLine("Mail sent at " + DateTime.Now);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to send mail\nError message: " + ex);
            }
        }
    }
}