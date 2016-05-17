using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DecoAPI.Models;
using System.Data.SqlClient;

namespace DecoAPI.PartLookup
{
    public class Color
    {
        public long ID { get; set; }
        public string ColorCode { get; set; }
        public string Description { get; set; }
        public int PlantColor { get; set; }
        public string ShortDescription { get; set; }

        public string PaintDescription { get; set; }

        public Color(int plantcolor)
        {
            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT * FROM [Production].[dbo].[Colors] where plant_color = @plantcolor";
            command.Parameters.AddWithValue("@plantcolor", plantcolor);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                ID = reader.GetInt64(0);
                ColorCode = reader.GetString(1);
                Description = reader.GetString(2);
                PlantColor = Int32.Parse(reader["plant_color"].ToString()); //plant color is stored in the db as a bigint. with a maximum of 200-ish unique values. It'll overflow if they actually put in a number that won't fit.
                ShortDescription = reader.GetString(4);
            }
            database.CloseConnection();

            command = new SqlCommand();
            command.CommandText = "SELECT [Description] FROM [WWConfig].[dbo].[System Colors] where [Plant Number] = @plantnumber";
            command.Parameters.AddWithValue("@plantnumber", PlantColor);
            reader = database.RunCommandReturnReader(command, Database.DBType.DecoSQL);

            if (reader.Read())
            {
                PaintDescription = reader.GetString(0);
            }
        }

    }
}