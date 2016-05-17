/* AbstractRack is the rewrite of DecoCore.Rack.Rack.cs
 * I renamed it AbstractRack due to a naming conflict with DecoCore.MES.Rack.cs
 * This class was originally implemented as an abstract class, although that fact was not being utilized. 
 */

using System;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DecoAPI.Models
{
    public class AbstractRack
    {
        #region Fields
        protected int rackID;
        protected string decostarPartNumber;
        protected string customerPartNumber;
        protected string rahauPartNumber;
        protected DateTime entryDate;
        protected string revision;
        protected int quantity;
        protected bool complete;
        protected int status;
        protected string lastLocation;
        protected string shipLocation;
        protected string purchaseOrder;
        protected RackTable table;
        protected bool serviceRack;
        protected string applciation = "DecoAPI";
        protected string sEvent = "Unspecified";
        #endregion

        #region Constructors
        /// <summary>
        /// Create a New Rack.
        /// </summary>
        /// <param name="partNumber">Part Number</param>
        /// <param name="quantity">Quantity on the Rack</param>
        public AbstractRack(string partNumber, int quantity)
        {
            this.decostarPartNumber = partNumber;
            this.quantity = quantity;
            entryDate = DateTime.Now;
            rackID = -1;

            revision = "";
            complete = true;
            status = 0;
            lastLocation = "SHIPWIP";
            shipLocation = "";
            customerPartNumber = "";
            rahauPartNumber = "";
            purchaseOrder = "";
            serviceRack = false;
            UpdateProduction();
        }        

        /// <summary>
        /// Create new rack
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="quantity"></param>
        /// <param name="shipLocation"></param>
        public AbstractRack(string partNumber, int quantity, string shipLocation)
        {
            this.decostarPartNumber = partNumber;
            this.quantity = quantity;
            entryDate = DateTime.Now;
            rackID = -1;

            revision = "";
            complete = true;
            status = 0;
            lastLocation = "SHIPWIP";
            this.shipLocation = shipLocation;
            customerPartNumber = "";
            rahauPartNumber = "";
            purchaseOrder = "";
            serviceRack = false;
            UpdateProduction();
        }        

        /// <summary>
        /// Find an already existing rack with the given identification number and
        ///  populate the rack object with that rack's information.
        /// </summary>
        /// <param name="rackID">Rack ID of existing rack</param>
        public AbstractRack(int rackID)
        {  
            this.rackID = rackID;
            
            Database database = new Database();
            SqlDataReader dr;
            SqlCommand command = new SqlCommand();
            command.CommandText = "Select part_number, application, (SELECT count(id)  FROM [RackRecords].[dbo].[Paint_Rack_XRef]  where rack_id = @rackID) as quantity, Entry_Date, status, last_location, ship_location from Production.dbo.Rackhistory where rack_id = @rackID";
            command.Parameters.AddWithValue("@rackID", rackID);

            try
            {
                database.OpenConnection();
                dr = database.RunCommandReturnReader(command);

                if (dr.Read())
                {
                    table = RackTable.RackHistory;
                    this.decostarPartNumber = dr["part_number"].ToString();
                    this.applciation = dr["application"].ToString();

                    this.quantity = Int32.Parse(dr["quantity"].ToString());
                    this.revision = "";
                    this.complete = true;
                    this.entryDate = DateTime.Parse(dr["Entry_Date"].ToString());
                    this.status = Int32.Parse(dr["status"].ToString());
                    this.lastLocation = dr["last_location"].ToString();
                    this.shipLocation = dr["ship_location"].ToString();
                    customerPartNumber = "";
                    serviceRack = false;  
                }
                else
                {
                    throw new Exception("Rack not Found");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                database.CloseConnection();
            }        
        }

        /// <summary>
        /// Find an already existing rack with the given identification number and
        ///  populate the rack object with that rack's information.
        /// </summary>
        /// <param name="rackID">Rack ID of existing rack</param>
        /// <param name="rackType">Type of rack, used to determine table used for lookup.</param>
        public AbstractRack(int rackID, string rackType)
        {
            if (rackType == "MES")
            {
                this.rackID = rackID;

                try
                {
                    Database database = new Database();
                    SqlDataReader dr;
                    SqlCommand command = new SqlCommand();
                    command.CommandText = "Select mc_part_number, mc_tcreation, mc_state, mc_location from MES.dbo.m_container where mc_id = @rackID";
                    command.Parameters.AddWithValue("@rackID", rackID);
                    
                    dr = database.RunCommandReturnReader(command);

                    if(dr.Read())
                    {
                        table = RackTable.m_container;
                        this.decostarPartNumber = dr["mc_part_number"].ToString();

                        this.revision = "";
                        this.complete = true;
                        this.entryDate = DateTime.Parse(dr["mc_tcreation"].ToString());
                        this.status = Int32.Parse(dr["mc_state"].ToString());
                        this.lastLocation = dr["mc_location"].ToString();
                        this.shipLocation = "";
                        customerPartNumber = "";
                        serviceRack = false;
                    }
                    dr.Close();
                    
                    // Do a count query to retrieve the current quantity for the given rack number
                    using (SqlConnection conn = new SqlConnection("Server=decosql02;Initial Catalog=mes;Persist Security Info=True;User ID=PartHistoryUser;Password=PartHistoryUser"))
                    {
                        command = new SqlCommand();
                        command.CommandText = "Select COUNT(mce_id) as PartsInRack From mes.dbo.m_container_entry Where mce_container_id = @rackID";
                        command.Parameters.AddWithValue("@rackID", rackID);
                        command.Connection = conn;
                        command.Connection.Open();
                        dr = database.RunCommandReturnReader(command);
                        dr.Read();
                        this.quantity = dr.GetInt32(0);
                        command.Connection.Close();
                    }
                }
                catch
                {
                    throw new Exception("Could not find the rack");
                }
            }
            else
            {
                throw new Exception("Rack type not recognized!");
            }
        }                
        #endregion

        #region Methods
        private void InitializeRevision()
        {
            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT pt.pt_rev FROM Decostar.dbo.pt_mstr_sql pt WHERE pt.pt_part = @decostarPartNumber";
            command.Parameters.AddWithValue("@decostarPartNumber", decostarPartNumber);

            database.OpenConnection();
            SqlDataReader DR = database.RunCommandReturnReader(command);

            if (DR.Read())
            {
                revision = DR["pt_rev"].ToString(); 
                DR.Close();
            }
            else
            {
                database.CloseConnection();
                throw new Exception("Could not find Revision Value.");
            }

            database.CloseConnection();
        }

        private void UpdateProduction()
        {
            Database database = new Database();
            //database.Open(DBType.SQL_Production);
            string theRackType = "";
            theRackType = "Production.dbo.sp_CreateRack_v2";
            
            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = theRackType;

            command.Parameters.AddWithValue("@Rack_Style_Id", -1);
            command.Parameters.AddWithValue("@Part_Number", decostarPartNumber);
            command.Parameters.AddWithValue("@Quantity", quantity);
            command.Parameters.AddWithValue("@Last_Location", lastLocation);
            command.Parameters.AddWithValue("@Ship_Location", shipLocation);                

            SqlParameter returnValue = new SqlParameter("@Return_Value", System.Data.SqlDbType.BigInt);
            returnValue.Direction = System.Data.ParameterDirection.ReturnValue;
            command.Parameters.Add(returnValue);

            database.ExecuteNonQuery(command);
            rackID = Int32.Parse(returnValue.Value.ToString());                

            command.Dispose();
            try
            {
                command = new SqlCommand();
                command.CommandText = "Production.dbo.InsertRackIntoHistory";
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Rack_ID", rackID);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@LastLocation", lastLocation);
                command.Parameters.AddWithValue("@Ship_Location", shipLocation);
                command.Parameters.AddWithValue("@Event", applciation);

                database.ExecuteNonQuery(command);
            }
            catch (Exception e)
            {
                throw e;
            }            
        }

        private void InitializeCP()
        {
            Database database = new Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT cp_cust_part FROM Decostar.dbo.cp_mstr_sql WHERE cp_part = @decostarPartNumber";
            command.Parameters.AddWithValue("@decostarPartNumber", decostarPartNumber);

            SqlDataReader dr = database.RunCommandReturnReader(command);
            if (dr.Read())
            {
                customerPartNumber = dr["cp_cust_part"].ToString();
            }
            else
            {
                throw new Exception("Cannot find Customer Part Number!");
            }

            dr.Dispose();
            command.Dispose();

            // Special stuff for GM -> Must find EXACT customer number
            if (decostarPartNumber.StartsWith("8"))
            {
                try
                {
                    command = new SqlCommand();
                    command.CommandText = "SELECT customer_description FROM RackRecords.dbo.Customer_Descriptions where customer_description = @shipLocation";
                    command.Parameters.AddWithValue("@shipLocation", shipLocation);
                    dr = database.RunCommandReturnReader(command);
                    string customer_description = dr["customer_description"].ToString();
                    database.CloseConnection();

                    SetCustomerPart(customer_description);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private void InitializeRP()
        {
            // Really, the rahau part number should be different than the customer
            //  part number. However, we're using the same table to store both.
            //  This means that if we use the rahau part number for anything, we 
            //  cannot store the customer part number as well. In the future, it
            //  would be a good idea to create a separate storage area for the
            //  rahau part number. In the meantime, there are two separate 
            //  variables to store each one, but they pull from the same place.
            InitializeCP();
            rahauPartNumber = customerPartNumber;
        }
        private void UpdateRack()
        {
            int iComplete = 0;
            if (complete) iComplete = 1;
            
            Database database = new Database();
            try
            {
                SqlCommand command = new SqlCommand();
                command.CommandText = " update Production.dbo." + table.ToString() + " "
                                    + " set "
                                    + " quantity = @quantity, complete = @complete, status = @status, last_location = @lastLocation, ship_location = @shipLocation "
                                    + " where rack_id = @rackID";

                command.Parameters.AddWithValue("@quantity", quantity);
                command.Parameters.AddWithValue("@complete", iComplete);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@lastLocation", lastLocation);
                command.Parameters.AddWithValue("@shipLocation", shipLocation);
                command.Parameters.AddWithValue("@rackID", rackID);
                
                database.ExecuteNonQuery(command);
                command.Dispose();

                //
                command = new SqlCommand();
                command.CommandText = "Insert into Production.dbo.history values (@rackID, GetDate(), @quantity, @status, @lastLocation, @shipLocation, @event)";

                command.Parameters.AddWithValue("@rackID", rackID);
                command.Parameters.AddWithValue("@quantity", quantity);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@lastLocation", lastLocation);
                command.Parameters.AddWithValue("@shipLocation", shipLocation);
                command.Parameters.AddWithValue("@event", sEvent);

                database.ExecuteNonQuery(command);                    
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateApplication()
        {
            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "Update Production.dbo.RackHistory set Application = @application where Rack_ID = @rackID";
            command.Parameters.AddWithValue("@application", applciation);
            command.Parameters.AddWithValue("@rackID", rackID);
            database.ExecuteNonQuery(command);
        }
        #endregion        

        #region Public Methods
        public void SetCustomerPart(string customerNumber)
        {
            Database database = new Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT cp_cust_part FROM Decostar.dbo.cp_mstr_sql " +
                            "WHERE cp_part = @decostarPartNumber and cp_cust = @customerNumber";
            command.Parameters.AddWithValue("@decostarPartNumber", decostarPartNumber);
            command.Parameters.AddWithValue("@customerNumber", customerNumber);

            SqlDataReader dr = database.RunCommandReturnReader(command);            
            customerPartNumber = dr["cp_cust_part"].ToString();

            database.CloseConnection();
        }

        public List<string> GetPaintLabels()
        {
            List<string> labels = new List<string>();

            Database database = new Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "Select paintscan FROM RackRecords.dbo.Paint_Rack_XRef Where rack_id = @rack_id";
            command.Parameters.AddWithValue("@rack_id", RackID);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                labels.Add(reader.GetString(0));
            }

            database.CloseConnection();

            return labels;
        }
        #endregion

        #region Properties
        public int RackID
        {
            get
            {
                return rackID;
            }
        }
        public string PartNumber
        {
            get
            {
                return decostarPartNumber;
            }
        }
        public string CustomerPartNumber
        {
            get
            {
                if (customerPartNumber == "")
                    InitializeCP();
                return customerPartNumber;
            }
        }
        public string RahauPartNumber
        {
            get
            {
                if (rahauPartNumber == "")
                    InitializeRP();
                return rahauPartNumber;
            }
        }
        public DateTime EntryDate
        {
            get
            {
                return entryDate;
            }
        }
        public string Revision
        {
            get
            {
                if (revision == "")
                {
                    InitializeRevision();
                }
                return revision;
            }
        }
        public int Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                if (value > 999)
                    throw new Exception("Quantity too large.");
                else
                    quantity = value;
                UpdateRack();
            }
        }
        public bool Complete
        {
            get
            {
                return complete;
            }
            set
            {
                complete = value;
                UpdateRack();
            }
        }
        public int Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                UpdateRack();
            }
        }
        public string LastLocation
        {
            get
            {
                return lastLocation;
            }
            set
            {
                lastLocation = value;
                UpdateRack();
            }
        }
        public string ShipLocation
        {
            get
            {
                return shipLocation;
            }
            set
            {
                shipLocation = value;
                UpdateRack();
            }
        }
        /// <summary>
        /// Accessor/Mutator for Service Rack boolean
        /// </summary>
        public bool ServiceRack
        {
            get
            {
                return serviceRack;
            }
            set
            {
                serviceRack = value;
            }
        }
        /// <summary>
        /// Overwrites RAN
        /// </summary>
        public string PurchaseOrder
        {
            get
            {
                return purchaseOrder;
            }
            set
            {
                purchaseOrder = value;
                if (purchaseOrder.Length > 99)
                    throw new Exception("Purchase Order cannot be more than seven characters");
            }
        }
        /// <summary>
        /// Overwrites PurchaseOrder
        /// </summary>
        public string RAN
        {
            get
            {
                return purchaseOrder;
            }
            set
            {
                purchaseOrder = value;
            }
        }
        public string Application
        {
            get
            {
                return applciation;
            }
            set
            {
                applciation = value;
                UpdateApplication();
            }
        }
        public string Event
        {
            set
            {
                sEvent = value;
            }
        }

        public RackType Type {get; set;}
        #endregion
    }

    #region Enums
    public enum RackTable
    {
        RackHistory,
        RackHistoryIMM,
        m_container,
        m_container_entry
    }

    public enum RackType
    {
        BMWE7Rack,
        EZGoRack,
        CCRack,
        BMWRack,
        MercedesRack,
        ToyotaRack,
        GMRack,
        GMT900Rack,
        KIARack,
        NissanRack,
        VWRack,
        GMRackDDC
    }
    #endregion
}
