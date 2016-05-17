using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace DecoAPI.DDCService
{
    public class VWChromeStrip
    {
        string pt_desc2;
        string cp_cust_part;

        public VWChromeStrip(Message message)
        {
            //substring part number
            //this gets us the unpackaged part number
            string partNumber = message.MoldLabel.Substring(0, 7);
            string finishedGood = "";

            switch (partNumber)
            {
                case "6360204":
                    finishedGood = "6367016";
                    break;
                case "6360305":
                    finishedGood = "6367017";
                    break;
                case "6360206":
                    finishedGood = "6367018";
                    break;
                case "6360307":
                    finishedGood = "6367019";
                    break;
                case "6360002":
                    finishedGood = "6367020";
                    break;
                default:
                    finishedGood = "Coke Can";
                    break;                
            }

            //Get Packaged Part Number

            #region Fetch Data

            
            pt_desc2 = "";
            cp_cust_part = "";

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();

            command.CommandText = "SELECT cp_cust_part, cp_comment "
                                + "FROM Decostar.dbo.cp_mstr_sql "
                                + "WHERE cp_part = @cp_part";
                                
            command.Parameters.AddWithValue("@cp_part", finishedGood);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            reader.Read();
            pt_desc2 = reader["cp_comment"].ToString();
            cp_cust_part = reader["cp_cust_part"].ToString();

            database.CloseConnection();

            var pattern = @"  +"; // match two or more spaces
            var groups = Regex.Split(cp_cust_part, pattern); //split string on multiple spaces
            var tokens = groups.Select(group => group.Replace(" ", String.Empty)); //remove all single spaces
            cp_cust_part = String.Join("  ", tokens.ToArray()); //rebuild string with double spaces where 2+ spaces used to be

            #endregion

            FormLabel(message.UnitPrinter);

            //pass scan 
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
            ipl += "<STX>H2,Lot     ;o94,137;f3;h11;w11;c26;r0;b0;d3,Lot: " + lot + ";<ETX>";
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