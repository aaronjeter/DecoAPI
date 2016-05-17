using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

using System.Data.SqlClient;
using DecoAPI.Models;
using DecoAPI.Printers;

namespace DecoAPI.Unload
{
    public class RackCreator
    {
        string scan_type;
        string style;
        int status;

        sPart part;

        Session session;

        public RackCreator(Session session)
        {
            this.session = session;
        }

        public static string GetScanType(string paintLabel)
        {
            string scanType = "";

            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT styles.scan_type FROM [Production].[dbo].[styles_xref] as styles "
                                + "Join [Production].[dbo].[pnt_exit_label] as paint_exit on paint_exit.pnt_style = styles.stylecode where paint_exit.pnt_barcode = @barcode";
            command.Parameters.AddWithValue("@barcode", paintLabel);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.HasRows == false)
            {
                scanType = "Invalid Paint Label";
            }
            else
            {
                reader.Read();
                scanType = reader["scan_type"].ToString();
            }

            database.CloseConnection();
            return scanType;
        }

        /// <summary>
        /// Method to process a session object.
        /// </summary> 
        /// <returns></returns>
        public Session ProcessSession()
        {
            #region Pull Data From Database by Paintlabel
            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT styles.scan_type, styles.stylecode, paint_exit.pnt_scan_status FROM [Production].[dbo].[styles_xref] as styles "
                                + "Join [Production].[dbo].[pnt_exit_label] as paint_exit on paint_exit.pnt_style = styles.stylecode where paint_exit.pnt_barcode = @barcode";            
            command.Parameters.AddWithValue("@barcode", session.PaintLabel);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            //If query returns no rows, paint barcode is invalid
            if (reader.HasRows == false)
            {
                session.PaintLabel = "";
                database.CloseConnection();
                throw new Exception("Invalid paint Label");
            }

            if (reader.Read())
            {
                scan_type = reader["scan_type"].ToString();
                style = reader["stylecode"].ToString(); session.Style = style; //Add style to session for later
                status = Int32.Parse(reader["pnt_scan_status"].ToString());

                //if scan status not 0, 1, or 2, throw invalid part exception
                if (status != 0 && status != 1 && status != 2 && status != 7)
                {
                    database.CloseConnection();
                    throw new Exception("This application does not accept parts marked as Rework or Scrap.");
                }                
            }
            else
            {
                database.CloseConnection();
                throw new Exception("Failed to Determine Double/Single scan status.");
            }
            database.CloseConnection();
            #endregion

            //Branch
            if (scan_type.Equals("single"))
            {
                session.SingleScan = true;
                SingleScan(style, status);
            }
            else if (scan_type.Equals("double"))
            {
                session.SingleScan = false;
                //Check to see if we have a Mold Label
                if (session.MoldPart != "")
                {
                    //If we do, we're ready to process a Double Scan
                    DoubleScan();                    
                }                
            }

            return session;
        }

        /// <summary>
        /// Method to Resolve a single scan to a Part
        /// </summary>
        protected void SingleScan(string style, int status)
        {
            string partNumber = new Paint().GetPartNumberByStyle(style);
            part = new Part().GetPaintedPart(partNumber, session.PaintLabel);
            ProcessPart();
        }

        /// <summary>
        /// Method to Resolve a double scan to a Part
        /// </summary>
        protected void DoubleScan()
        {
            part = new Part().GetPaintedPart(session.MoldPart, session.PaintLabel);
            ProcessPart();
        }

        /// <summary>
        /// Method to process a part (once label(s) are resolved to a part)
        /// </summary>
        protected void ProcessPart()
        {
            //Some sort of Corvette Check. Need the Paint Number instead of Finished good for those parts
            if (IsC6Part(part))
            {
                sPart pPart = new Part().GetByMoldPartAndPaintScan(part.Number, part.PaintSerial, PartType.PAINT);
                part = new Part().GetPartByPartNumber(pPart.Number);
            }

            // Get painted part number for LFa/QFa parts going to DDC for assembly
            if (part.Description1.Substring(0, 2) == "09" && (part.Description1.Substring(2, 2) == "QF" || part.Description1.Substring(2, 2) == "LF"))
            {
                sPart pPart = new Part().GetByMoldPartAndPaintScan(part.Number, part.PaintSerial, PartType.PAINT);
                part = new Part().GetPartByPartNumber(pPart.Number);
            }

            PassLabel(part);             

            ////Clear part from session, to prepare for next part
            session.PartNumber = part.Number;
            session.PartDescription = part.Description2;
            session.ClearPart();
        }

        public void ProcessAdditionalPart()
        {
            if (session.SingleScan == true)
            {
                string partNumber = new Paint().GetPartNumberByStyle(session.Style);
                part = new Part().GetPaintedPart(partNumber, session.PaintLabel);
            }
            else
            {
                part = new Part().GetPaintedPart(session.MoldPart, session.PaintLabel);
            }
            
            PassLabel(part);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public void PassLabel(sPart part)
        {
            try
            {
                // Now put it in the UniqueID table
                InsertUniqueID(part.PaintSerial, part.Number);

                // Print special Rehau part label for painted AMG W166 MOPF rockers going to Rehau for assembly
                //Note: This label prints in addition to any other labels printed, one per part
                if ((part.Description1.Substring(0, 9) == "01A6F R L" || part.Description1.Substring(0, 9) == "01A6F R R") && (part.PartType == "FG" || part.PartType == "PAINT"))
                {
                    try
                    {
                        //Printers.MiscellaneousLabelPrinter.PrintRehauPartLabel(part.Number, "204");
                        Printers.MiscellaneousLabelPrinter.PrintRehauPartLabel(part.Number, session.PassPrinter);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                //F25 Pass Label code will go here
                // Print special Plastic Omnium part label for BMW F25 Series Uppers
                if ((part.Description1.Substring(0, 9) == "02F25 FRU") && (part.PartType == "FG" || part.PartType == "PAINT"))
                {                    
                    //The last parameter for this method is the printer id. 204 is the pass label printer at unload. 228 is the IT test printer (3/8/2016)
                    //Printers.MiscellaneousLabelPrinter.PrintPassLabel(part.Number, "00000144", "204");
                    //Printers.MiscellaneousLabelPrinter.PrintPassLabel(part.Number, "00000144", "228");
                    Printers.MiscellaneousLabelPrinter.PrintPassLabel(part.Number, "00000144", session.PassPrinter);

                    //DO NOT DELETE!!! This code may still be needed later down the line (3/8/2016)
                    // Write paint label to log table to prevent re-scans
                    //DecoCore.MES.db PrintLogDB = new DecoCore.MES.db();
                    //PrintLogDB.Open(DecoCore.MES.DBType.SQL_AssemblyRecords);
                    //PrintLogDB.DBCommand.CommandText = "INSERT into AssemblyRecords.dbo.F25Series_PO_Log "
                    //                                    + "(PaintLabel, DecoPartNum, POPartNum, ScanTime) "
                    //                                    + "VALUES ('" + txtPaintScan.Text.Trim() + "','" + oPart.Number + "','" + oPart.CustomerNumber + "','" + DateTime.Now + "')";
                    //PrintLogDB.DBCommand.ExecuteNonQuery();
                    //PrintLogDB.Close();                    
                }


            }
            catch (Exception)
            {
                //return false;
            }            

            //return true;
        }

        public static bool UpdateHoldStatus(string barcode)
        {
            Database database = new Database();

            string status = "";

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT pnt_scan_status FROM [Production].[dbo].[pnt_exit_label] where paint_exit.pnt_barcode = @barcode";
            command.Parameters.AddWithValue("@barcode", barcode);
            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.HasRows == false)
            {
                status = "Invalid Paint Label";
            }
            else
            {
                reader.Read();
                status = reader["pnt_scan_status"].ToString();
            }

            database.CloseConnection();


            // Update Hold status of paint label in pnt_exit_label     
            // Update pnt_scan_status in pnt_exit_label table (*NEW* 7/28/15)
            // Only update parts that are fresh from the paint line. Finesse/Rework/Scrap is handled elsewhere
            if (status == "0" || status == "1")
            {                
                try
                {
                    command = new SqlCommand();
                    command.CommandText = "update Production.dbo.pnt_exit_label set pnt_scan_status = '7', hold = 'false' where pnt_barcode = @paintLabel";
                    command.Parameters.AddWithValue("@paintLabel", barcode);
                    database.RunCommand(command);
                }
                catch (Exception)
                {
                    //throw e;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Method to create a new rack
        /// </summary>
        private void AddNewRack()
        {
            AbstractRack rack;

            this.part = new Part().GetPaintedPart(session.PartNumber, session.PaintLabels[0]);

            if (part.Description1.Substring(2, 3) == "1V6" || part.Description1.Substring(2, 3) == "1VZ" || part.Description1.Substring(2, 3) == "1VS")
            {
                rack = new AbstractRack(part.Number, 0);
                if (part.Description1.Substring(6, 1) == "F" || part.Description1.Substring(6, 1) == "N")
                {
                    Printers.GMOrangeLabelPrinter.Print(part.Number, session.Printer);
                }
            }
            else
            {
                rack = RackFactory.Make(part.Number, 0);
            }

            if (IsC6Part(part))
            {
                rack.LastLocation = "NOSHIP";
            }

            session.Rack = rack;
        }

        private void UpdateRackCount()
        {
            Database database = new Database();
            SqlCommand command = new SqlCommand();   
            command.CommandText = "SELECT Count(*) FROM [RackRecords].[dbo].[Paint_Rack_XRef] where rack_id = @rack";
            command.Parameters.AddWithValue("@rack", session.Rack.RackID);

            SqlDataReader reader = database.RunCommandReturnReader(command);
            reader.Read();
            int rackCount = reader.GetInt32(0);

            AbstractRack rack = session.Rack;
            rack.Quantity = rackCount;

            session.PartDescription = part.Description2;
            session.RackID = session.Rack.RackID;
            session.RackQuantity = session.Rack.Quantity;

            database.CloseConnection();
        }        

        private void AddToRack(string paintLabel)
        {
            //After passing checks, add part to rack
            Database database = new Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "UPDATE [RackRecords].[dbo].[Paint_Rack_XRef] SET rack_id = @rack WHERE paintscan = @paintLabel "
                                + "IF @@ROWCOUNT=0 Insert INTO [RackRecords].[dbo].[Paint_Rack_XRef] (paintscan, moldscan, rack_id) VALUES (@paintLabel, @moldLabel, @rack)";
            command.Parameters.AddWithValue("@paintLabel", paintLabel);
            command.Parameters.AddWithValue("@moldLabel", "");
            command.Parameters.AddWithValue("@rack", session.Rack.RackID);

            database.RunCommand(command);

            database.CloseConnection();
        }

        /// <summary>
        /// Method to insert part into Unique Id table
        /// </summary>
        /// <param name="paintLabel"></param>
        /// <param name="partNumber"></param>
        public void InsertUniqueID(string paintLabel, string partNumber)
        {
            Database database = new Database();
            SqlCommand command;

            try
            {                
                //Add part to uniqueID table
                command = new SqlCommand();
                command.CommandText = "insert into Production.dbo.UniqueID (serialnumber, partnumber, datetime) values (@paintLabel, @partNumber, GetDate())";
                command.Parameters.AddWithValue("@paintLabel", paintLabel);
                command.Parameters.AddWithValue("@partNumber", partNumber);
                database.RunCommand(command);
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }                

        /// <summary>
        /// C6 is a corvette part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public bool IsC6Part(sPart part)
        {
            if (part.Description1.Substring(6, 1) == "A")
                return false;

            Database database = new Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "select    pt_desc1 from Decostar.dbo.pt_mstr_sql "
                                 + "where     pt_part = @PartNumber";
            command.Parameters.AddWithValue("@PartNumber", part.Number);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            reader.Read();
            string pt_desc1 = reader["pt_desc1"].ToString().Substring(3, 2);
            database.CloseConnection();

            if (pt_desc1 != "VZ" && pt_desc1 != "V6" && pt_desc1 != "VS")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Check that the part is not already scanned to the Rack
        /// </summary>
        /// <param name="paintLabel"></param>
        /// <returns></returns>
        private bool CheckDuplicate(string paintLabel)
        {
            bool returnValue;

            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "Select ID FROM [RackRecords].[dbo].[Paint_Rack_XRef] where rack_id = @rack AND paintscan = @paintLabel";
            command.Parameters.AddWithValue("@rack", session.Rack.RackID);
            command.Parameters.AddWithValue("@paintLabel", paintLabel);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            returnValue = reader.HasRows;
            database.CloseConnection();

            return returnValue;
        }

        /// <summary>
        /// Method to ensure that a part is scanned to a rack of that part type
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns>True if Rack and part match, false otherwise</returns>
        private bool CompareRackAndPart(string partNumber)
        {
            if (session.Rack.PartNumber == partNumber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method to print a rack
        /// </summary>
        public void PrintRack()
        {
            try
            {     
                //First, we check to see if we already have a Rack
                if (session.RackID != 0)
                {
                    session.Rack = RackFactory.Make(session.RackID);
                }
                else
                {
                    if (session.PartNumber != null && session.PartNumber != "0")
                    {
                        AddNewRack();
                    }
                    else
                    {
                        //We don't have a Rack or a Partnumber, so we certainly can't print anything
                        throw new Exception("No Rack or Part Number Specified");
                    }
                }

                //If we do, we open it, then add our list of barcodes to it
                this.part = new Part().GetPaintedPart(session.PartNumber, session.PaintLabels[0]);
                Parallel.ForEach(session.PaintLabels, paintLabel => AddToRack(paintLabel));
                UpdateRackCount();                
                
                Printers.Printer printer;

                //   E70/71 Service Make Internal Labels
                if ((part.Description1.StartsWith("02M71") || part.Description1.StartsWith("02M70")) && part.DescType == "SVPAINT")
                {
                    printer = new Printers.MSIGRackPrinter(session.Printer, this.ToString(), session.Rack);
                }
                else
                {
                    printer = Printers.PrintFactory.Select(session.Rack, Printers.Label.RCK, session.Printer, this.ToString());
                }

                // Corvette Roof Panels Make Internal Labels
                if (part.Description1.Substring(0, 2) == "08" && part.Description1.Substring(3, 1) == "V" && part.Description1.Substring(6, 1) == "A" && part.Description1.Substring(0, 2) == "08")
                {
                    Database database = new Database();
                    string sQuery = " select    * "
                                  + " from      Decostar.dbo.pt_mstr_sql "
                                  + " where     left(pt_desc1,18) = left('" + part.Description1 + "',16)"
                                  + " and       right(pt_desc1,4) = right('" + part.Description1 + "',4)"
                                  + " and       pt_part_type = 'PAINTA'";
                    SqlDataReader reader = database.RunCommandReturnReader(sQuery);
                    reader.Read();
                    string sNewPart = reader["pt_part"].ToString();

                    session.Rack = new AbstractRack(sNewPart, session.Rack.Quantity);
                    session.Rack.Type = RackType.MercedesRack;
                    printer = new Printers.MSIGRackPrinter(session.Printer, this.ToString(), session.Rack);
                }
                else if (part.Description1.Substring(2, 3) == "1V6" || part.Description1.Substring(2, 3) == "1VZ" || part.Description1.Substring(2, 3) == "1VS")
                {
                    printer = new Printers.InternalRackPrinter(session.Printer, this.ToString(), session.Rack);
                }

                //Print internal Do Not Ship labels for F25 series parts going to Inspection before shipping to Plastic Omnium
                else if (part.Description1.Substring(2, 3) == "F25")
                {
                    printer = new Printers.InternalRackPrinter(session.Printer, this.ToString(), session.Rack);
                }

                else
                {
                    printer = Printers.PrintFactory.Select(session.Rack, Printers.Label.RCK, session.Printer, this.ToString());
                }

                //Print and remove Rack from session
                printer.print();
                session.ClearPart();
                session.ClearRack();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string GetMoldPartNumber(string moldLabel)
        {
            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "Select PartNumber From [Production].[dbo].[UniqueID] where SerialNumber = @barcode";            
            command.Parameters.AddWithValue("@barcode", moldLabel);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            reader.Read();
            string moldPartNumber = reader["PartNumber"].ToString();
            database.CloseConnection();

            return moldPartNumber;
        }        
    }
}