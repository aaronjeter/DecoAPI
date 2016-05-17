using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;

using DecoAPI.Models;

namespace DecoAPI.Employees
{
    public class Employee
    {
        /// <summary>
        /// Validate Employee by Badge Number
        /// </summary>
        /// <param name="badge"></param>
        /// <returns>Boolean</returns>
        public bool ValidateEmployee(string badge)
        {
            Database database = new Database();
            
            SqlCommand command = new SqlCommand();
            command.CommandText = "Select [hrme_first_name], [hrme_last_name] FROM [Employee].[dbo].[hrm_employee] where hrme_number = @badgeNumber";
            command.Parameters.AddWithValue("@badgeNumber", badge);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            bool retval = reader.HasRows;
            database.CloseConnection();
            return retval;
        }

        /// <summary>
        /// Check if a user has permission to reprint Paint/Mold Labels
        /// </summary>
        /// <param name="badge"></param>
        /// <returns>Boolean</returns>
        public bool CheckLabelReprintUserList(string badge)
        {
            Database database = new Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT Name FROM [PaintLabelReprint].[dbo].[Users] where badge = @badge";
            command.Parameters.AddWithValue("@badge", badge);

            SqlDataReader reader = database.RunCommandReturnReader(command);
            bool hasrows = reader.HasRows;
            database.CloseConnection();

            return hasrows;
        }

        /// <summary>
        /// Method to return Employee's name given badge number
        /// </summary>
        /// <param name="badge"></param>
        /// <returns>Employee name in format: "firstname lastname"</returns>
        public string GetEmployeeName(string badge)
        {
            string name = "";

            Database database = new Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "Select [hrme_first_name], [hrme_last_name] FROM [Employee].[dbo].[hrm_employee] where hrme_number = @badgeNumber";
            command.Parameters.AddWithValue("@badgeNumber", badge);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                name = reader.GetString(0) + " " + reader.GetString(1);
            }
            database.CloseConnection();

            return name;
        }

        public string GetEmployeeInitials(string badge)
        {
            string initials = "";

            Database database = new Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "Select [hrme_first_name], [hrme_last_name] FROM [Employee].[dbo].[hrm_employee] where hrme_number = @badgeNumber";
            command.Parameters.AddWithValue("@badgeNumber", badge);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                initials = reader.GetString(0).Substring(0, 1) + reader.GetString(1).Substring(0, 1);
            }
            database.CloseConnection();

            return initials;
        }
    }
}