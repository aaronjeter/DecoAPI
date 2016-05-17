using System;
using System.IO;
using System.Data.SqlClient;

namespace DecoAPI.Models
{
    public class Transactions
    {
        protected string transferDropLocation;
        protected string RBEDropLocation;

        public Transactions()
        {
            transferDropLocation = "\\\\decos07t\\TransferDrop\\";
            RBEDropLocation = "\\\\decos07t\\RBEDrop\\";            
        }

        /// <summary>
        /// Method to conduct Inventory Transfer
        /// </summary>
        /// <param name="transactionType"></param>
        /// <param name="partNumber"></param>
        /// <param name="quantity"></param>
        /// <param name="fromSite"></param>
        /// <param name="fromLocation"></param>
        /// <param name="toSite"></param>
        /// <param name="toLocation"></param>
        /// <param name="rackID"></param>
        /// <param name="rackQuantity"></param>
        /// <param name="application"></param>
        //public void CreateInventoryTransfer(string partNumber, int quantity, string fromSite, string fromLocation, string toSite, string toLocation, int rackID, int rackQuantity, string application)
        //{
        //    CreateInventoryTransfer("INVXX", partNumber, quantity, fromSite, fromLocation, toSite, toLocation, rackID, rackQuantity, application);
        //}
		public void CreateInventoryTransfer(string transactionType, string partNumber, int quantity, string fromSite, string fromLocation, string toSite, string toLocation, int rackID, int rackQuantity, string application)
		{
            CreateDropFile(true, transferDropLocation, transactionType, partNumber, quantity, fromSite, fromLocation, toSite, toLocation);           

            // record transation in database           
            SqlCommand command = new SqlCommand();

            command.CommandText = "insert into " +
                                  "RackRecords.dbo.Transaction_History (Transaction_Time, History_Part_Number, Item_ID, Item_Quantity, Transaction_Type, " +
                                  "From_Location, To_Location, Transaction_Quantity, Application) " +
                                  "values (GetDate(), @partNumber, @rackID, @rackQuantity, 'Transfer', @fromSitefromLocation, @toSitetoLocation, @quantity, @application)";

            command.Parameters.AddWithValue("@partNumber", partNumber);
            command.Parameters.AddWithValue("@rackID", rackID);
            command.Parameters.AddWithValue("@rackQuantity", rackQuantity);
            command.Parameters.AddWithValue("@fromSitefromLocation", fromSite + fromLocation);
            command.Parameters.AddWithValue("@toSitetoLocation", toSite + toLocation);
            command.Parameters.AddWithValue("@quantity", quantity);
            command.Parameters.AddWithValue("@application", application);

            Database database = new Database();
            database.RunCommand(command);
		}        

        /// <summary>
        /// Method to conduct RBE transaction
        /// </summary>
        //public void CreateRBE(string partNumber, int quantity, string fromSite, string fromLocation, string toSite, string toLocation, int rackID, int rackQuantity, string application)
        //{
        //    CreateRBE("RBE", partNumber, quantity, fromSite, fromLocation, toSite, toLocation, rackID, rackQuantity, application);
        //}
        public void CreateRBE(string transactionType, string partNumber, int quantity, string fromSite, string fromLocation, string toSite, string toLocation, int rackID, int rackQuantity, string application)
		{
            CreateDropFile(false, RBEDropLocation, transactionType, partNumber, quantity, fromSite, fromLocation, toSite, toLocation);
                                    
            // record transation in database
            SqlCommand command = new SqlCommand();

            command.CommandText = "insert into " +
                                  "RackRecords.dbo.Transaction_History (Transaction_Time, History_Part_Number, Item_ID, Item_Quantity, Transaction_Type, " +
                                  "From_Location, To_Location, Transaction_Quantity, Application) " +
                                  "values (GetDate(), @partNumber, @rackID, @rackQuantity, 'RBE', @fromSitefromLocation, @toSitetoLocation, @quantity, @application)";

            command.Parameters.AddWithValue("@partNumber", partNumber);
            command.Parameters.AddWithValue("@rackID", rackID);
            command.Parameters.AddWithValue("@rackQuantity", rackQuantity);
            command.Parameters.AddWithValue("@fromSitefromLocation", fromSite + fromLocation);
            command.Parameters.AddWithValue("@toSitetoLocation", toSite + toLocation);
            command.Parameters.AddWithValue("@quantity", quantity);
            command.Parameters.AddWithValue("@application", application);

            Database database = new Database();
            database.RunCommand(command);
		}

        /// <summary>
        /// Create dropfile for inventory transaction
        /// </summary>
        /// <param name="inv"></param>
        /// <param name="dropLocation"></param>
        /// <param name="transactionType"></param>
        /// <param name="partNumber"></param>
        /// <param name="quantity"></param>
        /// <param name="fromSite"></param>
        /// <param name="fromLocation"></param>
        /// <param name="toSite"></param>
        /// <param name="toLocation"></param>
        private void CreateDropFile(bool inv, string dropLocation, string transactionType, string partNumber, int quantity, string fromSite, string fromLocation, string toSite, string toLocation)
        {
            // protect the quantity           
            if (quantity > 9999)
            {
                throw new Exception("Quantity too large.  Must be less than 9999.");
            }
             //build file name
            Random rand = new Random();
            int randomNumber = rand.Next(9999);
            string fileName = transactionType + "-" + DateTime.Now.ToString().Replace("/", "_").Replace(":", "_").Replace(" ", "_") + randomNumber.ToString();

            System.IO.StreamWriter sw;
            sw = new StreamWriter(dropLocation + fileName, false);

            if (inv)
            {
                //build string for inventory transfer
                sw.WriteLine(partNumber + "," + quantity.ToString() + ",," + fromSite + "," + toSite + "," + fromLocation + "," + toLocation);
            }
            else
            {
                //build string for RBE
                sw.WriteLine(partNumber + "," + quantity.ToString() + "," + toSite + "," + toLocation + ",," + fromSite + "," + fromLocation);
            }

            
            sw.Close();
            if (System.IO.File.Exists(dropLocation + fileName) != true)
            {
                throw new Exception("MFG/Pro file Write Failed.");
            }
             
        }
    }
}