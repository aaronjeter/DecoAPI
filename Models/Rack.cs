/*This is the rewrite of DeoCore.MES.Rack.cs */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace DecoAPI.Models
{
    /// <summary>
    /// This class is mainly used by the Warehouse application; to move racks between warehouses in the plant.
    /// </summary>
    public static class Rack
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RackID"></param>
        /// <returns></returns>
        public static sRack GetRack(int RackID)
        {
            sRack returnRack = new sRack();
            Database database = new Database();
            if (database.OpenConnection())
            {
                SqlCommand command = new SqlCommand();
                command.CommandText = "SELECT * FROM Production.dbo.RackHistory WHERE Rack_ID = @rackID";
                command.Parameters.AddWithValue("@rackID", RackID.ToString());
                SqlDataReader dr = database.RunCommandReturnReader(command);

                if (dr.Read())
                {
                    returnRack.id = Int32.Parse(dr["Rack_ID"].ToString());
                    returnRack.PartNumber = dr["Part_Number"].ToString();
                    returnRack.EntryDate = dr["Entry_Date"].ToString();
                    returnRack.StyleID = Int32.Parse(dr["Rack_Style_ID"].ToString());
                    returnRack.Quantity = Int32.Parse(dr["Quantity"].ToString());
                    returnRack.Complete = (bool)dr["Complete"];
                    returnRack.Status = Int32.Parse(dr["Status"].ToString());
                    returnRack.LastLocation = dr["Last_Location"].ToString();

                    returnRack.Type = rackType.RIM;
                }
                else
                {                    
                    database.CloseConnection();
                    throw new Exception("Can not find the rack.");                    
                }
            }
            else
            {
                database.CloseConnection();
                throw new Exception("Database Connection Error");
            }

            // Close the db
            database.CloseConnection();

            return returnRack;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RackID"></param>
        /// <param name="LastLocation"></param>
        /// <returns></returns>
        public static sRack ChangeRackLastLoc(int RackID, string LastLocation)
        {
            sRack returnRack = new sRack();
            Database database = new Database();
            returnRack = GetRack(RackID);

            SqlCommand command = new SqlCommand();
            command.CommandText = "UPDATE Production.dbo.RackHistory SET Last_Location = @LastLocation WHERE Rack_ID = @RackId";
            command.Parameters.AddWithValue("@LastLocation", LastLocation);
            command.Parameters.AddWithValue("@RackId", RackID);

            if (database.ExecuteNonQuery(command) != 1)
            {
                throw new Exception("Could not change the status. Unknown Error in database.");
            }

            returnRack = GetRack(RackID);
            return returnRack;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="WIPRackID"></param>
        /// <returns></returns>
        public static objWIPRack GetWIPRack(Int64 WIPRackID)
        {
            objWIPRack returnRack = new objWIPRack();

            Database database = new Database();
            if (database.OpenConnection())
            {
                SqlCommand command = new SqlCommand();
                SqlDataReader DR = null;

                command.CommandText = "SELECT * FROM Production.dbo.wip_label WHERE wip_id = @WIPRackID";
                command.Parameters.AddWithValue("@WIPRackID", WIPRackID.ToString());

                DR = database.RunCommandReturnReader(command);
                if (DR.Read())
                {
                    returnRack.id = DR.GetInt64(0);
                    returnRack.PartNumber = DR.GetString(1);
                    returnRack.Quantity = DR.GetInt32(2);
                    returnRack.EntryDate = DR.GetDateTime(3).ToString();
                    returnRack.Location = DR.GetString(4);
                    string sType = DR.GetString(5);
                    if (sType.ToLower() == "wip")
                    {
                        returnRack.Type = WIPRackType.WIP;
                    }
                    else if (sType.ToLower() == "rework")
                    {
                        returnRack.Type = WIPRackType.REWORK;
                    }
                    else if (sType.ToLower() == "finesse")
                    {
                        returnRack.Type = WIPRackType.FINESSE;
                    }
                    else if (sType.ToLower() == "holdsort")
                    {
                        returnRack.Type = WIPRackType.HOLDSORT;
                    }
                    else if (sType.ToLower() == "darkcolor")
                    {
                        returnRack.Type = WIPRackType.DarkColor;
                    }
                    else if (sType.ToLower() == "lightcolor")
                    {
                        returnRack.Type = WIPRackType.LightColor;
                    }
                    else
                    {
                        returnRack.Type = WIPRackType.FINISHED;
                    }
                    returnRack.Applicaiton = DR.GetString(6);
                    try
                    {
                        returnRack.MoldTime = DR.GetDateTime(7);
                    }
                    catch (Exception)
                    {
                        returnRack.MoldTime = new DateTime();
                    }
                    returnRack.MoldSerial = DR.GetString(8);

                    //returnRack.Type = rackType.RIM;
                }
                else
                {
                    database.CloseConnection();
                    throw new Exception("Can not find the rack.");
                }
                DR.Close();
            }
            else
            {
                database.CloseConnection();
                throw new Exception("Database Connection Error");
            }

            database.CloseConnection();
            return returnRack;
        }


        /// <summary>
        /// Change the location of a WIP rack in MES
        /// </summary>
        /// <param name="wipID"></param>
        /// <param name="LastLocation"></param>
        /// <returns></returns>
        public static void ChangeWIPLastLoc(Int64 wipID, string LastLocation)
        {
            Database database = new Database();
            SqlCommand command = new SqlCommand();

            command.CommandText = "UPDATE Production.dbo.wip_label SET wip_location = @LastLocation WHERE wip_id = @wipID";
            command.Parameters.AddWithValue("@LastLocation", LastLocation);
            command.Parameters.AddWithValue("@wipID", wipID);
            int returnVal = database.ExecuteNonQuery(command);
            if (returnVal != 1)
            {
                throw new Exception("Could not change the status. Unknown Error in database.");
            }
        }
    }
    

    #region Struct(s)    
    public struct sRack
    {
        public int id;
        public string PartNumber;
        public string EntryDate;
        public int StyleID;
        public int Quantity;
        public bool Complete;
        public int Status;
        public string LastLocation;
        public rackType Type;
    }

    public struct objWIPRack
    {
        public Int64 id;
        public string PartNumber;
        public int Quantity;
        public string EntryDate;
        public string Location;
        public WIPRackType Type;
        public string Applicaiton;
        public DateTime MoldTime;
        public string MoldSerial;
    }
    #endregion


    #region Enum(s)

    /// <summary>
    /// Rack Type. Based on source of molded part
    /// </summary>
    public enum rackType
    {
        RIM,
        IMM
    }

    /// <summary>
    /// Work on Progress (WIP) Rack.
    /// </summary>
    public enum WIPRackType
    {
        WIP,
        FINISHED,
        REWORK,
        FINESSE,
        HOLDSORT,
        LightColor,
        DarkColor
    }
    #endregion
    } 