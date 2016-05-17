using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;

namespace DecoAPI.CAR
{
    public class CarAlert
    {
        List<CAR> cars = new List<CAR>();

        List<Email.Email> emails = new List<Email.Email>();

        public void SendCARAlerts()
        {
            GetData();

            foreach (CAR car in cars)
            {
                emails.AddRange(Triage(car));
            }

            BundleEmails();
        }

        public void BundleEmails()
        {
            Dictionary<string, List<string>> emailList = new Dictionary<string, List<string>>();

            foreach (Email.Email mail in emails)
            {
                if (emailList.ContainsKey(mail.Address))
                {
                    emailList[mail.Address].Add(mail.Message);
                }
                else
                {
                    List<string> messageList = new List<string>();
                    messageList.Add(mail.Message);
                    emailList.Add(mail.Address, messageList);
                }
            }

            //Send Bundled Emails
            foreach (KeyValuePair<string, List<string>> e in emailList)
            {
                string address = e.Key;
                List<string> contents = e.Value;

                string message = "http://decoweb01/car";
                message += "\n\n";

                foreach (string s in contents)
                {
                    message += s + "\n";
                }

                Email.Email email = new Email.Email(address, message, "CAR Alert");
                email.SendMail();
            }
        }        

        public void GetData()
        {
            //List<string> car_ids = new List<string>();

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT id FROM Quality.dbo.new_cars where due_date IS NOT NULL";

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                string id = reader["id"].ToString();
                int idNum = Int32.Parse(id);
                cars.Add(new CAR(idNum));
            }
            database.CloseConnection();
        }

        public List<Email.Email> Triage(CAR car)
        {
            List<Email.Email> messages = new List<Email.Email>();

            if (car.CarStatus == "closed")
            {
                int daysSinceFinished = car.DaysSinceFinished();

                if (daysSinceFinished == 30)
                {
                    CheckupAlert(car);
                }
            }
            else
            {
                int days_remaining = car.DaysUntilDue();

                if (days_remaining > 3)
                {
                    //more than three days remain, no action required
                }
                else if (days_remaining > 0)
                {
                    messages.AddRange(NearDueAlerts(car));
                }
                else
                {
                    messages.AddRange(OverDueAlerts(car));

                    int days_overdue = car.DaysOverdue();

                    if (days_overdue >= 5)
                    {
                        messages.AddRange(GMAlerts(car));
                    }
                }
            }

            return messages;
        }

        public List<Email.Email> CheckupAlert(CAR car)
        {
            List<Email.Email> alerts = new List<Email.Email>();

            List<string> checkupList = car.GetCheckupList();

            string message = "CAR Request # " + car.ID + " has been closed for 30 days, and is ready for follow up";

            alerts.Add(new Email.Email(car.CreatedBy, message, "CAR Alert"));

            foreach (string s in checkupList)
            {
                alerts.Add(new Email.Email(s, message, "CAR Alert"));
            }

            return alerts;
        }

        public List<Email.Email> OverDueAlerts(CAR car)
        {
            List<Email.Email> alerts = new List<Email.Email>();
            string message;

            //Send to Manufacturing Engineer
            //But only if there's work for the ME to do. Otherwise we're just wasting his time.
            if (car.Progress == "assigned")
            {
                message = "\nCAR ID#: " + car.ID + ", Assigned To: " + car.AssignedTo + " was due by " + ((DateTime)car.DueDate).ToShortDateString() + " and is now overdue";
                alerts.Add(new Email.Email(car.AssignedTo, message, "CAR Alert"));
            }

            //Send to Quality Engineer
            //If there's work for the QE to do
            if (car.Progress == "dm_approved")
            {
                message = "\nCAR ID#: " + car.ID + ", Assigned To: " + car.AssignedTo + " was due by " + ((DateTime)car.DueDate).ToShortDateString() + " and is now overdue";
                alerts.Add(new Email.Email(car.CreatedBy, message, "CAR Alert"));
            }

            //Send to Department Manager
            message = "\nCAR ID#: " + car.ID + ", Assigned To: " + car.AssignedTo + " was due by " + ((DateTime)car.DueDate).ToShortDateString() + " and is now overdue";
            alerts.Add(new Email.Email(car.GetDepartmentManager(), message, "CAR Alert"));

            return alerts;
        }

        public List<Email.Email> NearDueAlerts(CAR car)
        {
            List<Email.Email> alerts = new List<Email.Email>();

            string message = "\nThis is an automated reminder that ";
            message += "CAR ID#: " + car.ID + " is due on " + ((DateTime)car.DueDate).ToShortDateString();

            if (car.Progress == "assigned")
            {
                alerts.Add(new Email.Email(car.AssignedTo, message, "CAR Alert"));
            }
            else
            {
                alerts.Add(new Email.Email(car.GetDepartmentManager(), message, "CAR Alert"));
            }

            return alerts;
        }

        public List<Email.Email> GMAlerts(CAR car)
        {
            List<Email.Email> alerts = new List<Email.Email>();

            List<string> gmList = car.GetGMList();

            string message = "\nCAR ID #" + car.ID + " Was due on: " + ((DateTime)car.DueDate).ToShortDateString() + " and has been overdue for " + car.DaysOverdue() + " days.";
            message += "\n Department: " + car.Department;
            message += "\n Waiting On: " + car.WaitingOn;
            message += "\n Quality Engineer: " + car.CreatedBy;
            message += "\n Manufacturing Engineer: " + car.AssignedTo;

            foreach (string  s in gmList)
            {
                alerts.Add(new Email.Email(s, message, "CAR Alert"));
            }

            return alerts;
        }


    }
}