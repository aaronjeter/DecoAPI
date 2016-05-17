using System;
using System.Data.SqlClient;

namespace DecoAPI.Models
{
    /// <summary>
    /// Summary description for part.
    /// </summary>
    public class Part
    {
        public sPart GetPartByCustomerPartNumber(string customerPartNumber)
        { 
            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "select cp_part from Decostar.dbo.cp_mstr_sql where cp_cust_part = @customerPartNumber";
            command.Parameters.AddWithValue("@customerPartNumber", customerPartNumber);

            return GetPartByPartNumber(database.RunSingleResultQuery(command));
        }
                
        public sPart GetPartByPartNumber(string PartNumber)
        {
            sPart returnPart = new sPart();
            
            Database database = new Database();
            if (database.OpenConnection())
            {
                SqlCommand command = new SqlCommand();
                
                // get alternate full description w/color name, not code, for Club Car
                command.CommandText = "SELECT pt_part, " +
                    "pt_desc1, " +
                    "pt_desc2, " +
                    "pt_prod_line, " +
                    "pt_part_type, " +
                    "pt_status, " +
                    "pt_loc, " +
                    "pt_site, " +
                    "Production.dbo.getColorDesc(SUBSTRING(pt_desc1,21,4)) as color_desc, " +
                    "Production.dbo.getFullDescription(pt_desc1) as full_desc, " +
                    "Production.dbo.getOptions(pt_desc1) as opt_desc, " +
                    "Production.dbo.getPartType(pt_desc1) as part_type, " +
                    "Production.dbo.getPosition(pt_desc1) as position, " +
                    "Production.dbo.getProgram(pt_desc1) as program, " +
                    "Production.dbo.getBMWPartType(pt_desc2) as bmw_part_type, " +
                    "pt_rev, " +
                    "pt_ship_wt " +
                    "FROM Decostar.dbo.pt_mstr_sql where pt_part = @PartNumber";

                command.Parameters.AddWithValue("@PartNumber", PartNumber);

                SqlDataReader DR = database.RunCommandReturnReader(command);
                if (DR.Read())
                {
                    returnPart.Number = DR["pt_part"].ToString();
                    returnPart.MoldSerial = "";
                    returnPart.Description1 = DR["pt_desc1"].ToString();
                    returnPart.Description2 = DR["pt_desc2"].ToString();
                    returnPart.ProductLine = DR["pt_prod_line"].ToString();
                    returnPart.PartType = DR["pt_part_type"].ToString();
                    returnPart.PartStatus = DR["pt_status"].ToString();
                    returnPart.DescColor = DR["color_desc"].ToString();
                    returnPart.DescFull = DR["full_desc"].ToString();
                    returnPart.DescOptions = DR["opt_desc"].ToString();
                    returnPart.DescPartType = DR["part_type"].ToString();
                    returnPart.DescPosition = DR["position"].ToString();
                    returnPart.DescProgram = DR["program"].ToString();
                    returnPart.BWMPartType = DR["bmw_part_type"].ToString();
                    returnPart.RevisionLevel = DR["pt_rev"].ToString();
                    returnPart.PartWeight = DR["pt_ship_wt"].ToString();
                    returnPart.DefaultLocation = DR["pt_loc"].ToString();
                    returnPart.DefaultSite = DR["pt_site"].ToString();                                        
                }
                else
                {
                    throw new Exception("Can not find part in pt_mstr.");
                }
                DR.Close();
                try
                {
                    command.CommandText = "select cp_cust_part, cp_comment from Decostar.dbo.cp_mstr_sql where cp_part = @PartNumber";
                    //command.Parameters.AddWithValue("@PartNumber", PartNumber);
                    DR = database.RunCommandReturnReader(command);
                    if(DR.Read())
                    {
                        returnPart.CustomerNumber = DR["cp_cust_part"].ToString();
                        returnPart.CustomerDesc = DR["cp_comment"].ToString();
                    }                    
                }
                catch
                {
                    returnPart.CustomerNumber = "";
                }
            }
            else
            {
                database.CloseConnection();
                throw new Exception("Database Connection Error");
            }
            database.CloseConnection();
            return returnPart;
        }

        public sPart GetPaintedPart(string MoldNumber, string PaintSerial)
        {
            return GetPaintedPart(MoldNumber, PaintSerial, false);
        }        

        public sPart GetPaintedPart(string moldPartNumber, string paintSerial, bool service)
        {
            int plt_clr = 0;
            Database database = new Database();

            try
            {
                SqlCommand command = new SqlCommand();
                command.CommandText = "SELECT pnt_color FROM Production.dbo.pnt_exit_label WHERE pnt_barcode = @paintLabel";
                command.Parameters.AddWithValue("@paintLabel", paintSerial);
                SqlDataReader reader = database.RunCommandReturnReader(command);
                reader.Read();
                plt_clr = Int32.Parse(reader["pnt_color"].ToString());
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                database.CloseConnection();
            }

            //do lookup to get plant color
            //Added the Paintlabel to the sPart struct as well. We have use of that info later on.
            sPart part = GetPaintedPart(moldPartNumber, plt_clr, service);
            part.PaintSerial = paintSerial;
            return part;
        }

        public sPart GetPaintedPart(string MoldPartNumber, int PlantColor, bool service)
        {
            sPart returnPart = new sPart();
            Database database = new Database();
            string ColorCode = "";
            string type = string.Empty;
            string cust = string.Empty;
            string prog = string.Empty;
            string tmp_pt_number = "";

            string selectLine = "SELECT pt_part FROM Decostar.dbo.pt_mstr_sql ";
            string whereLine = "WHERE Left(pt_desc1,18) = (SELECT LEFT(pt_desc1,18) FROM Decostar.dbo.pt_mstr_sql WHERE pt_part = @MoldPartNumber) ";
            string colorCodeLine = "AND RIGHT(pt_desc1,4) = @ColorCode";
            string partStatusActive = "AND pt_status = 'AC' ";

            // Check to see if the color is negative
            if (PlantColor < 0)
            {
                throw new Exception("Mechanical defect present. Bad part.");
            }

            try
            {  
                SqlDataReader reader = null;
                SqlCommand command = new SqlCommand();

                //Get ColorCode
                command.CommandText = "SELECT * FROM Production.dbo.Colors WHERE Plant_Color = @PlantColor";
                command.Parameters.AddWithValue("@PlantColor", PlantColor);

                reader = database.RunCommandReturnReader(command);
                reader.Read();
                ColorCode = reader["Color_Code"].ToString(); 
                
                reader.Close();
                command.Parameters.Clear();

                //Get type, Customer, and Program from part description 1
                command.CommandText = " SELECT SUBSTRING(pt_desc1, 7,1) as pos, "
                                               + "SUBSTRING(pt_desc1,1,2) as cust, "
                                               + "SUBSTRING(pt_desc1,3,3) as prog "
                                               + "FROM Decostar.dbo.pt_mstr_sql WHERE pt_part = @MoldPartNumber";

                command.Parameters.AddWithValue("@MoldPartNumber", MoldPartNumber);

                reader = database.RunCommandReturnReader(command);

                reader.Read();
                type = reader["pos"].ToString();
                cust = reader["cust"].ToString();
                prog = reader["prog"].ToString();
                                
                reader.Close();
                command.Parameters.Clear();

                command.Parameters.AddWithValue("@MoldPartNumber", MoldPartNumber);
                command.Parameters.AddWithValue("@ColorCode", ColorCode);

                
                if (type.ToLower() == "r")
                {
                    #region Rockers
                    #region Mercedes

                    //Customer 01 is Mercedes
                    if (cust == "01")
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = @PartType1 OR pt_part_type = @PartType2) " + partStatusActive + colorCodeLine;

                        // Mercedes AMG 166 MOPF parts that come out as PAINT and are sent to Rehau for assembly
                        if (prog == "A6F")
                        {    
                            command.Parameters.AddWithValue("PartType1", "PAINT");
                            command.Parameters.AddWithValue("PartType2", "SVPAINT");
                            reader = database.RunCommandReturnReader(command);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("PartType1", "FG");
                            command.Parameters.AddWithValue("PartType2", "SVFG");
                            reader = database.RunCommandReturnReader(command);
                        }
                    }
                    #endregion

                    #region Nissan
                    else if (cust == "07")
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = @PartType) " + partStatusActive + colorCodeLine;

                        // PAINT label exception for L42N
                        if (prog == "NML")
                        {
                            command.Parameters.AddWithValue("@PartType", "PAINT");
                            reader = database.RunCommandReturnReader(command);
                        }
                        else //if not L42N
                        {   
                            command.Parameters.AddWithValue("@PartType", "SVPAINT");
                            reader = database.RunCommandReturnReader(command);

                            if (reader.HasRows == false)
                            {
                                reader.Close();
                                command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'FG' OR pt_part_type = 'SVFG') " + partStatusActive + colorCodeLine;
                                reader = database.RunCommandReturnReader(command);
                            }
                        }
                    }
                    #endregion

                    #region BMW
                    else if (prog == "Z85" || prog == "M85" || prog == "M25")
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT' OR pt_part_type = 'SVPAINT') " + partStatusActive + colorCodeLine;
                        reader = database.RunCommandReturnReader(command);
                    }
                    #endregion
                    #region Kia/Hyundai
                    else if (prog == "XM " || prog == "LF " || prog == "QF " || prog == "AN ") // KIA / Hyundai
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'FG') " + partStatusActive + colorCodeLine;
                        reader = database.RunCommandReturnReader(command);
                    }
                    else if(prog == "JF ") //Print Painted Part label for JFA. JFA parts are assembled at DDC
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT') " + partStatusActive + colorCodeLine;
                        reader = database.RunCommandReturnReader(command);
                    }
                    #endregion
                    #region BMW
                    else if (prog == "M70" || prog == "M71")
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'FG') " + partStatusActive + colorCodeLine;
                        reader = database.RunCommandReturnReader(command);
                        if (reader.HasRows == false)
                        {
                            reader.Close();
                            command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVPAINT') " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                                command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVFG') " + partStatusActive + colorCodeLine;
                                reader = database.RunCommandReturnReader(command);
                            }
                        }
                    }
                    #endregion
                    #endregion
                }                
                else
                {
                    #region Mercedes V2
                    if ((prog == " V2") && (cust == "01"))
                    {
                        if (type == "A")
                        {
                            command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT') " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                                command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINTA') " + partStatusActive + colorCodeLine;
                                reader = database.RunCommandReturnReader(command);
                            }                            
                        }
                    }
                    #endregion

                    #region EZ-GO
                    // if it is an EZGo part, look for FG first
                    if (MoldPartNumber.StartsWith("3"))
                    {
                        // It is possible we have the wrong plant color for EZGo parts
                        // so we do a specific lookup for these parts
                        command.CommandText = " select 	color_code "
                                    + " from        Production.dbo.Colors "
                                    + " where	    plant_color = " + PlantColor
                                    + " and	        left(color_code,2) = 	( "
                                    + " 				                    select 	right(pt_desc1,2) "
                                    + " 				                    from 	Decostar.dbo.pt_mstr_sql "
                                    + "                                     where 	pt_part = @MoldPartNumber"
                                    + "                                     and     pt_status = 'AC' "
                                    + "                                     ) ";
                        reader = database.RunCommandReturnReader(command);
                        reader.Read();
                        ColorCode = reader["Color_Code"].ToString();

                        reader.Close();
                        
                        command.CommandText =
                        " SELECT pt_part, right(pt_desc1, 2) as mold_color " +
                        " FROM Decostar.dbo.pt_mstr_sql p " +
                        " WHERE p.pt_part = @MoldPartNumber ";

                        //command.Parameters.AddWithValue("@MoldPartNumber", MoldPartNumber);
                        reader = database.RunCommandReturnReader(command);
                        if (reader.HasRows == false)
                        {
                            reader.Close();
                            throw new Exception("Invalid Painted Part. Color: " + PlantColor + " Mold: " + MoldPartNumber);
                        }
                        string sMoldColor = "";
                        try
                        {
                            reader.Read();
                            sMoldColor = reader["mold_color"].ToString();
                            reader.Close();

                        }
                        catch
                        {
                            throw new Exception("Mold Color not found for " + MoldPartNumber.ToString());
                        }

                        //reader = database.RunCommandReturnReader("(SELECT LEFT(pt_desc1,16) DESC1 FROM Decostar.dbo.pt_mstr_sql WHERE pt_part = @MoldPartNumber)");
                        SqlCommand tempCommand = new SqlCommand();
                        tempCommand.CommandText = "(SELECT LEFT(pt_desc1,16) DESC1 FROM Decostar.dbo.pt_mstr_sql WHERE pt_part = @MoldPartNumber)";
                        tempCommand.Parameters.AddWithValue("@MoldPartNumber", MoldPartNumber);
                        reader = database.RunCommandReturnReader(tempCommand);
                        reader.Read();
                        string sDesc1 = reader["DESC1"].ToString();
                        reader.Close();

                        // If the part is NOT service, look for FG (Finished Good) 
                        //  if it IS, look for SVFG (Service Finished Good)
                        string sFinishedType = string.Empty;
                        if (service)
                            sFinishedType = "SVFG";
                        else
                            sFinishedType = "FG";

                        command.CommandText =
                            selectLine +
                            "WHERE Left(pt_desc1,16) = '" + sDesc1 + "'" +
                            "AND (pt_part_type = '" + sFinishedType + "') " +
                            "AND pt_status = 'AC' " +
                            "AND '" + sMoldColor + "' = LEFT(@ColorCode,2)" + colorCodeLine;
                            //"AND RIGHT(pt_desc1,4) = @ColorCode";
                        reader = database.RunCommandReturnReader(command);
                        if (reader.HasRows == false)
                        {
                            reader.Close();
                            throw new Exception("Cannot identify E-Z-Go part.");
                        }
                    }
                    #endregion
                    #region Club Car?
                    if (MoldPartNumber.StartsWith("42"))
                    {
                        /////
                        command.CommandText =
                        " SELECT pt_part, right(pt_desc1, 2) as mold_color " +
                        " FROM Decostar.dbo.pt_mstr_sql p " +
                        " WHERE p.pt_part = @MoldPartNumber ";

                        reader = database.RunCommandReturnReader(command);
                        if (reader.HasRows == false)
                        {
                            reader.Close();
                            throw new Exception("Invalid Painted Part. Color: " + PlantColor + " Mold: " + MoldPartNumber);
                        }
                        string sMoldColor = "";
                        try
                        {
                            reader.Read();
                            sMoldColor = reader["mold_color"].ToString();
                            reader.Close();

                        }
                        catch
                        {
                            throw new Exception("Mold Color not found for " + MoldPartNumber.ToString());
                        }
                        /////

                        reader = database.RunCommandReturnReader("(SELECT LEFT(pt_desc1,16) DESC1 FROM Decostar.dbo.pt_mstr_sql WHERE pt_part = @MoldPartNumber)");
                        reader.Read();
                        string sDesc1 = reader["DESC1"].ToString();
                        reader.Close();

                        command.CommandText = " SELECT    pt_part FROM Decostar.dbo.pt_mstr_sql " +
                                                            " WHERE     (Left(pt_desc1,16) = substring('" + sDesc1 + "',1,8) + '  L' " +
                                                            " OR        Left(pt_desc1,16) = Left('" + sDesc1 + "',16))" +
                                                            " AND       pt_part_type = 'FG' " +
                                                            " AND       pt_status = 'AC' " +
                                                            " AND       RIGHT(pt_desc1,4) = '" + sMoldColor + ColorCode.Substring(2, 2) + "' ";
                        reader = database.RunCommandReturnReader(command);
                        if (reader.HasRows == false)
                        {
                            reader.Close();
                            throw new Exception("Cannot identify Club Car part.");
                        }
                    }
                    #endregion
                    #region Nissan
                    if (MoldPartNumber.StartsWith("7"))
                    {
                        if (type == "B")
                        {
                            command.CommandText = selectLine + whereLine + "AND pt_part_type = 'PAINT' " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                            }
                        }
                    }
                    #endregion
                    #region Kia/Hyundai
                    if (prog == "XM " || prog == "LF " || prog == "AN ")  // KIA / Hyundai
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'FG') " + partStatusActive + colorCodeLine;
                        reader = database.RunCommandReturnReader(command);
                        if (reader.HasRows == false)
                        {
                            reader.Close();
                        }
                    }
                    else if (prog == "QF ") // Hyundai QFa Painted to DDC
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT') " + partStatusActive + colorCodeLine;
                        reader = database.RunCommandReturnReader(command);
                    }
                    #endregion
                    #region Honda
                    if (prog == " HR") // HONDA
                    {
                        command.CommandText = selectLine + whereLine + "AND pt_part_type = 'PAINT' " + partStatusActive + colorCodeLine;
                        reader = database.RunCommandReturnReader(command);
                        if (reader.HasRows == false)
                        {
                            reader.Close();
                            command.CommandText = selectLine + whereLine +"AND pt_part_type = 'FG' " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                            }
                        }
                    }
                    #endregion
                    #region GM
                    if (MoldPartNumber.StartsWith("8"))
                    {
                        if (type == "W")
                        {
                            command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT') " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                                command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'FG') " + partStatusActive + colorCodeLine;
                                reader = database.RunCommandReturnReader(command);
                                if (reader.HasRows == false)
                                {
                                    reader.Close();
                                    command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVPAINT') " + partStatusActive + colorCodeLine;
                                    reader = database.RunCommandReturnReader(command);
                                    if (reader.HasRows == false)
                                    {
                                        reader.Close();
                                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVFG') " + partStatusActive + colorCodeLine;
                                        reader = database.RunCommandReturnReader(command);
                                    }
                                }
                            }
                        }
                        else if (type == "A")
                        {
                            command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT') " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                                command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINTA') " + partStatusActive + colorCodeLine;
                                reader = database.RunCommandReturnReader(command);
                            }
                        }
                        else if (type == "L" || type == "O")
                        {
                            command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT') " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                                command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVFG') " + partStatusActive + colorCodeLine;
                                reader = database.RunCommandReturnReader(command);
                                if (reader.HasRows == false)
                                {
                                    reader.Close();
                                    command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVPAINT') " + partStatusActive + colorCodeLine;
                                    reader = database.RunCommandReturnReader(command);
                                    if (reader.HasRows == false)
                                    {
                                        reader.Close();
                                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'FG') " + partStatusActive + colorCodeLine;
                                        reader = database.RunCommandReturnReader(command);
                                    }
                                }
                            }
                        }
                        else
                        {
                            command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'FG') " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                                command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVPAINT') " + partStatusActive + colorCodeLine;
                                reader = database.RunCommandReturnReader(command);
                                if (reader.HasRows == false)
                                {
                                    reader.Close();
                                    command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVFG') " + partStatusActive + colorCodeLine;
                                    reader = database.RunCommandReturnReader(command);
                                }
                                if (reader.HasRows == false)
                                {
                                    reader.Close();
                                    command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT') " + partStatusActive + colorCodeLine;
                                    reader = database.RunCommandReturnReader(command);
                                }
                            }
                        }
                    }
                    #endregion
                    #region Big Catch-All at the end
                    if (reader.IsClosed)
                    {
                        command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'PAINT' OR pt_part_type = 'SVPAINT') " + partStatusActive + colorCodeLine;
                        reader = database.RunCommandReturnReader(command);
                        if (reader.HasRows == false)
                        {
                            reader.Close();
                            command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVFG') " + partStatusActive + colorCodeLine;
                            reader = database.RunCommandReturnReader(command);
                            if (reader.HasRows == false)
                            {
                                reader.Close();
                                command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'SVMULTI') " + partStatusActive + colorCodeLine;
                                reader = database.RunCommandReturnReader(command);
                                if (reader.HasRows == false)
                                {
                                    reader.Close();
                                    command.CommandText = selectLine + whereLine + "AND (pt_part_type = 'FG' OR pt_part_type = 'MOLDP') " + partStatusActive + colorCodeLine;
                                    reader = database.RunCommandReturnReader(command);
                                }
                            }
                        }
                    }
                    #endregion
                }

                //And finally, read that reader
                if (reader.HasRows == false)
                {
                    throw new Exception("Failed to Resolve to a Part Number");
                }
                reader.Read();
                tmp_pt_number = reader[0].ToString();
            }
            catch (Exception e)
            {
                throw e;
            }

            returnPart = GetPartByPartNumber(tmp_pt_number);
            return returnPart;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public sPart GetByMoldPartAndPaintScan(string moldPart, string paintScan, PartType type)
        {
            // Fields
            string colorQuery = string.Empty;
            string moldQuery = string.Empty;
            string colorCode = string.Empty;
            string desc1 = string.Empty;
            sPart oMoldPart = new sPart();
            //sPart oPaintPart = new sPart();
            Database database = new Database();

            // Get the Molded Part
            oMoldPart = GetByPartNumber(moldPart);

            // Get the Color Code of the Paint Scan
            colorQuery =  " select  color_code "
                        + " from    Colors "
                        + " where   plant_color =  (select  pnt_color "
                        + " from    Production.dbo.pnt_exit_label "
                        + " where   pnt_barcode = '" + paintScan + "')";

            SqlDataReader reader = database.RunCommandReturnReader(colorQuery);
            reader.Read();
            colorCode = reader["color_code"].ToString();            

            // Find the Part Number by Description1 and return it
            return GetByDescription1(oMoldPart.Description1, colorCode, type);
        }

        public sPart GetByDescription1(string sMoldDesc1, string sColorCode, PartType oType)
        {
            string sDescQuery = string.Empty;
            string sPartNumber = string.Empty;
            string sShortMoldDesc1 = string.Empty;
            Database database = new Database();
            
            sDescQuery = " select   pt_part "
                        + " from     Decostar.dbo.pt_mstr_sql "
                        + " where    left(pt_desc1,16) = left('" + sMoldDesc1 + "',16) "
                        + " and      right(pt_desc1,4) = '" + sColorCode + "' "
                        + " and      pt_part_type = '" + oType.ToString() + "' "
                        + " and      pt_status = 'AC' "
                        + " order by pt_rev desc ";
            SqlDataReader reader = database.RunCommandReturnReader(sDescQuery);
            reader.Read();
            sPartNumber = reader["pt_part"].ToString();            

            // Populate the Part and return it
            return GetByPartNumber(sPartNumber);
        }
        
        public sPart GetByPartNumber(string partNumber)
        {
            // Declare Variables            
            string query = string.Empty;
            sPart part = new sPart();
            Database database = new Database();

            // Test to make sure a part number has been given
            if (partNumber == string.Empty)
            {
                throw new Exception("Part cannot be found.");
            }
            
            SqlCommand command = new SqlCommand();            

            // Search for information about given Part Number
            //  and populate the Part object with found info.
            command.CommandText =   "select *, "
                                  + "Production.dbo.getColorDesc(SUBSTRING(pt_desc1,21,4)) as desc_color, "
                                  + "Production.dbo.getFullDescription(pt_desc1) as desc_full, "
                                  + "Production.dbo.getOptions(pt_desc1) as desc_options, "
                                  + "Production.dbo.getPartType(pt_desc1) as desc_part_type, "
                                  + "Production.dbo.getPosition(pt_desc1) as desc_position, "
                                  + "Production.dbo.getProgram(pt_desc1) as desc_program, "
                                  + "Production.dbo.getBMWPartType(pt_desc2) as desc_type "
                                  + "from Decostar.dbo.pt_mstr_sql "
                                  + "where pt_part = @partNumber "
                                  + "order by pt_rev desc ";
            command.Parameters.AddWithValue("@partNumber", partNumber);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            reader.Read();

            part.Number = reader["pt_part"].ToString();
            part.Description1 = reader["pt_desc1"].ToString();
            part.Description2 = reader["pt_desc2"].ToString();
            part.ProductLine = reader["pt_prod_line"].ToString();            
            part.RevisionLevel = reader["pt_rev"].ToString();
            part.DescColor = reader["desc_color"].ToString();
            part.DescFull = reader["desc_full"].ToString();
            part.DescOptions = reader["desc_options"].ToString();
            part.DescPartType = reader["desc_part_type"].ToString();
            part.DescPosition = reader["desc_position"].ToString();
            part.DescProgram = reader["desc_program"].ToString();
            part.DescType = reader["desc_type"].ToString();

            // Search for Customer Info. If none is found, make blank
            try
            {
                reader.Close();
                command = new SqlCommand();
                command.CommandText = "Select cp_cust_part, cp_comment from Decostar.dbo.cp_mstr_sql where cp_part = @partNumber";
                command.Parameters.AddWithValue("@partNumber", partNumber);
                reader = database.RunCommandReturnReader(command);

                part.CustomerNumber = reader["cp_cust_part"].ToString();
                part.CustomerDesc = reader["cp_comment"].ToString();
            }
            catch (Exception)
            {
                part.CustomerNumber = string.Empty;
                part.CustomerDesc = string.Empty;
            }

            return part;
        }
    }

    #region Struct(s)

    public struct sPart
    {
        public string Number;
        public string CustomerNumber;
        public string CustomerDesc;
        public string PaintSerial;
        public string MoldSerial;
        public string Description1;
        public string Description2;
        public string ProductLine;
        public string PartType;
        public string PartStatus;
        public string DescColor;
        public string DescFull;
        public string DescOptions;
        public string DescPartType;
        public string DescPosition;
        public string DescProgram;
        public string DescType;
        public string RevisionLevel;
        public string DefaultLocation;
        public string DefaultSite;
        public string BWMPartType;
        public string PartWeight;
    }

    public struct PassLabel
    {
        public string label_time;
        public string location;
        public string mold_scan;
        public string paint_scan;
        public string printed_label;
        public string part_number;
        public string serial;
    }

    public enum PartType
    {
        UNKNOWN,
        MOLD,
        MOLDP,
        PUNC100,
        PAINT1,
        PAINT,
        PAINTA,
        SVPAINT,
        SVFG,
        FG
    }

    public struct PaintExitID
    {
        public long ID;
        public string CreateDate;
        public string barcode;
        public int carrier;
        public int CarrierPosition;
        public int Round;
        public int Color;
        public int Style;
        public int Status;
        public long StatusID;
        public int ScanStatus;
        public int ReturnStatus;
        public int ReturnScanStatus;
        public DateTime ReturnStatusChange;
        public string XRefPartNumber;
        public string OptionedPartNumber;
        public string DispositionedBy;
        public DateTime? BlackoutDate;
        public string sScanType;
    }

    #endregion
}