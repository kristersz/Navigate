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
            Debug.WriteLine("shit started executing");
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            try
            {
                var smtpHost = System.Configuration.ConfigurationManager.AppSettings["SmtpHost"];
                var smtpPort = System.Configuration.ConfigurationManager.AppSettings["SmtpPort"];
                var senderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"];
                var port = 0;

                string subject = dataMap.GetString("Subject");
                string body = dataMap.GetString("Body");
                string dueDate = dataMap.GetString("Date");

                try
                {
                    port = Convert.ToInt32(smtpPort);
                }
                catch (FormatException ex)
                {
                    Debug.WriteLine("Input string is not a sequence of digits\nError message: " + ex);
                }

                SmtpClient smtp = new SmtpClient(smtpHost, port);
                smtp.Credentials = new System.Net.NetworkCredential(senderEmail, "dubultaisz18");
                smtp.EnableSsl = true;

                MailMessage mail = new MailMessage();
                mail.To.Add("kristers.zimecs@gmail.com");
                mail.Subject = string.Format("Uzdevums {0} ir jāpabeidz līdz {1}", subject, dueDate);
                mail.Body = string.Format("{0}", subject);
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