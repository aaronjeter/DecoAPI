/****************************************************************************
 * DecoCore.Rack.RackFactory
 * 
 * This is a Factory used to determine what kind of object
 *  needs to be created in order to make the correct rack
 *  type. It takes in either a part number and a quantity,
 *  or it takes in a rack number, and it will return the
 *  corresponding type of rack.
 * 
 * RackFactory.Select(...) is a static method that returns
 *  the correct type of Rack.
 * - sPartNumber: a string giving the part number of the
 *    part to make the rack out of. If it starts with a 
 *    "1", Make will return a MercedesRack, if it starts
 *    with a "5", Make will return a BMWRack, and if it
 *    starts with a "3", Make will return an EZGoRack
 * - iQuantity: an integer to be used to set the quantity
 *    of parts on the rack.
 * 
 * - iRackID: used in a Make overload which will find an
 *    existing rack and return that kind. It also looks
 *    at the part number associated with that rack to 
 *    determine which kind of rack to return.
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DecoAPI.Models
{
    public class RackFactory
    {
        /// <summary>
        /// Creates a new Rack using a part number and quantity on the rack.
        /// </summary>
        /// <param name="partNumber">Part on the rack</param>
        /// <param name="quantity">Quantity of parts on the rack</param>
        /// <returns>Gives back a Rack Object</returns>
        public static AbstractRack Make(string partNumber, int quantity)
        {
            return Make(partNumber, quantity, "");
        }
                      
        /// <summary>
        /// Creates a new Rack using a part number, a quantity, and a ship location
        /// </summary>
        /// <param name="partNumber">Part on the rack</param>
        /// <param name="quantity">Quantity of parts on the rack</param>
        /// <param name="shipLocation">The location that the rack will ship</param>
        /// <returns>Gives back a Rack Object</returns>
        public static AbstractRack Make(string partNumber, int quantity, string shipLocation)
        {
            sPart part = new Part().GetPartByPartNumber(partNumber);

            AbstractRack rack;

            //// MIRROR Rack ////
            if (part.Description1.Substring(6, 1) == "U" || part.Description1.PadRight(9, ' ').Substring(0, 9) == "02F25 FRU" || part.Description1.PadRight(9, ' ').Substring(0, 9) == "02F25 FFU")
            {
                rack = new AbstractRack(partNumber, quantity, shipLocation);
                rack.Type = RackType.BMWE7Rack;
                return rack;
            }
            //// MSIG Racks ////
            if (partNumber.StartsWith("1") || partNumber.StartsWith("2") || partNumber.StartsWith("7"))
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.MercedesRack;
                return rack;
            }                
            //// EZGO Racks ////
            if (partNumber.StartsWith("3"))
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.EZGoRack;
                return rack;
            }                
            //// Club Car Racks ////
            if (partNumber.StartsWith("4"))
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.CCRack;
                return rack;
            }                
            //// BMW Z85 Racks ////
            if (part.Description1.Substring(2, 3) == "Z85" || part.Description1.Substring(2, 3) == "M85")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.BMWRack;
                return rack;
            }                
            //// BMW E70/E71 Service Racks
            if ((part.Description1.Substring(2, 3) == "M70" || part.Description1.Substring(2, 3) == "M71") && part.PartType == "SVFG")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.MercedesRack; //this is intentional. MercedesRack is the default rack format, since we started out making mercedes parts.
                return rack;
            }                
            //// BMW E70/E71 Racks
            if (part.Description1.Substring(2, 3) == "M70" || part.Description1.Substring(2, 3) == "M71")
            {
                rack = new AbstractRack(partNumber, quantity, shipLocation);
                rack.Type = RackType.BMWE7Rack;
                return rack;
            }                
            //// BMW F25 Racks
            if (part.Description1.Substring(2, 3) == "M25")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.MercedesRack;
                return rack;
            }                
            //// BMW F15 Racks
            if (part.Description1.Substring(2, 3) == "M15" || part.Description1.Substring(2, 3) == "F15")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.MercedesRack;
                return rack;
            }                
            //// Honda Racks ////
            if (part.Description1.Substring(3, 1) == "H")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.MercedesRack;
                return rack;
            }                
            //// Toyota Racks ////
            if (part.Description1.Substring(3, 2) == "T1")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.ToyotaRack;
                return rack;
            }                
            //// VW Racks ////
            if (part.Description1.Substring(3, 2) == "VW")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.MercedesRack;
                return rack;
            }                
            //// General Motors C6 Racks & Z06 Racks ////
            if (part.Description1.Substring(2, 3) == "1V6" || part.Description1.Substring(2, 3) == "1VZ" || part.Description1.Substring(2, 3) == "1VS")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.GMRack;
                return rack;
            }                
            //// GMT 900 Racks ////
            if (part.Description1.Substring(3, 2) == "SU")
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.GMT900Rack;
                return rack;
            }                
            //// General Motors Racks //// && combined VW/Honda/whatever else we throw into #6
            if (partNumber.StartsWith("8") || partNumber.StartsWith("6"))
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.MercedesRack;
                return rack;
            } 
            //// KIA Racks ////
            if (partNumber.StartsWith("9") && (part.PartType == "PAINT" || part.PartType == "SVASSY" || part.PartType == "SVPAINT"))
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.MercedesRack;
                return rack;
            }                
            if (partNumber.StartsWith("9") && (part.PartType != "PAINT" && part.PartType != "SVASSY" && part.PartType != "SVPAINT"))
            {
                rack = new AbstractRack(partNumber, quantity);
                rack.Type = RackType.KIARack;
                return rack;
            }   
            throw new Exception("Rack does not correspond to customers");
        }
        
        /// <summary>
        /// Create a rack using an already existing Rack ID
        /// </summary>
        /// <param name="rackID">Rack ID used to populate the data.</param>
        /// <returns>Gives back a Rack Object</returns>
        public static AbstractRack Make(int rackID)
        {
            Database database = new Database();
            string partNumber = string.Empty;

            SqlCommand command = new SqlCommand();
            command.CommandText = "select rack_id from RackRecords.dbo.BMW_F25_Rack_XRef where bmw_rack_id = @rackID";
            command.Parameters.AddWithValue("@rackID", rackID);

            database.OpenConnection();
            SqlDataReader dr = database.RunCommandReturnReader(command);

            if (dr.Read())
            {
                rackID = Int32.Parse(dr["rack_id"].ToString());
            }
            database.CloseConnection();

            dr.Dispose();
            command.Dispose();

            try
            {
                command = new SqlCommand();
                command.CommandText = "SELECT part_number from Production.dbo.rackhistory where rack_id = @rackID";
                command.Parameters.AddWithValue("@rackID", rackID);
                database.OpenConnection();
                dr = database.RunCommandReturnReader(command);
                if (dr.Read())
                {
                    partNumber = dr["part_number"].ToString();
                } 
                database.CloseConnection();
                command.Dispose();
                dr.Dispose();
            }
            catch (Exception e)
            {
                database.CloseConnection();
                throw e;  
            }

            sPart part = new Part().GetPartByPartNumber(partNumber);
            AbstractRack rack;

            //// MIRROR Rack ////
            if (part.Description1.Substring(6, 1) == "U" || part.Description1.PadRight(9, ' ').Substring(0, 9) == "02F25 FRU" || part.Description1.PadRight(9, ' ').Substring(0, 9) == "02F25 FFU")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.BMWE7Rack;
                return rack;
            }
            //// MSIG Racks ////
            if (partNumber.StartsWith("1") || partNumber.StartsWith("2") || partNumber.StartsWith("7"))
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.MercedesRack;
                return rack;
            }
            //// EZGO Racks ////
            if (partNumber.StartsWith("3"))
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.EZGoRack;
                return rack;
            }
            //// Club Car Racks ////
            if (partNumber.StartsWith("4"))
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.CCRack;
                return rack;
            }
            //// BMW Z85 Racks ////
            if (part.Description1.Substring(2, 3) == "Z85" || part.Description1.Substring(2, 3) == "M85")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.BMWRack;
                return rack;
            }
            //// BMW E70/E71 Service Racks
            if ((part.Description1.Substring(2, 3) == "M70" || part.Description1.Substring(2, 3) == "M71") && part.PartType == "SVFG")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.MercedesRack; //this is intentional. MercedesRack is the default rack format, since we started out making mercedes parts.
                return rack;
            }
            //// BMW E70/E71 Racks
            if (part.Description1.Substring(2, 3) == "M70" || part.Description1.Substring(2, 3) == "M71")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.BMWE7Rack;
                return rack;
            }
            //// BMW F25 Racks
            if (part.Description1.Substring(2, 3) == "M25")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.MercedesRack;
                return rack;
            }
            //// BMW F15 Racks
            if (part.Description1.Substring(2, 3) == "M15" || part.Description1.Substring(2, 3) == "F15")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.MercedesRack;
                return rack;
            }
            //// Honda Racks ////
            if (part.Description1.Substring(3, 1) == "H")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.MercedesRack;
                return rack;
            }
            //// Toyota Racks ////
            if (part.Description1.Substring(3, 2) == "T1")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.ToyotaRack;
                return rack;
            }
            //// VW Racks ////
            if (part.Description1.Substring(3, 2) == "VW")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.MercedesRack;
                return rack;
            }
            //// General Motors C6 Racks & Z06 Racks ////
            if (part.Description1.Substring(2, 3) == "1V6" || part.Description1.Substring(2, 3) == "1VZ" || part.Description1.Substring(2, 3) == "1VS")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.GMRack;
                return rack;
            }
            //// GMT 900 Racks ////
            if (part.Description1.Substring(3, 2) == "SU")
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.GMT900Rack;
                return rack;
            }
            //// General Motors Racks //// && combined VW/Honda/whatever else we throw into #6
            if (partNumber.StartsWith("8") || partNumber.StartsWith("6"))
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.MercedesRack;
                return rack;
            }
            //// KIA Racks ////
            if (partNumber.StartsWith("9") && (part.PartType == "PAINT" || part.PartType == "SVASSY" || part.PartType == "SVPAINT"))
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.MercedesRack;
                return rack;
            }
            //// Kia SVFG and FG
            if (partNumber.StartsWith("9") && (part.PartType != "PAINT" && part.PartType != "SVASSY" && part.PartType != "SVPAINT"))
            {
                rack = new AbstractRack(rackID);
                rack.Type = RackType.KIARack;
                return rack;
            }
            throw new Exception("Rack does not correspond to customers");            
        }

        /// <summary>
        /// Create a rack using an already existing Rack ID
        /// </summary>
        /// <param name="rackID">Rack ID used to populate the data.</param>
        /// <param name="rackType">Type of rack, determining table used for lookup.</param>
        /// <returns>Gives back a Rack Object</returns>
        public static AbstractRack Make(int rackID, string rackType)
        {
            if (rackType == "RCK")
            {
                return Make(rackID);
            }
            else if (rackType == "MES")
            {
                Database database = new Database();                
                string partNumber = string.Empty;

                try
                {
                    SqlCommand command = new SqlCommand();
                    command.CommandText = "SELECT mc_part_number FROM mes.dbo.m_container WHERE mc_id = " + rackID;
                    SqlDataReader dr = database.RunCommandReturnReader(command);
                    dr.Read();
                    partNumber = dr["mc_part_number"].ToString();
                }
                catch (InvalidOperationException)
                {
                    throw new Exception("No record found for Rack: " + rackID);
                }

                sPart part = new Part().GetPartByPartNumber(partNumber);

                AbstractRack rack;

                //// MIRROR Rack ////
                if (part.Description1.Substring(6, 1) == "U" || part.Description1.PadRight(9, ' ').Substring(0, 9) == "02F25 FRU" || part.Description1.PadRight(9, ' ').Substring(0, 9) == "02F25 FFU")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.BMWE7Rack;
                    return rack;
                }
                //// MSIG Racks ////
                if (partNumber.StartsWith("1") || partNumber.StartsWith("2") || partNumber.StartsWith("7"))
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.MercedesRack;
                    return rack;
                }
                //// EZGO Racks ////
                if (partNumber.StartsWith("3"))
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.EZGoRack;
                    return rack;
                }
                //// Club Car Racks ////
                if (partNumber.StartsWith("4"))
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.CCRack;
                    return rack;
                }
                //// BMW Z85 Racks ////
                if (part.Description1.Substring(2, 3) == "Z85" || part.Description1.Substring(2, 3) == "M85")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.BMWRack;
                    return rack;
                }
                //// BMW E70/E71 Service Racks
                if ((part.Description1.Substring(2, 3) == "M70" || part.Description1.Substring(2, 3) == "M71") && part.PartType == "SVFG")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.MercedesRack; //this is intentional. MercedesRack is the default rack format, since we started out making mercedes parts.
                    return rack;
                }
                //// BMW E70/E71 Racks
                if (part.Description1.Substring(2, 3) == "M70" || part.Description1.Substring(2, 3) == "M71")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.BMWE7Rack;
                    return rack;
                }
                //// BMW F25 Racks
                if (part.Description1.Substring(2, 3) == "M25")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.MercedesRack;
                    return rack;
                }
                //// BMW F15 Racks
                if (part.Description1.Substring(2, 3) == "M15" || part.Description1.Substring(2, 3) == "F15")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.MercedesRack;
                    return rack;
                }
                //// Honda Racks ////
                if (part.Description1.Substring(3, 1) == "H")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.MercedesRack;
                    return rack;
                }
                //// Toyota Racks ////
                if (part.Description1.Substring(3, 2) == "T1")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.ToyotaRack;
                    return rack;
                }
                //// VW Racks ////
                if (part.Description1.Substring(3, 2) == "VW")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.MercedesRack;
                    return rack;
                }
                //// General Motors C6 Racks & Z06 Racks ////
                if (part.Description1.Substring(2, 3) == "1V6" || part.Description1.Substring(2, 3) == "1VZ" || part.Description1.Substring(2, 3) == "1VS")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.GMRack;
                    return rack;
                }
                //// GMT 900 Racks ////
                if (part.Description1.Substring(3, 2) == "SU")
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.GMT900Rack;
                    return rack;
                }
                //// General Motors Racks //// && combined VW/Honda/whatever else we throw into #6
                if (partNumber.StartsWith("8") || partNumber.StartsWith("6"))
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.MercedesRack;
                    return rack;
                }
                //// KIA Racks ////
                if (partNumber.StartsWith("9") && (part.PartType == "PAINT" || part.PartType == "SVASSY" || part.PartType == "SVPAINT"))
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.MercedesRack;
                    return rack;
                }
                if (partNumber.StartsWith("9") && (part.PartType != "PAINT" && part.PartType != "SVASSY" && part.PartType != "SVPAINT"))
                {
                    rack = new AbstractRack(rackID, rackType);
                    rack.Type = RackType.KIARack;
                    return rack;
                }
                throw new Exception("Rack does not correspond to customers");    
            }
            else
            {
                throw new Exception("Unhandled rack type; not RCK or MES");
            }
        }

        //public static AbstractRack SelectRackType(sPart part)
        //{
        //    AbstractRack rack;

        //    //// MIRROR Rack ////
        //    if (part.Description1.Substring(6, 1) == "U" || part.Description1.PadRight(9, ' ').Substring(0, 9) == "02F25 FRU" || part.Description1.PadRight(9, ' ').Substring(0, 9) == "02F25 FFU")
        //    {
        //        rack = new AbstractRack(partNumber, quantity, shipLocation);
        //        rack.Type = RackType.BMWE7Rack;
        //        return rack;
        //    }
        //    //// MSIG Racks ////
        //    if (partNumber.StartsWith("1") || partNumber.StartsWith("2") || partNumber.StartsWith("7"))
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.MercedesRack;
        //        return rack;
        //    }
        //    //// EZGO Racks ////
        //    if (partNumber.StartsWith("3"))
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.EZGoRack;
        //        return rack;
        //    }
        //    //// Club Car Racks ////
        //    if (partNumber.StartsWith("4"))
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.CCRack;
        //        return rack;
        //    }
        //    //// BMW Z85 Racks ////
        //    if (part.Description1.Substring(2, 3) == "Z85" || part.Description1.Substring(2, 3) == "M85")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.BMWRack;
        //        return rack;
        //    }
        //    //// BMW E70/E71 Service Racks
        //    if ((part.Description1.Substring(2, 3) == "M70" || part.Description1.Substring(2, 3) == "M71") && part.PartType == "SVFG")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.MercedesRack; //this is intentional. MercedesRack is the default rack format, since we started out making mercedes parts.
        //        return rack;
        //    }
        //    //// BMW E70/E71 Racks
        //    if (part.Description1.Substring(2, 3) == "M70" || part.Description1.Substring(2, 3) == "M71")
        //    {
        //        rack = new AbstractRack(partNumber, quantity, shipLocation);
        //        rack.Type = RackType.BMWE7Rack;
        //        return rack;
        //    }
        //    //// BMW F25 Racks
        //    if (part.Description1.Substring(2, 3) == "M25")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.MercedesRack;
        //        return rack;
        //    }
        //    //// BMW F15 Racks
        //    if (part.Description1.Substring(2, 3) == "M15" || part.Description1.Substring(2, 3) == "F15")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.MercedesRack;
        //        return rack;
        //    }
        //    //// Honda Racks ////
        //    if (part.Description1.Substring(3, 1) == "H")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.MercedesRack;
        //        return rack;
        //    }
        //    //// Toyota Racks ////
        //    if (part.Description1.Substring(3, 2) == "T1")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.ToyotaRack;
        //        return rack;
        //    }
        //    //// VW Racks ////
        //    if (part.Description1.Substring(3, 2) == "VW")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.MercedesRack;
        //        return rack;
        //    }
        //    //// General Motors C6 Racks & Z06 Racks ////
        //    if (part.Description1.Substring(2, 3) == "1V6" || part.Description1.Substring(2, 3) == "1VZ" || part.Description1.Substring(2, 3) == "1VS")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.GMRack;
        //        return rack;
        //    }
        //    //// GMT 900 Racks ////
        //    if (part.Description1.Substring(3, 2) == "SU")
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.GMT900Rack;
        //        return rack;
        //    }
        //    //// General Motors Racks //// && combined VW/Honda/whatever else we throw into #6
        //    if (partNumber.StartsWith("8") || partNumber.StartsWith("6"))
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.MercedesRack;
        //        return rack;
        //    }
        //    //// KIA Racks ////
        //    if (partNumber.StartsWith("9") && (part.PartType == "PAINT" || part.PartType == "SVASSY" || part.PartType == "SVPAINT"))
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.MercedesRack;
        //        return rack;
        //    }
        //    if (partNumber.StartsWith("9") && (part.PartType != "PAINT" && part.PartType != "SVASSY" && part.PartType != "SVPAINT"))
        //    {
        //        rack = new AbstractRack(partNumber, quantity);
        //        rack.Type = RackType.KIARack;
        //        return rack;
        //    }
        //    throw new Exception("Rack does not correspond to customers");
        //}

        /// <summary>
        /// Method to validate that scan is a valid rack label
        /// </summary>
        /// <param name="rack"></param>
        /// <returns>Boolean</returns>
        public static bool IsValidRack(string rack)
        {
            if (rack.Contains("|RCK|"))
                return true;
            if (rack.StartsWith("|MES|"))
                return true;
            if (rack.Contains("SDQ"))
                return true;            
            if (rack.Contains("S"))
                return true;
            if (rack.StartsWith("772"))
                return true;
            if (rack.StartsWith("1JUN129357955"))
                return true;
            
            return false;
        }

        public static AbstractRack OpenPartial(string paintLabel)
        {
            Database database = new Database();
            SqlCommand command = new SqlCommand();
            AbstractRack rack;

            try
            {    
                if (RackFactory.IsValidRack(paintLabel))
                {
                    //Assume scan is a rack label, and open Rack
                    paintLabel = RackFactory.StripRack(paintLabel);
                    rack = RackFactory.Make(Int32.Parse(paintLabel));
                }
                else
                {
                    //Assume scan is paintLabel, attempt to find rack paintlabel is on, and open that
                    int rackID;

                    command.CommandText = "Select rack_id FROM [RackRecords].[dbo].[Paint_Rack_XRef] where paintscan = @paintLabel";
                    command.Parameters.AddWithValue("@paintLabel", paintLabel);

                    SqlDataReader reader = database.RunCommandReturnReader(command);

                    if (reader.HasRows == false)
                    {
                        throw new Exception("Paint Label Not associated with a Rack");
                    }
                    else
                    {
                        reader.Read();
                        rackID = Int32.Parse(reader["rack_id"].ToString());
                        rack = RackFactory.Make(rackID);
                    }
                    database.CloseConnection();
                }
            }
            catch (Exception)
            {
                //throw e;
                return null;
            }

            return rack;
        }

        /// <summary>
        /// Method to strip out rack identifiers, leaving the rack #
        /// </summary>
        /// <param name="scan"></param>
        /// <returns></returns>
        public static string StripRack(string scan)
        {
            return scan.Replace("|RCK|", "").Replace("|MES|", "").Replace("SDQ", "").Replace("S", "").Replace("1JUN129357955", "").Replace("|WIP|", "");
        }
    }
}
