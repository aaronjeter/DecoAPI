using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;


namespace DecoAPI.CAR
{
    public class User
    {
        public string Name { get; set; }

        public string Role { get; set; }

        public string Email { get; set; }

        public string Department { get; set; }

        public User(string username)
        {
            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT Email, Name, Role, Department FROM Quality.dbo.car_users WHERE username = @username";
            command.Parameters.AddWithValue("@username", username);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                Email = reader["Email"].ToString();
                Name = reader["Name"].ToString();
                Role = reader["Role"].ToString();
                Department = reader["Department"].ToString();
            }
            database.CloseConnection();
        }
    }
}