using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DecoAPI.Models
{
    public class Paint
    {
        public Paint()
        {

        }

        /// <summary>
        /// Method to look up base part number by style code
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public string GetPartNumberByStyle(string style)
        {
            Database database = new Database();
            string partNumber = "";

            try
            {
                SqlCommand command = new SqlCommand();
                command.CommandText = "SELECT base_part_num FROM Production.dbo.styles_xref WHERE (active_flag > 0) AND (stylecode = @style)";
                command.Parameters.AddWithValue("@style", style);

                database.OpenConnection();
                SqlDataReader reader = database.RunCommandReturnReader(command);

                if(reader.Read())
                {
                    partNumber = reader["base_part_num"].ToString();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return partNumber;
        }

        public PaintExitID GetPaintExitIDByBarcode(string barcode)
        {
            PaintExitID objReturn = new PaintExitID();
            Database database = new Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = " SELECT * FROM  Production.dbo.pnt_exit_label, Production.dbo.styles_xref WHERE pnt_barcode = @Barcode and pnt_style = stylecode ";
            command.Parameters.AddWithValue("@Barcode", barcode);

            SqlDataReader reader = database.RunCommandReturnReader(command);
            
            if (reader.Read())
            {                
                objReturn.ID = Int64.Parse(reader["pnt_id"].ToString());
                objReturn.CreateDate = reader["pnt_create_datetime"].ToString();
                objReturn.barcode = reader["pnt_barcode"].ToString();
                objReturn.carrier = Int32.Parse(reader["pnt_carrier"].ToString());
                objReturn.CarrierPosition = Int32.Parse(reader["pnt_carrier_pos"].ToString());
                objReturn.Round = Int32.Parse(reader["pnt_round"].ToString());
                objReturn.Color = Int32.Parse(reader["pnt_color"].ToString());
                objReturn.Style = Int32.Parse(reader["pnt_style"].ToString());
                objReturn.Status = Int32.Parse(reader["pnt_status"].ToString());
                objReturn.ScanStatus = Int32.Parse(reader["pnt_scan_status"].ToString());
                objReturn.DispositionedBy = reader["dispositioned_by"].ToString();
                objReturn.sScanType = reader["scan_type"].ToString();

                string test = reader["pnt_return_status_change"].ToString();

                if (reader["pnt_status_id"].ToString() == string.Empty)
                {
                    objReturn.StatusID = 0;
                }
                else
                {
                    objReturn.StatusID = Int64.Parse(reader["pnt_status_id"].ToString());
                }

                if (reader["pnt_return_status_change"].ToString() == "")
                {
                    SqlCommand updateCommand = new SqlCommand();

                    updateCommand.CommandText = "update Production.dbo.pnt_exit_label set pnt_return_scan_status = 0, pnt_return_status_change = GetDate(), " +
                                                "pnt_return_status = 0, pnt_status = 1 " +
                                                "where pnt_barcode = @barcode";

                    command.Parameters.AddWithValue("@barcode", barcode);
                    database.RunCommand(updateCommand);
                    objReturn.ReturnStatusChange = DateTime.Now;
                    objReturn.ReturnScanStatus = 0;
                    objReturn.ReturnStatus = 0;                    
                }
                else
                {
                    objReturn.ReturnStatusChange = Convert.ToDateTime(reader["pnt_return_status_change"].ToString());
                    objReturn.ReturnStatus = Int32.Parse(reader["pnt_return_status"].ToString());
                    objReturn.ReturnScanStatus = Int32.Parse(reader["pnt_return_scan_status"].ToString());
                }                

                objReturn.XRefPartNumber = GetPartNumberXRefByObj(objReturn.Style);
                objReturn.OptionedPartNumber = GetOptionedPartNumber(objReturn.carrier, objReturn.CreateDate);

                // Close the DataReader
                reader.Close();

                // A part might have a blackout time associated with it.
                //  If it does, find it and record it in "BlackoutDate"
                try
                {
                    string query = " select    blackout_time "
                                 + " from      PaintRecords.dbo.RoofPanelBlackoutLog "
                                 + " where     paint_scan = '" + barcode + "'";
                    reader = database.RunCommandReturnReader(query);
                    objReturn.BlackoutDate = DateTime.Parse(reader["blackout_time"].ToString());
                }
                catch
                {
                    objReturn.BlackoutDate = null;
                }

                return objReturn;
            }

            database.CloseConnection();

            return objReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        private string GetPartNumberXRefByObj(int style)
        {
            Database database = new Database();

            string retval = "";

            string query = "SELECT id, stylecode, description, base_part_num, active_flag " +
                           "FROM  Production.dbo.styles_xref " +
                           "WHERE (active_flag > 0) AND (stylecode = '" + style + "')";

            SqlDataReader reader = database.RunCommandReturnReader(query);
            if (reader.Read())
            {
                retval = reader.GetString(3).Trim();                
            }

            database.CloseConnection();
            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sCarrier"></param>
        /// <param name="sCreateDate"></param>
        /// <returns></returns>
        private string GetOptionedPartNumber(int sCarrier, string sCreateDate)
        {
            // Create variables
            string returnValue = "";
            string query;
            DateTime carrierDate = new DateTime();
            DateTime lowTime;
            DateTime highTime;

            Database database = new Database();

            // Turn the Create Date into a Date object
            //  and add/subtract 5 minutes to form 2 objects
            carrierDate = DateTime.Parse(sCreateDate);
            lowTime = carrierDate.AddMinutes(-5);
            highTime = carrierDate.AddMinutes(5);

            // Create a SQL Query to get the part
            query =     " select	pt_part " +
                        " from	    PaintExitLabelApplication.dbo.CarrierTracking ct, Decostar.dbo.pt_mstr_sql pt " +
                        " where	    pt.pt_desc1 = ct.carrier_description " +
                        " and	    carrier_number = " + sCarrier +
                        " and	    recorded_time between '" + lowTime + "' and '" + highTime + "' " +
                        " and       pt.pt_part_type = 'PAINT' " +
                        " and       pt.pt_status = 'AC' ";
            
            SqlDataReader reader = database.RunCommandReturnReader(query);
            returnValue = reader["pt_part"].ToString();            

            return returnValue;
        }

        public static void CommitBlackout(PaintExitID oPaintLabel)
        {
            // Create Database Object
            Database database = new Database();
            // Open connection to Database
            //database.Open(DBType.SQL_PaintRecords);
                        

            // Make sure paint label has not been set as Blackout
            string query = " select    count(*) as pnt_exists "
                          + " from      PaintRecords.dbo.RoofPanelBlackoutLog "
                          + " where     paint_scan = '" + oPaintLabel.barcode + "'";

            SqlDataReader reader = database.RunCommandReturnReader(query);
            reader.Read();

            string exists = reader["pnt_exists"].ToString();
            // Close the DataReader for use below
            reader.Close();

            if (exists != "0")
                throw new Exception("Paint Label already scanned as Blackout");

            // Write Insert Statement
            string insert = " insert into RoofPanelBlackoutLog "
                           + " (paint_scan, blackout_time) "
                           + " values ('" + oPaintLabel.barcode + "', GetDate())";
            database.RunCommand(insert);
            // Close Connections
            reader.Close(); 
            database.CloseConnection();
        }
    }
}
