using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

using System.Text.RegularExpressions;

namespace DecoAPI.DDCService
{
    public class VWServiceUnitLabel
    {
        string pt_part, pt_desc1, pt_desc2, cp_cust_part;
        Message message;

        public VWServiceUnitLabel(Message message)
        {
            this.message = message;

            //If we don't have a moldpartnumber, we go get it
            //Otherwise, we check that the mold label scanned is the same part number as the box we're building
            if (message.MoldPartNumber == null || message.MoldPartNumber == "")
            {
                message.MoldPartNumber = Unload.RackCreator.GetMoldPartNumber(message.MoldLabel);
            }
            else
            {
                string temp = Unload.RackCreator.GetMoldPartNumber(message.MoldLabel);
                
                if (temp != message.MoldPartNumber)
                {
                    throw new Exception("Part Does Not match other parts in Box!");
                }
            }
            
            GetDataByMoldPartNumber();
        }

        public void GetMaxRackQuantity(string svfgPartNumber)
        {
            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();

            command.CommandText = "Select quantity from Production.dbo.Max_Rack_Quantity Where partnumber = @partnumber";
            command.Parameters.AddWithValue("@partnumber", svfgPartNumber);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                message.MaxRackQuantity = Int32.Parse(reader["quantity"].ToString());
            }
            else
            {
                throw new Exception("Failed to Determine Max Rack Quantity");
            }
        }

        public void GetDataByMoldPartNumber()
        {
            #region Fetch Data

            pt_part = "";
            pt_desc1 = "";
            pt_desc2 = "";
            cp_cust_part = ""; 

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();

            command.CommandText = "SELECT pt.pt_part, pt.pt_desc1, pt.pt_desc2, cp.cp_cust_part "
                                + "FROM Decostar.dbo.pt_mstr_sql pt "
                                + "INNER JOIN Decostar.dbo.cp_mstr_sql cp ON pt.pt_part = cp.cp_part "
                                + "WHERE pt.pt_desc1 LIKE (LEFT((SELECT pt_desc1 FROM Decostar.dbo.pt_mstr_sql WHERE pt_part = @MoldPartNumber) + space(18),18) + '%') "
                                + "AND pt.pt_part_type = 'SVFG' AND pt.pt_status = 'AC'";
            command.Parameters.AddWithValue("@MoldPartNumber", message.MoldPartNumber);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                pt_part = reader["pt_part"].ToString();
                pt_desc1 = reader["pt_desc1"].ToString();
                pt_desc2 = reader["pt_desc2"].ToString();
                cp_cust_part = reader["cp_cust_part"].ToString();
            }

            database.CloseConnection();

            var pattern = @"  +"; // match two or more spaces
            var groups = Regex.Split(cp_cust_part, pattern); //split string on multiple spaces
            var tokens = groups.Select(group => group.Replace(" ", String.Empty)); //remove all single spaces
            cp_cust_part = String.Join(" ", tokens.ToArray()); //rebuild string with single spaces where double spaces used to be

            #endregion

            //Get Max Rack Quantity
            GetMaxRackQuantity(pt_part);

            //Print Unit Label
            FormLabel(message.UnitPrinter);
            message.Notification = "Unit Label Printed";
            message.Quantity++;

            //If we've reached the Maximum Rack Quantity, we also print a box label, and then clear message quantity
            if (message.Quantity == message.MaxRackQuantity)
            {
                FormLabel(message.BoxPrinter, message.Quantity);
                message.Quantity = 0;
                message.MoldPartNumber = "";
                message.Notification = "Box Label Printed";                
            }
        }        

        public void FormLabel(string printer, int quantity = 1)
        {
            string lot = "", exipry = "";
            
            //Apparently you can set the quantity of labels to print in ipl
            //my printhandler class does that anyways, but I've left it it rather than try to manipulate raw ipl

            string ipl = "";
            ipl += "<STX>R<ETX>";
            ipl += "<STX><ESC>k<ETX>";
            ipl += "<STX><ESC>C0<ETX>";
            ipl += "<STX><ESC>P;<ETX>";
            ipl += "<STX>E*;F*;<ETX>";
            ipl += "<STX>L39;D0;<ETX>";
            ipl += "<STX>B0,PartNo  ;o301,128;f3;h80;w2;c6;i0;d3," + cp_cust_part + ";D39;<ETX>";
            ipl += "<STX>H1,Country ;o67,136;f3;h11;w11;c26;r0;b0;d3,Made in USA;<ETX>";
            ipl += "<STX>H2,Lot     ;o94,137;f3;h11;w11;c26;r0;b0;d3,Lot: " + lot +";<ETX>";
            ipl += "<STX>H3,Expire  ;o122,388;f3;h11;w11;c26;r0;b0;d3," + exipry + ";<ETX>";
            ipl += "<STX>H4,ExpireLa;o121,136;f3;h11;w11;c26;r0;b0;d3,Expire date:;<ETX>";
            ipl += "<STX>H5,PDate   ;o149,294;f3;h11;w11;c26;r0;b0;d3,PP " + DateTime.Now.Date.ToString("dd.MM.yyyy") + ";<ETX>";
            ipl += "<STX>H6,Count   ;o148,136;f3;h11;w11;c26;r0;b0;d3," + quantity + " pieces(s)" + ";<ETX>";
            ipl += "<STX>H7,Descr   ;o176,137;f3;h11;w11;c26;r0;b0;d3," + pt_desc2 + ";<ETX>";
            ipl += "<STX>H8,PartDSP ;o200,137;f3;h11;w11;c26;r0;b0;d3," + cp_cust_part + ";<ETX>";
            ipl += "<STX>R<ETX>";
            ipl += "<STX>R<ETX>";
            ipl += "<STX><ESC>k<ETX>";
            ipl += "<STX><ESC>C0<LF>";
            ipl += "<SI>g1,567<LF>";
            //ipl += "<SI>g0,420<LF>";
            ipl += "<SI>T1<LF>";
            ipl += "<SI>R1<SI>r0<LF>";
            ipl += "<SI>D0<LF>";
            ipl += "<SI>c0<SI>t0<LF>";
            ipl += "<SI>l0<LF>";
            ipl += "<SI>F2<LF>";
            ipl += "<SI>f0<LF>";
            ipl += "<SI>L1000<LF>";
            ipl += "<SI>I5<LF>";
            ipl += "<SI>i0<LF>";
            ipl += "<SI>W609;<SI>S40<LF>";
            ipl += "<SI>d0<LF>";
            ipl += "<ETX><LF>";
            ipl += "<STX><ESC>E*<CAN><ETX>";
            ipl += "<STX><RS>1<ETX><STX><US>" + "1" + "<ETX><STX><ETB><ETX>";
            ipl += "<STX><FF><ETX>";

            Printers.PrintHandler ph = new Printers.PrintHandler(printer);
            if (ph.SendToPrinter(ipl) == false)
            {
                throw new Exception("Could not ping printer");
            }            
        }
    }
}