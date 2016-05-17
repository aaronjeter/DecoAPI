using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DecoAPI.Printers
{
    public class GMOrangeLabelPrinter
    {
        public static void Print(string partNumber, string printer)
        {
            #region Printer Info
            string name = string.Empty;
            string IP = string.Empty;
            string alias = string.Empty;
            string DPI = string.Empty;

            string query = string.Empty;

            try
            {
                // Get the Printer Information
                Models.Database database = new Models.Database();
                
                query = "SELECT printer_id, device_path, ip_address, alias, dpi FROM Print_Service.dbo.PrinterPaths WHERE printer_id = " + printer;

                SqlDataReader reader = database.RunCommandReturnReader(query);

                name = reader["device_path"].ToString();
                IP = reader["ip_address"].ToString();
                alias = reader["alias"].ToString();
                DPI = reader["dpi"].ToString();
                database.CloseConnection();
            }
            catch (Exception e)
            {
                throw e;
            }
            #endregion

            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);

            #region IPL Head
            string sIPLHead = "<STX><SI>t0<ETX>" +
                                "<STX><SI>T1<ETX> " +
                                "<STX><SI>g0,420<ETX> " +
                                "<STX><SI>d0<ETX> " +
                                "<STX><SI>S40<ETX> " +
                                "<STX><SI>R1<ETX> " +
                                "<STX><SI>r115<ETX> " +
                                "<STX><SI>D115<ETX> " +
                                "<STX><SI>I6<ETX> " +
                                "<STX><SI>F0<ETX> " +
                                "<STX><ESC>C1<ETX> " +
                                "<STX><ESC>P<ETX> " +
                                "<STX>E1;F1;<ETX> ";
            #endregion
            #region IPL Body
            string sIPLBody = "";

            if (DPI == "400")
            {
                sIPLBody =          "<STX>H11;o64,300;f0;c26;h6;w2;k70;d3," + DateTime.Now.ToShortDateString() + ";<ETX>" +
                                    "<STX>H12;o564,800;f0;c26;h2;w2;k70;d3," + part.DescColor + ";<ETX>";
            }
            else if (DPI == "200")
            {
                sIPLBody =          "<STX>H11;o32,150;f0;c26;h3;w1;k70;d3," + DateTime.Now.ToShortDateString() + ";<ETX>" +
                                    "<STX>H12;o282,400;f0;c26;h1;w1;k70;d3," + part.DescColor + ";<ETX>";
            }
            #endregion
            #region IPL Tail
            string sIPLTail = "<STX>R<ETX> " +
                                "<STX><ESC>E1<ETX> " +
                                "<STX><CAN><ETX> " +
                                "<STX><ETX> " +
                                "<STX><ETB><ETX>";
            #endregion

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            PrintHandler ph = new PrintHandler(printer);
            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }            
        }
    }
}