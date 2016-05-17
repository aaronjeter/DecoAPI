using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DecoAPI.Models;
using System.Data.SqlClient;

namespace DecoAPI.PartLookup
{
    public class PaintLabel
    {
        public long ID { get; set; }
        public DateTime Time { get; set; }
        public string Barcode { get; set; }
        public string Carrier { get; set; }
        public int Position { get; set; }
        public int Round { get; set; }
        public int Color { get; set; }
        public int Style { get; set; }
        public int ScanStatus { get; set; }

        public PaintLabel(string barcode)
        {
            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT * FROM [Production].[dbo].[pnt_exit_label] where pnt_barcode = @barcode";
            command.Parameters.AddWithValue("@barcode", barcode);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                ID = reader.GetInt64(0);
                Time = reader.GetDateTime(1);
                Barcode = reader.GetString(2);
                Carrier = reader.GetString(3);
                Position = reader.GetInt32(4);
                Round = reader.GetInt32(5);
                Color = reader.GetInt32(6);
                Style = reader.GetInt32(7);
                //we don't need pnt_status (8)
                //we don't need pnt_status_id (9)
                ScanStatus = reader.GetInt32(10);
            }
        }
    }
}