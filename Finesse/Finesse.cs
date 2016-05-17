using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;

namespace DecoAPI.Finesse
{
    public class Finesse
    {
        public static List<DefectCategory> GetDefectCategories()
        {
            List<DefectCategory> categories = new List<DefectCategory>();

            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "Select * FROM Quality.dbo.defectCategories";

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                DefectCategory category = new DefectCategory();
                category.ID = Int32.Parse(reader["id"].ToString());
                category.Name = reader["name"].ToString();
                categories.Add(category);
            }

            database.CloseConnection();

            return categories;
        }

        public static List<DefectType> GetDefectTypes(int id)
        {
            List<DefectType> types = new List<DefectType>();

            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "Select * FROM Quality.dbo.defectTypes WHERE category = @id";
            command.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                DefectType type = new DefectType();
                type.ID = Int32.Parse(reader["id"].ToString());
                type.Category = Int32.Parse(reader["category"].ToString());
                type.Name = reader["name"].ToString();
                types.Add(type);
            }

            database.CloseConnection();

            return types;
        }
    }
}