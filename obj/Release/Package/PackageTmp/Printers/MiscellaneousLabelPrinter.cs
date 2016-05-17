using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace DecoAPI.Printers
{
    public class MiscellaneousLabelPrinter
    {  
        /// <summary>
        /// Used to print Rehaue Label for Unload App
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="printer"></param>
        public static void PrintRehauPartLabel(string partNumber, string printer)
        {
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);  
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            //Change IPL here to new Rehau label format
            #region IPL Body
            string sIPLBody = "";

            if (DPI == "400")
            {
                throw new Exception("No 400 DPI Option");
            }
            else if (DPI == "200")
            {
                #region IPL
                sIPLBody = "<STX>R<ETX><LF>" +
                    "<LF><STX><ESC>k<ETX><LF>" +
                    "<LF><STX><ESC>C0<ETX><LF>" +
                    "<LF><STX><ESC>P;<ETX><LF>" +
                    "<LF><STX>E*;F*;<ETX><LF>" +
                    "<LF><STX>L39;D0;<ETX><LF>" +
                    "<LF><STX>B0,RehauPar;o113,25;f3;h80;w3;c6;i0;d0,10;D39;<ETX><LF>" +
                    "<LF><STX>H1,Text0002;o35,180;f3;h10;w10;c26;r0;b0;d0,20;<ETX><LF>" +
                    "<LF><STX>H2,Descript;o150,25;f3;h11;w11;c26;r0;b0;d0,27;<ETX><LF>" +
                    "<LF><STX>H3,RehauPar;o35,25;f3;h10;w10;c26;r0;b0;d0,10;<ETX><LF>" +
                    "<LF><STX>R<ETX><LF>" +
                    "<LF><STX>R<ETX><LF>" +
                    "<LF><STX><ESC>k<ETX><LF>" +
                    "<LF><STX><ESC>C0<LF>" +
                    "<LF><SI>g0,424<LF>" +
                    "<LF><SI>T1<LF>" +
                    "<LF><SI>R0<SI>r0<LF>" +
                    "<LF><SI>D0<LF>" +
                    "<LF><SI>c0<SI>t0<LF>" +
                    "<LF><SI>l0<LF>" +
                    "<LF><SI>F2<LF>" +
                    "<LF><SI>f0<LF>" +
                    "<LF><SI>L1000<LF>" +
                    "<LF><SI>I5<LF>" +
                    "<LF><SI>i0<LF>" +
                    "<LF><SI>W406;<SI>S40<LF>" +
                    "<LF><SI>d0<LF>" +
                    "<LF><ETX><LF>" +
                    "<LF><STX><ESC>E*<CAN><ETX><LF>" +
                    "<LF><STX><ESC>F0<LF>" +
                    "P" + part.CustomerNumber + "<ETX><LF> ---This is the Rehau part number prefixed with a P--" +
                    "<LF><STX><ESC>F1<LF>" +
                    DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "<ETX><LF> --Date and Time Printed--" +
                    "<LF><STX><ESC>F2<LF>" +
                    part.Description2 + "<ETX><LF> --Part Description--" +
                    "<LF><STX><ESC>F3<LF>" +
                    "P" + part.CustomerNumber + "<ETX><LF> ---This is the Rehau part number prefixed with a P--" +
                    "<LF><STX><RS>1<ETX><STX><US>1<ETX><STX><ETB><ETX><LF><LF>";
                #endregion
            }
            #endregion

            string sIPL = sIPLBody;
            
            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints a Pass label for BMW or GM
        /// </summary>
        /// <param name="sPartNumber"></param>
        /// <param name="location"></param>
        /// <param name="printer"></param>
        public static void PrintPassLabel(string partNumber, string location, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);

            #region Get Part # Information
            try
            {
                // Get the Part # Information
                Models.Database database = new Models.Database();
                SqlCommand command = new SqlCommand();
                command.CommandText = " select cp_cust_part from Decostar.dbo.cp_mstr_sql where cp_cust = @location and cp_part = @partNumber";
                command.Parameters.AddWithValue("@location", location);
                command.Parameters.AddWithValue("@partNumber", partNumber);
                partNumber = database.RunSingleResultQuery(command);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            #endregion

            Models.sPart part = new Models.Part().GetPartByCustomerPartNumber(partNumber);

            #region IPL Head
            string sIPLHead = "<STX><ESC>C<ETX>"
                            + "<STX><ESC>P<ETX>"
                            + "<STX>E1;F1<ETX>"
                            + "<STX><SI>g0,420<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";            
            
            sIPLBody =          // Description
                                "<STX>L1;o135,3;f3;l420;w3;<ETX>" +
                                "<STX>H2;o135,20;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
                                "<STX>B3;o110,40;f3;c6,0;h65;w3;r0;i0;d3," + partNumber + ";<ETX>" +
                                "<STX>H4;o45,40;f3;c30;h1;w1;d3,Part #: " + partNumber + ";<ETX>" +
                                "<STX>H5;o30,20;f3;c30;h1;w1;d3," + DateTime.Now.ToShortDateString() + ";<ETX>" +
                                "<STX>H6;o30,190;f3;c30;h1;w1;d3,Decostar #: " + part.Number + ";<ETX>" +
                                "<STX>L7;o3,3;f3;l420;w3;<ETX>";
            
            #endregion
            #region IPL Tail
            string sIPLTail = "<STX>R;<ETX><STX><CAN><ETX><STX><ESC>E1<ETX><STX><CAN><ETX><STX><ETB><ETX>";
            #endregion

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }            
        }

        /// <summary>
        /// Prints a Service Label for Nissan
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="printer"></param>
        public static void PrintNissanServiceLabel(string partNumber, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region Part Info
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            #endregion

            #region Build
            string sBarcodeWidth = "2";
            if (part.CustomerNumber.Length > 10)
                sBarcodeWidth = "1";
            string sTextWidth = "31";
            if (part.CustomerNumber.Length > 10)
                sTextWidth = "25";
            #endregion

            #region IPL Head
            string sIPLHead = "<STX>R<ETX><LF><LF><STX><ESC>k<ETX><LF><LF><STX><ESC>C0<ETX><LF><LF><STX><ESC>P;<ETX><LF><LF><STX>E*;F*;<ETX><LF><LF><STX>L39;D0;<ETX><LF><LF>";
            #endregion
            #region IPL Body
            string sIPLBody = "";

            if (DPI == "200")
            {
                sIPLBody = "<STX>B0,Barc0003        ;o185,445;f1;h61;w" + sBarcodeWidth + ";c0,3;i0;r1;d3," + part.CustomerNumber.Replace("-", "").Replace(" ", "") + ";D39;<ETX>"
                            + "<LF><LF><STX>H1,Text0007;o80,250;f1;h10;w10;c26;r0;b0;d0,12;<ETX>"
                            + "<LF><LF><STX>H2,part    ;o105,457;f1;h23;w" + sTextWidth + ";c25;r0;b0;d3," + part.CustomerNumber.Substring(0, 5) + "-" + part.CustomerNumber.Substring(5) + ";<ETX>"
                            + "<LF><LF><STX>H3,Date    ;o80,260;f1;h14;w13;c26;r0;b0;d3," + DateTime.Now.ToShortDateString() + ";<ETX>"
                            + "<LF><LF><STX>H4,Descr   ;o55,455;f1;h14;w13;c26;r0;b0;d3," + part.CustomerDesc.Substring(0, 17) + ";<ETX>"
                            + "<LF><LF><STX>H5,Quantity;o45,67;f1;h28;w27;c25;r0;b0;d3,1;<ETX>"
                            + "<LF><LF><STX>H6,Labe0000;o25,179;f1;h11;w11;c26;r0;b0;d3,MADE IN USA;<ETX>";
            }
            else if (DPI == "")
            {
                sIPLBody = "<STX>B0,Barc0003        ;o95,222;f1;h30;w1;c0,3;i0;r1;d3," + part.CustomerNumber.Replace("-", "").Replace(" ", "") + ";D39;<ETX>"
                            + "<LF><LF><STX>H1,Text0007;o42,125;f1;h10;w10;c26;r0;b0;d0,12;<ETX>"
                            + "<LF><LF><STX>H2,part    ;o55,228;f1;h23;w31;c25;r0;b0;d3," + part.CustomerNumber + ";<ETX>"
                            + "<LF><LF><STX>H3,Date    ;o40,130;f1;h14;w13;c26;r0;b0;d3," + DateTime.Now.ToShortDateString() + ";<ETX>"
                            + "<LF><LF><STX>H4,Descr   ;o30,227;f1;h14;w13;c26;r0;b0;d3," + part.CustomerDesc.Substring(0, 17) + ";<ETX>"
                            + "<LF><LF><STX>H5,Quantity;o25,33;f1;h28;w27;c25;r0;b0;d3,1;<ETX>"
                            + "<LF><LF><STX>H6,Labe0000;o15,89;f1;h11;w11;c26;r0;b0;d3,MADE IN USA;<ETX>";
            }
            #endregion
            #region IPL Tail
            string sIPLTail = "<LF><LF><STX>R<ETX><LF><LF><STX>R<ETX><LF><LF><STX><ESC>k<ETX><LF><LF><STX><ESC>C0<LF><LF><SI>g1,567<LF><LF><SI>T1<LF><LF><SI>R1<SI>r0<LF><LF><SI>D0<LF><LF><SI>c0<SI>t0<LF><LF><SI>l0<LF><LF><SI>F2<LF><LF><SI>f0<LF><LF><SI>L1000<LF><LF><SI>I5<LF><LF><SI>i0<LF><LF><SI>W508;<SI>S40<LF><LF><SI>d0<LF><LF><ETX><LF><LF><STX><ESC>E*<CAN><ETX><LF><LF><STX><ESC>F1<LF><ETX><LF><LF><STX><RS>1<ETX><STX><US>1<ETX><STX><ETB><ETX><LF><LF><STX><FF><ETX><LF><LF>";
            #endregion

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }            
        }

        /// <summary>
        /// Prints a Small GM Service Label
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="quantity"></param>
        /// <param name="printer"></param>
        public static void PrintSmallGMServiceLabel(string partNumber, int quantity, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region Part Info
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            #endregion

            #region IPL Head
            string sIPLHead = "<STX><SI>t0<ETX>" +
                                "<STX><SI>T1<ETX> " +
                                "<STX><SI>g1,567<ETX> " +
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
                sIPLBody =  // Barcode (centered)
                            "<STX>B1;o550,120;f3;c0,3;i0;h120;w3;d3," + part.CustomerNumber + "<ETX> " +
                    // Source Code & Date Code
                            "<STX>H2;o430,30;f3;c26;w1;h1;k10;d3," + "06300 " + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.DayOfYear + ";<ETX> " +
                    // GM Part Number
                            "<STX>H3;o324,30;f3;c26;w1;h1;k12;d3," + "GM#" + ";<ETX> " +
                            "<STX>H4;o370,150;f3;c26;w1;h1;k20;d3," + part.CustomerNumber + ";<ETX> " +
                    // Quantity
                            "<STX>H5;o210,30;f3;c26;w1;h1;k12;d3," + "QTY." + ";<ETX> " +
                            "<STX>H6;o260,180;f3;c26;w1;h1;k20;d3," + quantity + ";<ETX> " +
                    // Made in USA
                            "<STX>H7;o210,460;f3;c26;w1;h1;k12;d3," + "MADE IN USA" + ";<ETX> " +
                            "";
            }
            else if (DPI == "200")
            {
                sIPLBody =  // Barcode (centered)
                            "<STX>B1;o275,60;f3;c0,3;i0;h60;w2;d3," + part.CustomerNumber + "<ETX> " +
                    // Source Code & Date Code
                            "<STX>H2;o215,15;f3;c26;w1;h1;k10;d3," + "06300 " + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.DayOfYear + ";<ETX> " +
                    // GM Part Number
                            "<STX>H3;o162,15;f3;c26;w1;h1;k12;d3," + "GM#" + ";<ETX> " +
                            "<STX>H4;o185,75;f3;c26;w1;h1;k20;d3," + part.CustomerNumber + ";<ETX> " +
                    // Quantity
                            "<STX>H5;o105,15;f3;c26;w1;h1;k12;d3," + "QTY." + ";<ETX> " +
                            "<STX>H6;o130,90;f3;c26;w1;h1;k20;d3," + quantity + ";<ETX> " +
                    // Made in USA
                            "<STX>H7;o105,230;f3;c26;w1;h1;k12;d3," + "MADE IN USA" + ";<ETX> " +
                            "";
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

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints a Large GM Service Label
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="quantity"></param>
        /// <param name="printer"></param>
        public static void PrintLargeGMServiceLabel(string partNumber, int quantity, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region Part Info
            Models.sPart oPart = new Models.Part().GetPartByPartNumber(partNumber);
            #endregion

            #region Description
            string sCustDesc = oPart.CustomerDesc;
            if (sCustDesc.Length > 25)
                sCustDesc = sCustDesc.Substring(0, 24);
            #endregion

            #region IPL Head
            string sIPLHead = "<STX><SI>t0<ETX>" +
                                "<STX><SI>T1<ETX> " +
                                "<STX><SI>g1,567<ETX> " +
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
                sIPLBody =  // Barcode (centered)
                            "<STX>B1;o840,150;f3;c0,3;i0;h160;w8;d3," + oPart.CustomerNumber + "<ETX> " +
                    // Source Code & Date Code
                            "<STX>H2;o610,1100;f3;c26;w1;h1;k10;d3," + "06300 " + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.DayOfYear + ";<ETX> " +
                    // GM Part Number
                            "<STX>H3;o630,60;f3;c26;w1;h1;k12;d3," + "GM#" + ";<ETX> " +
                            "<STX>H4;o680,180;f3;c26;w1;h1;k20;d3," + oPart.CustomerNumber + ";<ETX> " +
                    // Quantity
                            "<STX>H5;o460,60;f3;c26;w1;h1;k12;d3," + "QTY." + ";<ETX> " +
                            "<STX>H6;o510,420;f3;c26;w1;h1;k20;d3," + quantity + ";<ETX> " +
                    // Customer Description
                            "<STX>H7;o320,60;f3;c26;w1;h1;k12;d3," + sCustDesc + ";<ETX> " +
                    // Made in USA
                            "<STX>H8;o320,1100;f3;c26;w1;h1;k12;d3," + "MADE IN USA" + ";<ETX> " +
                    // D.O.T.
                            "<STX>H9;o510,1100;f3;c26;w1;h1;k20;d3," + "D.O.T." + ";<ETX> " +
                            "";
            }
            else if (DPI == "200")
            {
                sIPLBody =  // Barcode (centered)
                            "<STX>B1;o420,75;f3;c0,3;i0;h80;w4;d3," + oPart.CustomerNumber + "<ETX> " +
                    // Source Code & Date Code
                            "<STX>H2;o305,550;f3;c26;w1;h1;k10;d3," + "06300 " + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.DayOfYear + ";<ETX> " +
                    // GM Part Number
                            "<STX>H3;o315,30;f3;c26;w1;h1;k12;d3," + "GM#" + ";<ETX> " +
                            "<STX>H4;o340,90;f3;c26;w1;h1;k20;d3," + oPart.CustomerNumber + ";<ETX> " +
                    // Quantity
                            "<STX>H5;o230,30;f3;c26;w1;h1;k12;d3," + "QTY." + ";<ETX> " +
                            "<STX>H6;o255,105;f3;c26;w1;h1;k20;d3," + quantity + ";<ETX> " +
                    // Customer Description
                            "<STX>H7;o160,30;f3;c26;w1;h1;k12;d3," + sCustDesc + ";<ETX> " +
                    // Made in USA
                            "<STX>H8;o160,550;f3;c26;w1;h1;k12;d3," + "MADE IN USA" + ";<ETX> " +
                    // D.O.T.
                            "<STX>H9;o255,550;f3;c26;w1;h1;k20;d3," + "D.O.T." + ";<ETX> " +
                            "";
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

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints a Small GM DOT Label
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="quantity"></param>
        /// <param name="printer"></param>
        public static void PrintSmallGMDOTLabel(string partNumber, int quantity, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;
            
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);            
            
            #region Database Storage
            string groupNumber = "";
            try
            {
                Models.Database database = new Models.Database();
                SqlCommand command = new SqlCommand();
                command.CommandText = " select GroupNumber from RackRecords.dbo.GMServiceDOT where PartNumber = @partNumber ";
                command.Parameters.AddWithValue("@partNumber", partNumber);

                groupNumber = database.RunSingleResultQuery(command);
            }
            catch (Exception)
            {
                throw new Exception("Translation not found");
            }
            #endregion

            #region Date
            string sDate = DateTime.Now.Year.ToString().Substring(2, 2);
            string sDay = DateTime.Now.DayOfYear.ToString();
            if (sDay.Length == 1)
            {
                sDate += "00" + sDay;
            }
            else if (sDay.Length == 2)
            {
                sDate += "0" + sDay;
            }
            else
            {
                sDate += sDay;
            }
            #endregion

            #region IPL Head
            string sIPLHead = "<STX><SI>t0<ETX>" +
                                "<STX><SI>T1<ETX> " +
                                "<STX><SI>g1,567<ETX> " +
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
                sIPLBody = "";
            }
            else if (DPI == "200")
            {
                sIPLBody =  // Barcode (centered)
                            "<STX>B01;o275,60;f3;c0,3;i0;h60;w2;d3," + part.CustomerNumber + "<ETX> " +
                            // Source Code & Date Code
                            "<STX>H02;o215,15;f3;c26;w1;h1;k10;d3," + "06300 " + sDate + ";<ETX> " +
                            "<STX>H03;o215,300;f3;c26;w1;h1;k12;d3,GR." + groupNumber + ";<ETX> " +
                            "<STX>H04;o162,370;f3;c26;w1;h1;k12;d3,D.O.T.;<ETX> " +
                            // GM Part Number
                            "<STX>H05;o162,15;f3;c26;w1;h1;k12;d3," + "GM#" + ";<ETX> " +
                            "<STX>H06;o185,75;f3;c26;w1;h1;k20;d3," + part.CustomerNumber + ";<ETX> " +
                            // Quantity
                            "<STX>H07;o105,15;f3;c26;w1;h1;k12;d3," + "QTY." + ";<ETX> " +
                            "<STX>H08;o130,90;f3;c26;w1;h1;k20;d3," + quantity + ";<ETX> " +
                            "";
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

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints a Large GM DOT Label
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="quantity"></param>
        /// <param name="printer"></param>
        public static void PrintLargeGMDOTLabel(string partNumber, int quantity, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;
            
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);

            #region Database Storage
            string englishDesc = string.Empty;
            string frenchDesc = string.Empty;
            string spanishDesc = string.Empty;
            string groupNumber = string.Empty;
            try
            {
                Models.Database database = new Models.Database();
                SqlCommand command = new SqlCommand();
                command.CommandText = " select * from RackRecords.dbo.GMServiceDOT where PartNumber = @partNumber";
                command.Parameters.AddWithValue("@partNumber", partNumber);

                SqlDataReader reader = database.RunCommandReturnReader(command);

                if (reader.Read())
                {
                    englishDesc = reader["English"].ToString();
                    frenchDesc = reader["French"].ToString();
                    spanishDesc = reader["Spanish"].ToString();
                    groupNumber = reader["GroupNumber"].ToString();
                }                
            }
            catch (Exception)
            {
                throw new Exception("Translation not found");
            }
            #endregion

            #region Date
            string sDate = DateTime.Now.Year.ToString().Substring(2, 2);
            string sDay = DateTime.Now.DayOfYear.ToString();
            if (sDay.Length == 1)
            {
                sDate += "00" + sDay;
            }
            else if (sDay.Length == 2)
            {
                sDate += "0" + sDay;
            }
            else
            {
                sDate += sDay;
            }
            #endregion

            #region IPL Head
            string sIPLHead = "<STX><SI>t0<ETX>" +
                                "<STX><SI>T1<ETX> " +
                                "<STX><SI>g1,567<ETX> " +
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
                sIPLBody =  "";
            }
            else if (DPI == "200")
            {
                sIPLBody =  // Barcode (centered)
                            "<STX>B01;o420,75;f3;c0,3;i0;h80;w4;d3," + part.CustomerNumber + "<ETX> " +
                            // Source Code & Date Code + Group + DOT
                            "<STX>H02;o330,400;f3;c20;w1;h2;d3," + "06300 " + sDate + ";<ETX> " +
                            "<STX>H03;o330,610;f3;c20;w1;h2;d3,GR." + groupNumber + ";<ETX> " +
                            "<STX>H04;o270,650;f3;c20;w1;h2;d3,D.O.T.;<ETX> " +
                            // GM Part Number
                            "<STX>H05;o330,30;f3;c20;w1;h2;d3," + "GM#" + ";<ETX> " +
                            "<STX>H06;o340,90;f3;c26;w1;h1;k20;d3," + part.CustomerNumber + ";<ETX> " +
                            // Quantity
                            "<STX>H07;o280,30;f3;c20;w1;h2;d3," + "QTY." + ";<ETX> " +
                            "<STX>H08;o290,90;f3;c26;w1;h1;k20;d3," + quantity + ";<ETX> " +
                            // Customer Description
                            "<STX>H09;o235,30;f3;c20;w1;h2;d3," + englishDesc + ";<ETX> " +
                            "<STX>H10;o190,30;f3;c20;w1;h2;d3," + frenchDesc + ";<ETX> " +
                            "<STX>H11;o145,30;f3;c20;w1;h2;d3," + spanishDesc + ";<ETX> " +
                            "";
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

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints a Large Honda Part Label
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="printer"></param>
        public static void PrintLargeHondaPartLabel(string partNumber, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region Part Info
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            #endregion

            #region IPL Head
            string sIPLHead = "<STX><SI>t0<ETX>" +
                                "<STX><SI>T1<ETX> " +
                                "<STX><SI>g1,567<ETX> " +
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
                sIPLBody =  // Honda Customer Part Number
                            "<STX>H1;o600,20;f3;c26;w1;h1;k20;d3," + part.CustomerNumber + ";<ETX> " +
                    // Honda Customer Part Number Barcode
                            "<STX>B2;o440,20;f3;c0,3;i0;h160;w3;d3," + part.CustomerNumber + "<ETX> " +
                    // Quantity (barcode, human readable)
                            "<STX>B3;o260,20;f3;c0,3;i0;h160;w6;d3,1<ETX> " +
                            "<STX>H4;o260,400;f3;c26;w1;h1;k20;d3," + "1" + ";<ETX> " +
                    // Vendor Code, Contract Package, Julian Date
                            "<STX>H5;o280,1000;f3;c26;w1;h1;k10;d3," + "VC 002519" + ";<ETX> " +
                            "<STX>H6;o220,1000;f3;c26;w1;h1;k10;d3," + "CP " + ";<ETX> " +
                            "<STX>H7;o160,1000;f3;c26;w1;h1;k10;d3," + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.DayOfYear + ";<ETX> " +
                    // Assembled in USA
                            "<STX>H8;o100,40;f3;c26;w1;h1;k12;d3," + "ASSEMBLED IN USA" + ";<ETX> " +
                            "";
            }
            else if (DPI == "200")
            {
                sIPLBody =  // Honda Customer Part Number
                            "<STX>H1;o350,40;f3;c26;w1;h1;k20;d3," + part.CustomerNumber + ";<ETX> " +
                    // Honda Customer Part Number Barcode
                            "<STX>B2;o270,40;f3;c0,3;i0;h100;w2;d3," + part.CustomerNumber + "<ETX> " +
                    // Quantity (barcode, human readable)
                            "<STX>B3;o150,40;f3;c0,3;i0;h80;w3;d3,1<ETX> " +
                            "<STX>H4;o150,220;f3;c26;w1;h1;k20;d3," + "1" + ";<ETX> " +
                    // Vendor Code, Contract Package, Julian Date
                            "<STX>H5;o160,520;f3;c26;w1;h1;k10;d3," + "VC 002519" + ";<ETX> " +
                            "<STX>H6;o130,520;f3;c26;w1;h1;k10;d3," + "CP " + ";<ETX> " +
                            "<STX>H7;o100,520;f3;c26;w1;h1;k10;d3," + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.DayOfYear + ";<ETX> " +
                    // Assembled in USA
                            "<STX>H8;o70,40;f3;c26;w1;h1;k12;d3," + "ASSEMBLED IN USA" + ";<ETX> " +
                            "";
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

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints a Small Honda Part Label
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="printer"></param>
        public static void PrintSmallHondaPartLabel(string partNumber, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);

            #region IPL Head
            string sIPLHead = "<STX><SI>t0<ETX>" +
                                "<STX><SI>T1<ETX> " +
                                "<STX><SI>g1,567<ETX> " +
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
                sIPLBody =  // Honda Customer Part Number
                            "<STX>H1;o400,20;f3;c26;w1;h1;k16;d3," + part.CustomerNumber + ";<ETX> " +
                    // Honda Customer Part Number Barcode
                            "<STX>B2;o300,20;f3;c0,0;i0;h100;w3;d3," + part.CustomerNumber + "<ETX> " +
                    // Quantity (barcode, human readable) 
                            "<STX>B3;o180,20;f3;c0,3;i0;h80;w4;d3,1<ETX> " +
                            "<STX>H4;o200,400;f3;c26;w1;h1;k16;d3," + "1" + ";<ETX> " +
                    // Vendor Code, Contract Package, Julian Date
                            "<STX>H5;o160,800;f3;c26;w1;h1;k6;d3," + "VC 002519" + ";<ETX> " +
                            "<STX>H6;o130,800;f3;c26;w1;h1;k6;d3," + "CP " + ";<ETX> " +
                            "<STX>H7;o100,800;f3;c26;w1;h1;k6;d3," + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.DayOfYear + ";<ETX> " +
                    // Assembled in USA
                            "<STX>H8;o80,40;f3;c26;w1;h1;k8;d3," + "ASSEMBLED IN USA" + ";<ETX> " +
                            "";
            }
            else if (DPI == "200")
            {
                sIPLBody =  // Honda Customer Part Number
                            "<STX>H1;o260,40;f3;c26;w1;h1;k16;d3," + part.CustomerNumber + ";<ETX> " +
                    // Honda Customer Part Number Barcode
                            "<STX>B2;o200,40;f3;c16,0,0,0;i0;h70;w2;d3," + part.CustomerNumber + "<ETX> " +
                    // Quantity (barcode, human readable) 
                            "<STX>B3;o110,40;f3;c0,3;i0;h50;w2;d3,1<ETX> " +
                            "<STX>H4;o115,220;f3;c26;w1;h2;k16;d3," + "1" + ";<ETX> " +
                    // Vendor Code, Contract Package, Julian Date
                            "<STX>H5;o110,420;f3;c26;w1;h1;k6;d3," + "VC 002519" + ";<ETX> " +
                            "<STX>H6;o90, 420;f3;c26;w1;h1;k6;d3," + "CP " + ";<ETX> " +
                            "<STX>H7;o70, 420;f3;c26;w1;h1;k6;d3," + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.DayOfYear + ";<ETX> " +
                    // Assembled in USA
                            "<STX>H8;o50,40;f3;c26;w1;h1;k8;d3," + "ASSEMBLED IN USA" + ";<ETX> " +
                            "";
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

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints a Pass label for BMW or GM
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="printer"></param>
        public static void PrintPartLabel(string partNumber, string printer)
        {
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region Odometer

            string serial = "";

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "Production.dbo.sp_getOdometerSerial5";
            command.Parameters.AddWithValue("@id", 1);
            command.Parameters.AddWithValue("@nextOne", serial);

            serial = database.RunSingleResultQuery(command);   
            #endregion           

            #region IPL Head
            string sIPLHead = "";
            sIPLHead = "<STX>R<ETX>"
                     + "<STX><ESC>C<SI>h<SI>I1<ETX>"
                     + "<STX><ESC>P;F*<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";

            if (DPI == "400")
            {
                sIPLBody =          // Description
                                    "<STX>L1;o270,6;f3;l840;w3;<ETX>" +
                                    "<STX>H2;o270,40;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>B3;o220,30;f3;c6,0;h130;w4;r0;i0;d3,P" + part.Number + serial + ";<ETX>" +
                                    "<STX>H4;o90,80;f3;c30;h1;w1;d3,Part #: " + part.Number + ";<ETX>" +
                                    "<STX>H5;o60,40;f3;c30;h1;w1;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>H6;o60,250;f3;c30;h1;w1;d3," + "Decostar Part Label" + ";<ETX>" +
                                    "<STX>L7;o6,6;f3;l840;w3;<ETX>";
            }
            else if (DPI == "200")
            {
                sIPLBody =          // Description
                                    "<STX>L1;o135,3;f3;l420;w3;<ETX>" +
                                    "<STX>H2;o135,20;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>B3;o110,15;f3;c6,0;h65;w2;r0;i0;d3,P" + part.Number + serial + ";<ETX>" +
                                    "<STX>H4;o45,40;f3;c30;h1;w1;d3,Part #: " + part.Number + ";<ETX>" +
                                    "<STX>H5;o30,20;f3;c30;h1;w1;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>L6;o3,3;f3;l420;w3;<ETX>";
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

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints a Part label for BMW or GM or KIA. This method also updates the AssemblyRecords Database
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="sLocation"></param>
        /// <param name="printer"></param>
        public static string PrintPartLabel(string partNumber, string printer, string moldLabel, string paintLabel)
        {
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region Odometer
            string serial = "";

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "Production.dbo.sp_getOdometerSerial5";
            command.Parameters.AddWithValue("@id", 1);
            command.Parameters.AddWithValue("@nextOne", serial);

            serial = database.RunSingleResultQuery(command);

            // CHMSL, KIA, & Service will have Decostar Part Numbers in the barcode
            //  whereas LPI will have Customer Part Numbers
            string partLabel = string.Empty;
            string HRPart = string.Empty;
            if ((part.Description1.Substring(6, 1) == "O" || part.PartType == "PAINTA") || part.Description1.Substring(0, 2) == "09")
            {
                partLabel = "P" + part.Number + serial;
                HRPart = part.Number;
            }
            else if (part.Description1.Substring(6, 1) == "L")
            {
                partLabel = "P" + part.CustomerNumber + serial;
                HRPart = part.CustomerNumber;
            }

            #endregion

            #region IPL Head
            string sIPLHead;
            sIPLHead = "<STX>R<ETX>"
                     + "<STX><ESC>C<SI>h<SI>I1<ETX>"
                     + "<STX><ESC>P;F*<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";

            if (DPI == "400")
            {
                sIPLBody =          "<STX>L1;o270,6;f3;l840;w3;<ETX>" +
                                    "<STX>H2;o270,40;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>B3;o220,30;f3;c6,0;h130;w4;r0;i0;d3," + partLabel + ";<ETX>" +
                                    "<STX>H4;o90,80;f3;c30;h1;w1;d3,Part #: " + HRPart + ";<ETX>" +
                                    "<STX>H5;o60,40;f3;c30;h1;w1;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>H6;o60,250;f3;c30;h1;w1;d3," + "Decostar Part Label" + ";<ETX>" +
                                    "<STX>L7;o6,6;f3;l840;w3;<ETX>";
            }
            else if (DPI == "200")
            {
                sIPLBody =          "<STX>L1;o135,3;f3;l420;w3;<ETX>" +
                                    "<STX>H2;o135,20;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>B3;o110,15;f3;c6,0;h65;w2;r0;i0;d3," + partLabel + ";<ETX>" +
                                    "<STX>H4;o45,40;f3;c30;h1;w1;d3,Part #: " + HRPart + ";<ETX>" +
                                    "<STX>H5;o30,20;f3;c30;h1;w1;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>L6;o3,3;f3;l420;w3;<ETX>";
            }
            #endregion
            #region IPL Tail
            string sIPLTail = "<STX>R<ETX> " +
                                "<STX><ESC>E1<ETX> " +
                                "<STX><CAN><ETX> " +
                                "<STX><ETX> " +
                                "<STX><ETB><ETX>";
            sIPLTail = "";
            sIPLTail = "<STX>D0<ETX>"
                     + "<STX>R<ETX>"
                     + "<STX><ESC>E*,1<CAN><ETX>"
                     + "<STX><RS>1<US>1<ETB><FF><ETX>";
            #endregion

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }

            #region Database Updates

            // This is only recorded for GM CHMSL/LPI and KIA parts
            if ((part.Description1.Substring(0, 2) != "08") && (part.Description1.Substring(0, 2) != "09"))
                return partLabel;
            if ((part.Description1.Substring(6, 1) == "O") && (part.Description1.Substring(0, 2) == "08"))
            {
                // Record the Part Label in the Database
                database = new Models.Database();
                command = new SqlCommand();
                command.CommandText = " insert into AssemblyRecords.dbo.CHMSL_Assembly (part_label, mold_scan, paint_scan, part_number) "
                                    + " values (@partLabel, @moldLabel, @paintLabel, @partNumber)";
                command.Parameters.AddWithValue("@partLabel", partLabel);
                command.Parameters.AddWithValue("@moldLabel", moldLabel);
                command.Parameters.AddWithValue("@paintLabel", paintLabel);
                command.Parameters.AddWithValue("@partNumber", partNumber);

                database.ExecuteNonQuery(command);
            }
            else if ((part.Description1.Substring(6, 1) == "L") && (part.Description1.Substring(0, 2) == "08"))
            {
                // Record the Part Label in the Database
                database = new Models.Database();
                command = new SqlCommand();
                command.CommandText = " insert into AssemblyRecords.dbo.LPI_Assembly (part_label, mold_scan, paint_scan, part_number, station, bypass) "
                                    + " values (@partLabel, @moldLabel, @paintLabel, @partNumber, 'DecoCore', 1)";
                command.Parameters.AddWithValue("@partLabel", partLabel);
                command.Parameters.AddWithValue("@moldLabel", moldLabel);
                command.Parameters.AddWithValue("@paintLabel", paintLabel);
                command.Parameters.AddWithValue("@partNumber", partNumber);

                database.ExecuteNonQuery(command);

            }
            else if (part.Description1.Substring(0, 2) == "09")
            {
                // Record the Part Label in the Database
                database = new Models.Database();
                command = new SqlCommand();
                command.CommandText = " insert into AssemblyRecords.dbo.KIA_Assembly (part_label, mold_scan, paint_scan, part_number, print_date) "
                                    + " values (@partLabel, @moldLabel, @paintLabel, @date)";
                command.Parameters.AddWithValue("@partLabel", partLabel);
                command.Parameters.AddWithValue("@moldLabel", moldLabel);
                command.Parameters.AddWithValue("@paintLabel", paintLabel);
                command.Parameters.AddWithValue("@partNumber", partNumber);
                command.Parameters.AddWithValue("@date", DateTime.Now);

                database.ExecuteNonQuery(command);                
            }

            #endregion 

            return partLabel;
        }

        /// <summary>
        /// Prints a Part label for KIA
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="sLocation"></param>
        /// <param name="printer"></param>
        public static string PrintKIAPartLabel(string partNumber, string printer, string moldLabel, string paintLabel)
        {
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region Odometer
            string serial = "";

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "Production.dbo.sp_getOdometerSerial5";
            command.Parameters.AddWithValue("@id", 1);
            command.Parameters.AddWithValue("@nextOne", serial);

            serial = database.RunSingleResultQuery(command);

            // KIA will have Decostar Part Numbers in the barcode
            string partLabel = string.Empty;
            string HRPart = string.Empty;
            if (part.Description1.Substring(0, 2) == "09")
            {
                partLabel = "P" + part.Number + serial;
                HRPart = part.Number;
            }

            #endregion

            #region PGN/PAC
            string PGN = string.Empty;
            string PAC = string.Empty;

            database = new Models.Database();
            command = new SqlCommand();
            command.CommandText = " select * from RackRecords.dbo.KIA_PGN_PAC_xref where deco_partnum = @partNumber";
            command.Parameters.AddWithValue("@partNumber", partNumber);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                PGN = reader["kia_pgn"].ToString();
                PAC = reader["kia_pac"].ToString();
            }
            database.CloseConnection();            
            #endregion

            #region IPL Head
            string sIPLHead;
            sIPLHead = "<STX>R<ETX>"
                     + "<STX><ESC>C<SI>h<SI>I1<ETX>"
                     + "<STX><ESC>P;F*<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";

            if (DPI == "400")
            {
                sIPLBody =          // Description
                                    "<STX>L1;o780,6;f3;l1680;w3;<ETX>" +
                                    "<STX>H2;o780,40;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>B3;o690,80;f3;c6,0;h130;w4;r0;i0;d3," + partLabel + ";<ETX>" +
                                    "<STX>H4;o580,40;f3;c30;h1;w1;d3,Decostar Part #: " + HRPart + ";<ETX>" +
                                    "<STX>H5;o530,50;f3;c30;h1;w1;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>L6;o390,6;f3;l1680;w3;<ETX>" +
                                    "<STX>H7;o390,40;f3;c30;h1;w1;d3,PGN/PAC: " + PGN + " " + PAC + ";<ETX>" +
                                    "<STX>B8;o320,80;f3;c0,0;h130;w4;r0;i0;d3," + PGN + PAC + ";<ETX>" +
                                    "<STX>H9;o100,40;f3;c30;h1;w1;d3,Customer Part #: " + part.CustomerNumber + ";<ETX>" +
                                    "<STX>L10;o20,6;f3;l1680;w3;<ETX>";
                if (part.Description1.Substring(2, 2) == "LF")  //(sPartNumber == "9710007" || sPartNumber == "9710008")
                {
                    sIPLBody += "<STX>H7;o390,40;f3;c30;h1;w1;d3,ALC Code: " + PGN + PAC + ";<ETX>" +
                                    "<STX>B8;o320,80;f3;c0,0;h130;w4;r0;i0;d3," + PGN + PAC + ";<ETX>";
                }
            }
            else if (DPI == "200")
            {
                sIPLBody =          // Description
                                    "<STX>L1;o385,3;f3;l840;w3;<ETX>" +
                                    "<STX>H2;o385,20;f3;c30;h2;w2;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>B3;o345,40;f3;c6,0;h80;w3;r0;i0;d3," + partLabel + ";<ETX>" +
                                    "<STX>H4;o260,20;f3;c30;h2;w2;d3,Decostar Part #: " + HRPart + ";<ETX>" +
                                    "<STX>H5;o220,25;f3;c30;h1;w2;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>L6;o195,3;f3;l840;w3;<ETX>" +
                                    "<STX>H7;o195,20;f3;c30;h2;w2;d3,PGN/PAC: " + PGN + " " + PAC + ";<ETX>" +
                                    "<STX>B8;o150,40;f3;c0,0;h80;w3;r0;i0;d3," + PGN + PAC + ";<ETX>" +
                                    "<STX>H9;o60,20;f3;c30;h2;w2;d3,Customer Part #: " + part.CustomerNumber + ";<ETX>" +
                                    "<STX>L10;o15,3;f3;l840;w3;<ETX>";
                if (part.Description1.Substring(2, 2) == "LF")  //(sPartNumber == "9710007" || sPartNumber == "9710008")
                {
                    sIPLBody += "<STX>H7;o195,20;f3;c30;h2;w2;d3,ALC Code: " + PGN + PAC + ";<ETX>" +
                                    "<STX>B8;o150,40;f3;c0,0;h80;w3;r0;i0;d3," + PGN + PAC + ";<ETX>";
                }
            }
            #endregion
            #region IPL Tail
            string sIPLTail = "<STX>R<ETX> " +
                                "<STX><ESC>E1<ETX> " +
                                "<STX><CAN><ETX> " +
                                "<STX><ETX> " +
                                "<STX><ETB><ETX>";
            sIPLTail = "";
            sIPLTail = "<STX>D0<ETX>"
                     + "<STX>R<ETX>"
                     + "<STX><ESC>E*,1<CAN><ETX>"
                     + "<STX><RS>1<US>1<ETB><FF><ETX>";
            #endregion

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }

            // This is only recorded for KIA parts
            if ((part.Description1.Substring(0, 2) != "09"))
                return partLabel;
            else if (part.Description1.Substring(0, 2) == "09")
            {
                database = new Models.Database();
                command = new SqlCommand();

                command.CommandText = " insert into AssemblyRecords.dbo.KIA_Assembly (part_label, mold_scan, paint_scan, part_number, print_date) "
                                    + " values (@partLabel, @moldLabel, @paintLabel, @partNumber, @time)";
                command.Parameters.AddWithValue("@partLabel", partLabel);
                command.Parameters.AddWithValue("@moldLabel", moldLabel);
                command.Parameters.AddWithValue("@paintLabel", paintLabel);
                command.Parameters.AddWithValue("@partNumber", partNumber);
                command.Parameters.AddWithValue("@time", DateTime.Now);
            }

            return partLabel;
        }

        /// <summary>
        /// Prints a Service Part label for KIA
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="sLocation"></param>
        /// <param name="printer"></param>
        public static string PrintKIAServicePartLabel(string partNumber, string printer, string moldLabel, string paintLabel)
        {
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region Odometer
            string serial = "";

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "Production.dbo.sp_getOdometerSerial5";
            command.Parameters.AddWithValue("@id", 1);
            command.Parameters.AddWithValue("@nextOne", serial);

            serial = database.RunSingleResultQuery(command);

            // KIA will have Decostar Part Numbers in the barcode
            string partLabel = string.Empty;
            string HRPart = string.Empty;
            if (part.Description1.Substring(0, 2) == "09")
            {
                partLabel = "P" + part.Number + serial;
                HRPart = part.Number;
            } 
            #endregion            

            #region IPL Head
            string sIPLHead;
            sIPLHead = "<STX>R<ETX>"
                     + "<STX><ESC>C<SI>h<SI>I1<ETX>"
                     + "<STX><ESC>P;F*<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";

            if (DPI == "400")
            {
                sIPLBody =          // Description
                                    "<STX>L1;o780,6;f3;l1680;w3;<ETX>" +
                                    "<STX>H2;o780,40;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>B3;o690,80;f3;c6,0;h130;w4;r0;i0;d3," + partLabel + ";<ETX>" +
                                    "<STX>H4;o580,40;f3;c30;h1;w1;d3,Decostar Part #: " + HRPart + ";<ETX>" +
                                    "<STX>H5;o530,50;f3;c30;h1;w1;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>L6;o390,6;f3;l1680;w3;<ETX>" +
                                    "<STX>H7;o390,40;f3;c30;h1;w1;d3,KIA SERVICE;<ETX>" +
                                    "<STX>B8;o320,80;f3;c0,0;h130;w3;r0;i0;d3," + part.CustomerNumber + ";<ETX>" +
                                    "<STX>H9;o100,40;f3;c30;h1;w1;d3,Customer Part #: " + part.CustomerNumber + ";<ETX>" +
                                    "<STX>L10;o20,6;f3;l1680;w3;<ETX>";
            }
            else if (DPI == "200")
            {
                sIPLBody =          // Description
                                    "<STX>L1;o390,3;f3;l840;w3;<ETX>" +
                                    "<STX>H2;o390,20;f3;c30;h2;w2;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>B3;o345,40;f3;c6,0;h80;w3;r0;i0;d3," + partLabel + ";<ETX>" +
                                    "<STX>H4;o260,20;f3;c30;h2;w2;d3,Decostar Part #: " + HRPart + ";<ETX>" +
                                    "<STX>H5;o220,25;f3;c30;h1;w2;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>L6;o195,3;f3;l840;w3;<ETX>" +
                                    "<STX>H7;o195,20;f3;c30;h2;w2;d3,KIA SERVICE;<ETX>" +
                                    "<STX>B8;o150,40;f3;c0,0;h80;w2;r0;i0;d3," + part.CustomerNumber + ";<ETX>" +
                                    "<STX>H9;o60,20;f3;c30;h2;w2;d3,Customer Part #: " + part.CustomerNumber + ";<ETX>" +
                                    "<STX>L10;o10,3;f3;l840;w3;<ETX>";
            }
            #endregion
            #region IPL Tail
            string sIPLTail = "<STX>R<ETX> " +
                                "<STX><ESC>E1<ETX> " +
                                "<STX><CAN><ETX> " +
                                "<STX><ETX> " +
                                "<STX><ETB><ETX>";
            sIPLTail = "";
            sIPLTail = "<STX>D0<ETX>"
                     + "<STX>R<ETX>"
                     + "<STX><ESC>E*,1<CAN><ETX>"
                     + "<STX><RS>1<US>1<ETB><FF><ETX>";
            #endregion

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }

            // This is only recorded for KIA parts
            if ((part.Description1.Substring(0, 2) != "09"))
                return partLabel;
            else if (part.Description1.Substring(0, 2) == "09")
            {
                database = new Models.Database();
                command = new SqlCommand();

                command.CommandText = " insert into AssemblyRecords.dbo.KIA_Assembly (part_label, mold_scan, paint_scan, part_number, print_date) "
                                    + " values (@partLabel, @moldLabel, @paintLabel, @partNumber, @time)";
                command.Parameters.AddWithValue("@partLabel", partLabel);
                command.Parameters.AddWithValue("@moldLabel", moldLabel);
                command.Parameters.AddWithValue("@paintLabel", paintLabel);
                command.Parameters.AddWithValue("@partNumber", partNumber);
                command.Parameters.AddWithValue("@time", DateTime.Now);
                database.RunCommand(command);
            }

            return partLabel;
        }

        public static void PrintKIAServiceShipLabel(string partNumber, string printer, string PONumber, string deliveryDate, string quantity)
        {
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            #region IPL Header
            string sIPLHead = "<STX><ESC>C<ETX>"
                            + "<STX><ESC>P<ETX>"
                            + "<STX>E1;F1<ETX>"
                            + "<STX><SI>g0,420<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";

            if (DPI == "400")
            {
                throw new Exception("No 400 DPI Option");
            }
            else if (DPI == "200")
            {
                sIPLBody = "<STX>H1;o35,10;f0;c26;w1;h1;k10;d3,SUPPLIER;<ETX> " +
                            "<STX>H2;o70,55;f0;c26;w1;h1;k16;d3,DECOSTAR INDUSTRIES<ETX> " +
                            "<STX>H3;o70,100;f0;c26;w1;h1;k12;d3,1 DECOMA DRIVE<ETX> " +
                            "<STX>H4;o70,140;f0;c26;w1;h1;k12;d3,CARROLLTON, GA 30117<ETX> " +
                            "<STX>L5;o650,1;;l200;f3;w1;<ETX> " +
                            "<STX>H6;o700,10;f0;c26;w1;h1;k10;d3,PART NUMBER;<ETX> " +
                            "<STX>H7;o700,70;f0;c26;w1;h3;k24;d3," + part.CustomerNumber + "<ETX> " +
                            "<STX>L8;o42,200;;l1120;f0;w1;<ETX> " +
                            "<STX>H9;o35,210;f0;c26;w1;h1;k10;d3,PO Number;<ETX> " +
                            "<STX>H10;o70,270;f0;c26;w1;h;k24;d3," + PONumber + "<ETX> " +
                            "<STX>L11;o42,400;;l1120;f0;w1;<ETX> " +
                            "<STX>H12;o450,210;f0;c26;w1;h1;k10;d3,SHIP TO:;<ETX> " +
                            "<STX>H13;o450,260;f0;c26;w1;h1;k16;d3,MOBIS ALABAMA RDC;<ETX> " +
                            "<STX>H14;o450,320;f0;c26;w1;h1;k10;d3,1385 MITCHELL YOUNG ROAD, MONTGOMERY, AL 36108;<ETX> " +
                            "<STX>L15;o430,200;;l200;f3;w1;<ETX> " +
                            "<STX>H16;o35,410;f0;c26;w1;h13;k10;d3,Quantity;<ETX> " +
                            "<STX>H17;o70,470;f0;c26;w1;h1;k18;d3," + quantity + "<ETX> " +
                            "<STX>L18;o270,400;;l200;f3;w1;<ETX> " +
                            "<STX>H19;o300,410;f0;c26;w1;h1;k10;d3,DELIVERY DATE<ETX> " +
                            "<STX>H20;o300,470;f0;c26;w1;h1;k18;d3," + deliveryDate + "<ETX> " +
                            "<STX>L21;o600,400;;l200;f3;w1;<ETX> " +
                            "<STX>H22;o620,410;f0;c26;w1;h1;k10;d3,DESCRIPTION;<ETX> " +
                            "<STX>H23;o630,470;f0;c26;w1;h4;k12;d3," + part.Description2 + ";<ETX> " +
                            "<STX>L24;o42,600;;l1120;f0;w1;<ETX> " +
                            "<STX>B25;o350,650;f0;c6,0;i0;w3;h110;d3," + part.CustomerNumber + "<ETX> " +
                            "<STX>H26;o675,600;f0;c26;w1;h1;k12;d3," + part.CustomerNumber + ";<ETX> ";
            }
            #endregion

            #region IPL Tail
            string sIPLTail = "<STX>R;<ETX><STX><CAN><ETX><STX><ESC>E1<ETX><STX><CAN><ETX><STX><ETB><ETX>";
            #endregion

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        /// <summary>
        /// Prints an F25 Pass Label
        /// </summary>
        /// <param name="serial">Label Serial</param>
        /// <param name="printer">Printer Number</param>
        public static void PrintF25PartLabel(string description, string partNumber, string serial, string assemblyTime, string machine, string printer)
        {
            Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;
            
            #region IPL Head
            string sIPLHead = "<STX><ESC>C<ETX>"
                            + "<STX><ESC>P<ETX>"
                            + "<STX>E1;F1<ETX>"
                            + "<STX><SI>g0,420<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";
            sIPLBody =          // Description
                                "<STX>L1;o135,3;f3;l420;w3;<ETX>" +
                                "<STX>H2;o135,20;f3;c30;h1;w1;d3," + description + ";<ETX>" +
                                "<STX>B3;o110,8;f3;c6,0;h65;w2;r0;i0;d3," + serial + ";<ETX>" +
                                "<STX>H4;o45,40;f3;c30;h1;w1;d3,Part #: " + partNumber + ";<ETX>" +
                                "<STX>H5;o30,20;f3;c30;h1;w1;d3,Assembled: " + assemblyTime + ";<ETX>" +
                                "<STX>H6;o30,350;f3;c30;k5;h1;w1;d3," + machine + ";<ETX>" +
                                "<STX>L7;o3,3;f3;l420;w3;<ETX>";

            #endregion
            #region IPL Tail
            string sIPLTail = "<STX>R;<ETX><STX><CAN><ETX><STX><ESC>E1<ETX><STX><CAN><ETX><STX><ETB><ETX>";
            #endregion
            #region Print
            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }            
            #endregion
        }

        public static void PrintDOTLabel(int quantity, string printer)
        {
            PrintHandler ph = new PrintHandler(printer);
            string DPI = ph.dpi;

            string sIPL = "";

            #region IPL
            if (DPI == "200")
            {
                sIPL = "<STX><ESC>E1<ETX>" +
                            "<STX><ESC>E2<ETX>" +
                            "<STX><ESC>F1<ETX>" +
                            "<STX><ESC>F2<ETX>" +
                            "<STX><ESC>C1<ETX>" +
                            "<STX><ESC>k<ETX>" +
                            "<STX><SI>L150<ETX>" +
                            "<STX><SI>S30<ETX>" +
                            "<STX><SI>d0<ETX>" +
                            "<STX><SI>h0,0;<ETX>" +
                            "<STX><SI>l8<ETX>" +
                            "<STX><SI>I2<ETX>" +
                            "<STX><SI>F0<ETX>" +
                            "<STX><SI>D0<ETX>" +
                            "<STX><SI>t0<ETX>" +
                            "<STX><SI>W410<ETX>" +
                            "<STX><SI>g1,567<ETX>" +
                            "<STX><ESC>P<ETX>" +
                            "<STX>E1;F1;<ETX>" +

                            "<STX>H0;o140,175;f3;c30;h1;w1;d3,;<ETX>" +
                            "<STX>H1;o135,40;f2;c28;h1;w1;k10;d3,GMX245;<ETX>" +
                            "<STX>H2;o135,70;f3;c28;h1;w1;k9;d3,LEXARCOAT 1000;<ETX>" +
                            "<STX>H3;o105,70;f3;c28;h1;w1;k9;d3,DOT 435    AS5;<ETX>" +
                            "<STX>H4;o75,70;f3;c28;h1;w1;k9;d3,M-203;<ETX>" +
                            "<STX>H5;o50,70;f3;c28;h1;w1;k8;d3,LEXAMAR CORP;<ETX>" +
                            "<STX>R<ETX>" +
                            "<STX><ESC>E1<ETX>" +
                            "<STX><CAN><ETX>" +
                            "<STX><ETX>" +
                            "<STX><RS>1<ETX>" +
                            "<STX><ETB><ETX>";
            }
            else if (DPI == "400")
            {
                sIPL = "<STX><ESC>E1<ETX>" +
                            "<STX><ESC>E2<ETX>" +
                            "<STX><ESC>F1<ETX>" +
                            "<STX><ESC>F2<ETX>" +
                            "<STX><ESC>C1<ETX>" +
                            "<STX><ESC>k<ETX>" +
                            "<STX><SI>L150<ETX>" +
                            "<STX><SI>S30<ETX>" +
                            "<STX><SI>d0<ETX>" +
                            "<STX><SI>h0,0;<ETX>" +
                            "<STX><SI>l8<ETX>" +
                            "<STX><SI>I2<ETX>" +
                            "<STX><SI>F0<ETX>" +
                            "<STX><SI>D0<ETX>" +
                            "<STX><SI>t0<ETX>" +
                            "<STX><SI>W410<ETX>" +
                            "<STX><SI>g1,567<ETX>" +
                            "<STX><ESC>P<ETX>" +
                            "<STX>E1;F1;<ETX>" +

                            "<STX>H0;o280,350;f3;c30;h1;w1;d3,;<ETX>" +
                            "<STX>H1;o290,46;f2;c28;h1;w1;k12;d3,GMX245;<ETX>" +
                            "<STX>H2;o290,140;f3;c30;h1;w1;k10;d3,LEXARCOAT 1000;<ETX>" +
                            "<STX>H3;o220,140;f3;c30;h1;w1;k10;d3,DOT 435    AS5;<ETX>" +
                            "<STX>H4;o160,140;f3;c30;h1;w1;k10;d3,M-203;<ETX>" +
                            "<STX>H5;o100,140;f3;c30;h1;w1;k10;d3,LEXAMAR CORP;<ETX>" +
                            "<STX>R<ETX>" +
                            "<STX><ESC>E1<ETX>" +
                            "<STX><CAN><ETX>" +
                            "<STX><ETX>" +
                            "<STX><RS>1<ETX>" +
                            "<STX><ETB><ETX>";
            }
            #endregion                       

            if (ph.SendToPrinter(sIPL, quantity) == false)
            {
                throw new Exception("Could not ping printer");
            }            
        }

        /// <summary>
        /// Prints a Pass label for Honda
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="sLocation"></param>
        /// <param name="printer"></param>
        //public static void PrintHondaLabel(string partNumber, string printer)
        //{
        //    PrintHondaLabel(partNumber, "", "", "", printer);
        //}

        /// <summary>
        /// Prints a Pass label for Honda and adds to assembly log
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="bedScan"></param>
        /// <param name="coverScan"></param>
        /// <param name="printer"></param>
        //public static void PrintHondaLabel(string partNumber, string bedScan, string coverScan, string otherPaint, string printer)
        //{
        //    Models.sPart part = new Models.Part().GetPartByPartNumber(partNumber);
        //    PrintHandler ph = new PrintHandler(printer);
        //    string DPI = ph.dpi;

        //    #region Odometer
        //    string serial = "";

        //    Models.Database database = new Models.Database();
        //    SqlCommand command = new SqlCommand();
        //    command.CommandType = System.Data.CommandType.StoredProcedure;
        //    command.CommandText = "Production.dbo.sp_getOdometerSerial5";
        //    command.Parameters.AddWithValue("@id", 1);
        //    command.Parameters.AddWithValue("@nextOne", serial);

        //    serial = database.RunSingleResultQuery(command);            
        //    #endregion                      

        //    #region IPL Head
        //    string sIPLHead = "<STX><SI>t0<ETX>" +
        //                        "<STX><SI>T1<ETX> " +
        //                        "<STX><SI>g1,567<ETX> " +
        //                        "<STX><SI>d0<ETX> " +
        //                        "<STX><SI>S40<ETX> " +
        //                        "<STX><SI>R1<ETX> " +
        //                        "<STX><SI>r115<ETX> " +
        //                        "<STX><SI>D115<ETX> " +
        //                        "<STX><SI>I6<ETX> " +
        //                        "<STX><SI>F0<ETX> " +
        //                        "<STX><ESC>C1<ETX> " +
        //                        "<STX><ESC>P<ETX> " +
        //                        "<STX>E1;F1;<ETX> ";
        //    #endregion
        //    #region IPL Body
        //    string sIPLBody = "";

        //    if (DPI == "400")
        //    {
        //        sIPLBody =          // Description
        //                            "<STX>L1;o270,6;f3;l840;w3;<ETX>" +
        //                            "<STX>H2;o270,60;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
        //                            "<STX>B3;o220,70;f3;c6,0;h130;w4;r0;i0;d3," + part.Number + sSerial + ";<ETX>" +
        //                            "<STX>H4;o90,100;f3;c30;h1;w1;d3,Part #: " + part.Number + ";<ETX>" +
        //                            "<STX>H5;o60,60;f3;c30;h1;w1;d3," + DateTime.Now.ToShortDateString() + ";<ETX>" +
        //                            "<STX>H6;o60,270;f3;c30;h1;w1;d3," + "Decostar Part Label" + ";<ETX>" +
        //                            "<STX>L7;o6,6;f3;l840;w3;<ETX>";
        //    }
        //    else if (DPI == "200")
        //    {
        //        sIPLBody =          // Description
        //                            "<STX>L1;o135,3;f3;l420;w3;<ETX>" +
        //                            "<STX>H2;o135,30;f3;c30;h1;w1;d3," + part.Description2 + ";<ETX>" +
        //                            "<STX>B3;o110,35;f3;c6,0;h65;w2;r0;i0;d3," + part.Number + sSerial + ";<ETX>" +
        //                            "<STX>H4;o45,50;f3;c30;h1;w1;d3,Part #: " + part.Number + ";<ETX>" +
        //                            "<STX>H5;o30,30;f3;c30;h1;w1;d3," + DateTime.Now.ToShortDateString() + ";<ETX>" +
        //                            "<STX>L6;o3,3;f3;l420;w3;<ETX>";
        //    }
        //    #endregion
        //    #region IPL Tail
        //    string sIPLTail = "<STX>R<ETX> " +
        //                        "<STX><ESC>E1<ETX> " +
        //                        "<STX><CAN><ETX> " +
        //                        "<STX><ETX> " +
        //                        "<STX><ETB><ETX>";
        //    #endregion

        //    string sIPL = sIPLHead + sIPLBody + sIPLTail;

        //    if (ph.SendToPrinter(sIPL) == false)
        //    {
        //        throw new Exception("Could not ping printer");
        //    }

        //    // Record the Part Label in the Database
        //    database = new Models.Database();
        //    command = new SqlCommand();
            
        //    string sHondaPassLabel = part.Number + serial;

        //    command.CommandText = " insert into AssemblyRecords.dbo.Honda_Assembly "
        //                   + "(part_label, pnt_bed_scan, pnt_cover_scan, pnt_other_scan, part_number, print_date) "
        //                   + " values ('" + sHondaPassLabel + "', '" + bedScan + "','" + coverScan + "'," + otherPaint + "','" + partNumber + "','" + DateTime.Now + "')";

        //    database.RunCommand(command);
        //}
    }


}