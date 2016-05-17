using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DecoAPI.Models;
using System.Data.SqlClient;

namespace DecoAPI.PartLookup
{
    public class Style
    {
        public int ID { get; set; }
        public int StyleCode { get; set; }
        public string Description { get; set; }
        public string BasePartNumber { get; set; }
        public int Active { get; set; }
        public int Base { get; set; }
        public string ScanType { get; set; }
        public string PrintSide { get; set; }

        public Style(int stylecode)
        {
            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT * FROM [Production].[dbo].[styles_xref] where stylecode = @stylecode";
            command.Parameters.AddWithValue("@stylecode", stylecode);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                ID = reader.GetInt32(0);
                StyleCode = reader.GetInt32(1);
                Description = reader.GetString(2);
                BasePartNumber = reader.GetString(3);
                Active = reader.GetInt32(4);
                Base = reader.GetInt32(5);
                ScanType = reader.GetString(6);
                PrintSide = reader.GetString(7);
            }
        }
    }
}