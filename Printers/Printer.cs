/****************************************************************************
 * DecoAPI.Print.Printer (Originally DecoCore)
 * 
 * A Printer object is created to allow easy access to print labels.
 * 
 * This is an abstract class that is extended by various specific print
 *  implementations. This class is responsible for handling the
 *  constructor of the various print classes as well as the print() 
 *  method which determines whether to call the printIPL() or printZPL()
 *  methods (implemented by its children). 
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DecoAPI.Printers
{
    public abstract class Printer
    {
        #region Fields
        protected string name = string.Empty;
        protected string sIP = string.Empty;
        protected string alias = string.Empty;
        protected string sID = string.Empty;
        protected string application = string.Empty;
        protected string DPI = string.Empty;
        protected string sPO = string.Empty;
        protected string location = string.Empty;
        protected Language ePL;
        protected Models.AbstractRack rack;
        public Address address;

        #endregion

        #region Constructor

        public Printer(string ID, string application, Models.AbstractRack rack)
        {
            this.sID = ID;
            this.application = application;
            this.sPO = "";
            populateFields();
            if (ID.Contains("|PRN|")) ePL = Language.IPL;
            if (ID.Contains("|ZPL|")) ePL = Language.ZPL;
            this.rack = rack;
            this.rack.Application = application;
            GetDefaultAddress();
        }

        #endregion

        #region Functions

        public bool isValid()
        {
            if (sID.Contains("|PRN|")) return true;
            if (sID.Contains("|ZPL|")) return true;
            return false;
        }
        
        public void print()
        {
            if (ePL == Language.IPL || ePL == null)
            {
                printIPL(rack);
            }
            else if (ePL == Language.ZPL)
            {
                printZPL(rack);
            }
            else
            {
                throw new Exception("Printer Language not set");
            }
        }
        public void print(Models.AbstractRack oRack)
        {
            if (ePL == Language.IPL || ePL == null)
            {
                printIPL(oRack);
            }
            else if (ePL == Language.ZPL)
            {
                printZPL(oRack);
            }
            else
            {
                throw new Exception("Printer Language not set");
            }
        }
        // This method is used to find the unqualified
        //  ID on the scanned printer. That is, to find
        //  the ID without the |PRN| or |ZPL| prefix.
        private string unqualifiedID()
        {
            return sID.Replace("|PRN|", "").Replace("|ZPL|", "");
        }

        #endregion

        #region Abstract Functions

        protected abstract void printIPL(object label);

        protected abstract void printZPL(object label);

        #endregion

        #region Properties

        public string IP {get; set;}

        public string Alias {get; set;} 

        public string ID {get; set;}

        public string Name {get; set;}

        public string PO {get {return sPO;} set{ sPO = value.Replace("P", "");}}

        public Language PL {get; set;}

        public Address Addr {get; set;}

        # endregion

        #region Back End

        protected void populateFields()
        {
            try
            {
                Models.Database database = new Models.Database();
                string query = " SELECT printer_id, device_path, ip_address, alias, dpi FROM Print_Service.dbo.PrinterPaths WHERE printer_id = " + unqualifiedID();
                SqlDataReader dr = database.RunCommandReturnReader(query);
                
                if (dr.Read())
                {                    
                    name = dr["device_path"].ToString();
                    sIP = dr["ip_address"].ToString();
                    alias = dr["alias"].ToString();
                    DPI = dr["dpi"].ToString();                    
                }

                database.CloseConnection();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void GetDefaultAddress()
        {
            string customer = string.Empty;

            if (rack.Type == Models.RackType.GMRack || rack.Type == Models.RackType.GMRackDDC)
            {
                customer = "GM";
            }
            else
            {
                return;
            }

            if (rack.ShipLocation == string.Empty)
                rack.ShipLocation = "Bowling";

            if (rack.ServiceRack)
                return;


            string query = "select * from RackRecords.dbo.ShippingAddresses where Customer = '" + customer + "' and ship_location_txt = '" + rack.ShipLocation + "'";

            Models.Database database = new Models.Database();
            SqlDataReader reader = database.RunCommandReturnReader(query);
            reader.Read();
            
            address.ID = Int32.Parse(reader["Address_ID"].ToString());
            address.Customer = reader["Customer"].ToString();
            address.Description = reader["Description"].ToString();
            address.Line1 = reader["Line_1"].ToString();
            address.Line2 = reader["Line_2"].ToString();
            address.Line3 = reader["Line_3"].ToString();
            address.Line4 = reader["Line_4"].ToString();
            address.ShipLocation = reader["Ship_Location_TXT"].ToString();
            address.TempPo = reader["PO"].ToString();
            rack.PurchaseOrder = address.TempPo;

            database.CloseConnection();
        }

        #endregion        
    }

    /*********************************
    * TempPo is only to be used with the GMLabelPrinting App.
    *********************************/

    public struct Address
    {
        public int ID; public string Description;
        public string Line1; public string Line2;
        public string Line3; public string Line4;
        public string ShipLocation;
        public string Customer; public string TempPo;
    }

    public enum Language
    {
        IPL = 0,
        ZPL = 1,
    }
}
