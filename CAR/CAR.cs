using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;

namespace DecoAPI.CAR
{
    public class CAR
    {
        #region Getters/Setters

        public int ID { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? DateOpened { get; set; }
        public DateTime? IncidentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? LastUpdated { get; set; }

        public string Customer { get; set; }
        public string Program { get; set; }
        public string CarStatus { get; set; }
        public string Department { get; set; }
        public string RevisionNumber { get; set; }
        public string TypeOfIssue { get; set; }
        public string PartDescription { get; set; }
        public string PartStyle { get; set; }
        public string PartColor { get; set; }
        public string PartNumber { get; set; }
        public string CustomerComplaintNumber { get; set; }
        public string CustomerSymptoms { get; set; }

        public string AssignedTo { get; set; }

        public string Progress { get; set; }

        public string EmergencyResponse { get; set; }
        public string EmergencyResponseDate { get; set; }
        public string EmergencyResponsePercent { get; set; }
        public string TeamLeader { get; set; }
        public string TeamLeaderDepartment { get; set; }
        public string TeamMember1 { get; set; }
        public string TeamMember1Department { get; set; }
        public string TeamMember2 { get; set; }
        public string TeamMember2Department { get; set; }
        public string TeamMember3 { get; set; }
        public string TeamMember3Department { get; set; }

        public string ProblemDescription { get; set; }
        public string ContainmentAction { get; set; }
        public string ContainmentActionDate { get; set; }
        public string ContainmentActionPercent { get; set; }

        public string RootCause1 { get; set; }
        public string RootCause1Percent { get; set; }
        public string RootCause2 { get; set; }
        public string RootCause2Percent { get; set; }
        public string RootCause3 { get; set; }
        public string RootCause3Percent { get; set; }

        public string CorrectiveAction1 { get; set; }
        public string CorrectiveAction1Percent { get; set; }
        public string CorrectiveAction2 { get; set; }
        public string CorrectiveAction2Percent { get; set; }
        public string CorrectiveAction3 { get; set; }
        public string CorrectiveAction3Percent { get; set; }

        public string PCA1 { get; set; }
        public string PCA1Date { get; set; }
        public string PCA2 { get; set; }
        public string PCA2Date { get; set; }
        public string PCA3 { get; set; }
        public string PCA3Date { get; set; }

        public string SPA1 { get; set; }
        public string SPA1Primary { get; set; }
        public string SPA1Secondary { get; set; }
        public string SPA1Date { get; set; }
        public string SPA2 { get; set; }
        public string SPA2Primary { get; set; }
        public string SPA2Secondary { get; set; }
        public string SPA2Date { get; set; }
        public string SPA3 { get; set; }
        public string SPA3Primary { get; set; }
        public string SPA3Secondary { get; set; }
        public string SPA3Date { get; set; }

        public string D3ValidationName { get; set; }
        public string D3ValidationDate { get; set; }
        public string D3ValidationSupport { get; set; }
        public string D4ValidationName { get; set; }
        public string D4ValidationDate { get; set; }
        public string D4ValidationSupport { get; set; }
        public string D5ValidationName { get; set; }
        public string D5ValidationDate { get; set; }
        public string D5ValidationSupport { get; set; }
        public string D6ValidationName { get; set; }
        public string D6ValidationDate { get; set; }
        public string D6ValidationSupport { get; set; }

        public string TeamRecognition1 { get; set; }
        public string TeamRecognition1Date { get; set; }
        public string TeamRecognition1Report { get; set; }
        public string TeamRecognition2 { get; set; }
        public string TeamRecognition2Date { get; set; }
        public string TeamRecognition2Report { get; set; }
        public string TeamRecognition3 { get; set; }
        public string TeamRecognition3Date { get; set; }
        public string TeamRecognition3Report { get; set; }

        public DateTime? DateClosed { get; set; }

        public string WaitingOn { get; set; }

        public string Notes { get; set; }
        #endregion

        /// <summary>
        /// Basic method to get CAR object with all fields. There will likely be other, more specified constructors for specific uses
        /// </summary>
        /// <param name="id"></param>
        public CAR(int id)
        {
            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "Select * From Quality.dbo.new_cars WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                database.SetReader(reader);

                #region Read Data

                ID = Int32.Parse(reader["id"].ToString());

                CreatedBy = database.SafeGetString("created_by");

                DateOpened = database.SafeGetDate("date_opened");
                IncidentDate = database.SafeGetDate("incident_date");
                DueDate = database.SafeGetDate("due_date");
                LastUpdated = database.SafeGetDate("last_updated");

                Customer = database.SafeGetString("customer");
                Program = database.SafeGetString("program");
                CarStatus = database.SafeGetString("car_status");
                Department = database.SafeGetString("department");
                RevisionNumber = database.SafeGetString("revision_number");
                TypeOfIssue = database.SafeGetString("type_of_issue");
                PartDescription = database.SafeGetString("part_description");
                PartStyle = database.SafeGetString("part_style");
                PartColor = database.SafeGetString("part_color");
                PartNumber = database.SafeGetString("part_number");
                CustomerComplaintNumber = database.SafeGetString("customer_complaint_number");
                CustomerSymptoms = database.SafeGetString("customer_symptoms");

                AssignedTo = database.SafeGetString("assigned_to");
                Progress = database.SafeGetString("progress");
                EmergencyResponse = database.SafeGetString("emergency_response");
                EmergencyResponseDate = database.SafeGetString("emergency_response_date");
                EmergencyResponsePercent = database.SafeGetString("emergency_response_percent");
                TeamLeader = database.SafeGetString("team_leader");
                TeamLeaderDepartment = database.SafeGetString("team_leader_department");
                TeamMember1 = database.SafeGetString("team_member_1");
                TeamMember1Department = database.SafeGetString("team_member_1_department");
                TeamMember2 = database.SafeGetString("team_member_2");
                TeamMember2Department = database.SafeGetString("team_member_2_department");
                TeamMember3 = database.SafeGetString("team_member_3");
                TeamMember3Department = database.SafeGetString("team_member_3_department");

                ProblemDescription = database.SafeGetString("problem_description");
                ContainmentAction = database.SafeGetString("containment_action");
                ContainmentActionDate = database.SafeGetString("containment_action_date");
                ContainmentActionPercent = database.SafeGetString("containment_action_percent");

                RootCause1 = database.SafeGetString("root_cause_1");
                RootCause1Percent = database.SafeGetString("root_cause_1_percent");
                RootCause2 = database.SafeGetString("root_cause_2");
                RootCause2Percent = database.SafeGetString("root_cause_2_percent");
                RootCause3 = database.SafeGetString("root_cause_3");
                RootCause3Percent = database.SafeGetString("root_cause_3_percent");

                CorrectiveAction1 = database.SafeGetString("corrective_action_1");
                CorrectiveAction1Percent = database.SafeGetString("corrective_action_1_percent");
                CorrectiveAction2 = database.SafeGetString("corrective_action_2");
                CorrectiveAction2Percent = database.SafeGetString("corrective_action_2_percent");
                CorrectiveAction3 = database.SafeGetString("corrective_action_3");
                CorrectiveAction3Percent = database.SafeGetString("corrective_action_3_percent");

                PCA1 = database.SafeGetString("pca_1");
                PCA1Date = database.SafeGetString("pca_1_date");
                PCA2 = database.SafeGetString("pca_2");
                PCA2Date = database.SafeGetString("pca_2_date");
                PCA3 = database.SafeGetString("pca_3");
                PCA3Date = database.SafeGetString("pca_3_date");

                SPA1 = database.SafeGetString("spa_1");
                SPA1Primary = database.SafeGetString("spa_1_primary");
                SPA1Secondary = database.SafeGetString("spa_1_secondary");
                SPA1Date = database.SafeGetString("spa_1_date");

                SPA2 = database.SafeGetString("spa_2");
                SPA2Primary = database.SafeGetString("spa_2_primary");
                SPA2Secondary = database.SafeGetString("spa_2_secondary");
                SPA2Date = database.SafeGetString("spa_2_date");

                SPA3 = database.SafeGetString("spa_3");
                SPA3Primary = database.SafeGetString("spa_3_primary");
                SPA3Secondary = database.SafeGetString("spa_3_secondary");
                SPA3Date = database.SafeGetString("spa_3_date");

                D3ValidationDate = database.SafeGetString("d3_validation_name");
                D3ValidationDate = database.SafeGetString("d3_validation_date");
                D3ValidationSupport = database.SafeGetString("d3_validation_support");

                D4ValidationDate = database.SafeGetString("d4_validation_name");
                D4ValidationDate = database.SafeGetString("d4_validation_date");
                D4ValidationSupport = database.SafeGetString("d4_validation_support");

                D5ValidationDate = database.SafeGetString("d5_validation_name");
                D5ValidationDate = database.SafeGetString("d5_validation_date");
                D5ValidationSupport = database.SafeGetString("d5_validation_support");

                D6ValidationDate = database.SafeGetString("d6_validation_name");
                D6ValidationDate = database.SafeGetString("d6_validation_date");
                D6ValidationSupport = database.SafeGetString("d6_validation_support");

                TeamRecognition1 = database.SafeGetString("team_recognition_1");
                TeamRecognition1Date = database.SafeGetString("team_recognition_1_date");
                TeamRecognition1Report = database.SafeGetString("team_recognition_1_report");

                TeamRecognition2 = database.SafeGetString("team_recognition_2");
                TeamRecognition2Date = database.SafeGetString("team_recognition_2_date");
                TeamRecognition2Report = database.SafeGetString("team_recognition_2_report");

                TeamRecognition3 = database.SafeGetString("team_recognition_3");
                TeamRecognition3Date = database.SafeGetString("team_recognition_3_date");
                TeamRecognition3Report = database.SafeGetString("team_recognition_3_report");

                DateClosed = database.SafeGetDate("date_closed");

                WaitingOn = database.SafeGetString("waiting_on");

                Notes = database.SafeGetString("notes");

                #endregion
            }

            database.CloseConnection();
        }

        public static void CreateCar(CAR car)
        {
            Models.Database database = new Models.Database();

            #region Sql Command Setup
            SqlCommand command = new SqlCommand();
            command.CommandText = "INSERT INTO [Quality.dbo.new_cars] (created_by, date_opened, incident_date, due_date, last_updated, customer, program, car_status, department, revision_number, customer_complaint_number, type_of_issue, part_description, part_style, part_color, part_number, customer_symptoms, assigned_to, progress, waiting_on) "
                                + "VALUES "
                                + "(@created_by, @date_opened, @incident_date, @due_date, @last_updated, @customer, @program, @car_status, @department, @revision_number, @customer_complaint_number, @type_of_issue, @part_description, @part_style, @part_color, @part_number, @customer_symptoms, @assigned_to, @progress, @waiting_on)";

            command.Parameters.AddWithValue("@created_by", car.CreatedBy);
            command.Parameters.AddWithValue("@date_opened", car.DateOpened);
            command.Parameters.AddWithValue("@incident_date", car.IncidentDate);
            command.Parameters.AddWithValue("@due_date", car.DueDate);
            command.Parameters.AddWithValue("@last_updated", car.LastUpdated);

            command.Parameters.AddWithValue("@customer", car.Customer);
            command.Parameters.AddWithValue("@program", car.Program);

            command.Parameters.AddWithValue("@car_status", car.CarStatus);
            command.Parameters.AddWithValue("@department", car.Department);
            command.Parameters.AddWithValue("@revision_number", 1);

            command.Parameters.AddWithValue("@customer_complaint_number", car.CustomerComplaintNumber);
            command.Parameters.AddWithValue("@type_of_issue", car.TypeOfIssue);
            command.Parameters.AddWithValue("@part_description", car.PartDescription);
            command.Parameters.AddWithValue("@part_style", car.PartStyle);
            command.Parameters.AddWithValue("@part_color", car.PartColor);
            command.Parameters.AddWithValue("@part_number", car.PartNumber);
            command.Parameters.AddWithValue("@customer_symptoms", car.CustomerSymptoms);

            command.Parameters.AddWithValue("@assigned_to", car.AssignedTo);
            command.Parameters.AddWithValue("@progress", car.Progress);
            command.Parameters.AddWithValue("@waiting_on", car.WaitingOn);
            #endregion
            database.RunCommand(command);
        }

        public static void UpdateCar(CAR car)
        {
            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "UPDATE Quality.dbo.new_cars"
                                + "WHERE id = @id";

            command.Parameters.AddWithValue("@id", car.ID);
        }

        public string GetDepartmentManager()
        {
            string departmentManager = null;

            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT [email] FROM Quality.dbo.car_users WHERE [department] = @department AND [role] = 'dm'";
            command.Parameters.AddWithValue("@department", Department);

            SqlDataReader reader = database.RunCommandReturnReader(command);
            database.SetReader(reader);

            if (reader.Read())
            {
                departmentManager = database.SafeGetString("email");
            }

            database.CloseConnection();

            return departmentManager;
        }

        public List<string> GetCheckupList()
        {
            List<string> checkupList = new List<string>();

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT [email] FROM Quality.dbo.car_users WHERE checkup_list = 1";

            SqlDataReader reader = database.RunCommandReturnReader(command);
            database.SetReader(reader);

            while (reader.Read())
            {
                checkupList.Add(database.SafeGetString("email"));
            }
            database.CloseConnection();

            return checkupList;
        }

        public List<string> GetGMList()
        {
            List<string> gmList = new List<string>();

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT [email] FROM Quality.dbo.car_users WHERE [role] = 'gm'";

            SqlDataReader reader = database.RunCommandReturnReader(command);
            database.SetReader(reader);

            while (reader.Read())
            {
                gmList.Add(database.SafeGetString("email"));
            }
            database.CloseConnection();

            return gmList;
        }

        public int DaysUntilDue()
        {
            int days = 0;

            days = ((DateTime) DueDate - DateTime.Today).Days;

            return days;
        }

        /// <summary>
        /// Return number of days CAR has been overdue
        /// </summary>
        /// <returns></returns>
        public int DaysOverdue()
        {
            int days = 0;

            days = (DateTime.Today - (DateTime) DueDate).Days;

            return days;
        }

        /// <summary>
        /// Return number of days since CAR was marked as complete
        /// </summary>
        /// <returns></returns>
        public int DaysSinceFinished()
        {
            int days = 0;
            days = (DateTime.Today - (DateTime)DateClosed).Days;
            return days;
        }
    }
}