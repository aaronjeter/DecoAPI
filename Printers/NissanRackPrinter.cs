using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DecoAPI.Printers
{
    public class NissanRackPrinter : Printer
    {
        private string sLocation;

        #region Overloaded Constructor
        public NissanRackPrinter(string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }

        //public NissanRackPrinter(string sID, string application, Rack.Rack oRack, string sLocation) : base(sID, application, oRack) 
        //{
        //    this.sLocation = sLocation;
        //}
        #endregion
        #region Print Overrides

        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            Models.sPart part = new Models.Part().GetPartByPartNumber(rack.PartNumber);

            string sPtDesc2 = part.Description2;

            if (sPtDesc2.Length > 20)
            {
                sPtDesc2 = sPtDesc2.Substring(0, 20);
            }

            string largeMarking = string.Empty;
            string specialMarking = string.Empty;

            #region Special Markings
            // Search Based on Ship Location & pt_desc1
            string query = " select    * "
                          + " from      RackRecords.dbo.NissanSpecialMarking "
                          + " where     Location = '" + sLocation + "' "
                          + " and       General_Desc = '" + part.Description1.Substring(6, 3) + "' ";

            //Open DB Connection
            Models.Database database = new Models.Database();

            SqlDataReader reader = database.RunCommandReturnReader(query);
            largeMarking = reader["Large_Letters"].ToString();
            specialMarking = reader["Small_Letters"].ToString();
            database.CloseConnection();
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
                sIPLBody =          // PART NO. FIELD
                                    "<STX>H1;o10,50;f0;c26;w1;h1;k10;d3,PART NO.;<ETX>" +
                                    "<STX>H2;o10,100;f0;c26;w1;h1;k10;d3,(P);<ETX>" +
                                    "<STX>H3;o300,0;f0;c26;w1;h1;k36;d3," + part.CustomerNumber + ";<ETX>" +
                                    "<STX>B4;o100,220;f0;c0,0;i0;w6;h160;d3,P" + part.CustomerNumber + ";<ETX>" +
                                    "<STX>L5;o0,395;f0;l2300;w1;<ETX>" +
                                    // QUANTITY FIELD
                                    "<STX>H6;o10,400;f0;c26;w1;h1;k10;d3,QUANTITY;<ETX>" +
                                    "<STX>H7;o10,450;f0;c26;w1;h1;k10;d3,(Q);<ETX>" +
                                    "<STX>H8;o300,350;f0;c26;w1;h1;k36;d3," + rack.Quantity + ";<ETX>" +
                                    "<STX>B9;o100,580;f0;c0,0;i0;w6;h160;d3,Q" + rack.Quantity + ";<ETX>" +
                                    "<STX>H35;o730,400;f0;c26;w1;h1;k20;d3,EA;<ETX>" +
                                    "<STX>L10;o0,780;f0;l2300;w1;<ETX>" +
                                    // SUPPLIER FIELD
                                    "<STX>H11;o10,785;f0;c26;w1;h1;k10;d3,SUPPLIER<ETX>" +
                                    "<STX>H12;o10,840;f0;c26;w1;h1;k10;d3,(V)<ETX>" +
                                    "<STX>H13;o300,765;f0;c26;w1;h1;k22;d3,1063100;<ETX>" +
                                    "<STX>B14;o100,940;f0;c0,0;i0;w5;h160;d3,V1063100;<ETX>" +
                                    "<STX>L15;o0,1120;f0;l1150;w1;<ETX>" +
                                    // SERIAL FIELD
                                    "<STX>H16;o10,1125;f0;c26;w1;h1;k10;d3,SERIAL<ETX>" +
                                    "<STX>H17;o10,1185;f0;c26;w1;h1;k10;d3,(S)<ETX>" +
                                    "<STX>H18;o280,1120;f0;c26;w1;h1;k22;d3," + rack.RackID + ";<ETX>" +
                                    "<STX>B19;o100,1300;f0;c0,0;i0;w5;h160;d3,S" + rack.RackID + ";<ETX>" +
                                    // DESCRIPTION
                                    "<STX>H30;o950,400;f0;c26;w1;h1;k12;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>L29;o900,500;f0;l1400;w1;<ETX>" +
                                    // VERTICAL: Quantity/RAN
                                    "<STX>L21;o900,780;f1;l385;w1;<ETX>" +
                                    // RAN Field (A)
                                    "<STX>H23;o950,500;f0;c26;w1;h1;k10;d3,RAN (A)<ETX>" +
                                    "<STX>H25;o1450,475;f0;c26;w1;h1;k20;d3," + rack.RAN + "<ETX>" +
                                    "<STX>B22;o950,600;f0;c0,0;i0;w6;h160;d3,A" + rack.RAN + "<ETX>" +
                                    // SPECIAL MARKING FIELD
                                    "<STX>H26;o1200,790;f0;c26;w1;h1;k10;d3,SPECIAL MARKING;<ETX>" +
                                    "<STX>H27;o1200,770;f0;c26;w1;h1;k40;d3," + largeMarking + ";<ETX>" +
                                    "<STX>H28;o1200,1150;f0;c26;w1;h1;k20;d3," + specialMarking + ";<ETX>" +
                                    "<STX>L34;o1150,1310;f0;l1150;w1;<ETX>" +
                                    // VERTICAL: SERIAL/DATE
                                    "<STX>L31;o1150,1540;f1;l760;w1;<ETX>" +
                                    // DATE
                                    "<STX>H32;o1190,1310;f0;c26;w1;h1;k12;d3," + "MFG. DATE" + ";<ETX>" +
                                    "<STX>H33;o1190,1385;f0;c26;w1;h1;k12;d3," + DateTime.Now.ToString() + ";<ETX>" +
                                    "<STX>L36;o1150,1480;f0;l1150;w1;<ETX>" +
                                    // DECOSTAR PART #
                                    "<STX>H37;o1190,1500;f0;c26;w1;h1;k12;d3," + "DECOSTAR # " + part.Number + "   " + sLocation + ";<ETX>" +
                                    // DECOSTAR
                                    "<STX>H20;o10,1520;f0;c26;w1;h1;k8;d3,DECOSTAR INDUSTRIES, CARROLLTON GA 30117<ETX>";
            }
            else if (DPI == "200")
            {
                sIPLBody =          // PART NO. FIELD
                                    "<STX>H1;o5,25;f0;c26;w1;h1;k10;d3,PART NO.;<ETX>" +
                                    "<STX>H2;o5,50;f0;c26;w1;h1;k10;d3,(P);<ETX>" +
                                    "<STX>H3;o150,0;f0;c26;w1;h1;k36;d3," + part.CustomerNumber + ";<ETX>" +
                                    "<STX>B4;o50,110;f0;c0,0;i0;w3;h80;d3,P" + part.CustomerNumber + ";<ETX>" +
                                    "<STX>L5;o0,200;f0;l1150;w1;<ETX>" +
                                    // QUANTITY FIELD
                                    "<STX>H6;o5,200;f0;c26;w1;h1;k10;d3,QUANTITY;<ETX>" +
                                    "<STX>H7;o5,225;f0;c26;w1;h1;k10;d3,(Q);<ETX>" +
                                    "<STX>H8;o150,175;f0;c26;w1;h1;k36;d3," + rack.Quantity + ";<ETX>" +
                                    "<STX>B9;o50,290;f0;c0,0;i0;w3;h80;d3,Q" + rack.Quantity + ";<ETX>" +
                                    "<STX>H35;o365,200;f0;c26;w1;h1;k20;d3,EA;<ETX>" +
                                    "<STX>L10;o0,390;f0;l1150;w1;<ETX>" +
                                    // SUPPLIER FIELD
                                    "<STX>H11;o5,392;f0;c26;w1;h1;k10;d3,SUPPLIER<ETX>" +
                                    "<STX>H12;o5,420;f0;c26;w1;h1;k10;d3,(V)<ETX>" +
                                    "<STX>H13;o150,382;f0;c26;w1;h1;k22;d3,1063100;<ETX>" +
                                    "<STX>B14;o50,470;f0;c0,0;i0;w2;h80;d3,V1063100;<ETX>" +
                                    "<STX>L15;o0,560;f0;l575;w1;<ETX>" +
                                    // SERIAL FIELD
                                    "<STX>H16;o5,562;f0;c26;w1;h1;k10;d3,SERIAL<ETX>" +
                                    "<STX>H17;o5,592;f0;c26;w1;h1;k10;d3,(S)<ETX>" +
                                    "<STX>H18;o140,560;f0;c26;w1;h1;k22;d3," + rack.RackID + ";<ETX>" +
                                    "<STX>B19;o50,650;f0;c0,0;i0;w2;h80;d3,S" + rack.RackID + ";<ETX>" +
                                    // DESCRIPTION
                                    "<STX>H30;o475,200;f0;c26;w1;h1;k12;d3," + part.Description2 + ";<ETX>" +
                                    "<STX>L29;o450,250;f0;l700;w1;<ETX>" +
                                    // VERTICAL: Quantity/RAN
                                    "<STX>L21;o450,390;f1;l192;w1;<ETX>" +
                                    // RAN Field (A)
                                    "<STX>H23;o475,250;f0;c26;w1;h1;k10;d3,RAN (A)<ETX>" +
                                    "<STX>H25;o725,237;f0;c26;w1;h1;k20;d3," + rack.RAN + "<ETX>" +
                                    "<STX>B22;o475,300;f0;c0,0;i0;w3;h80;d3,A" + rack.RAN + "<ETX>" +
                                    // SPECIAL MARKING FIELD
                                    "<STX>H26;o600,395;f0;c26;w1;h1;k10;d3,SPECIAL MARKING;<ETX>" +
                                    "<STX>H27;o600,385;f0;c26;w1;h1;k40;d3," + largeMarking + ";<ETX>" +
                                    "<STX>H28;o600,575;f0;c26;w1;h1;k20;d3," + specialMarking + ";<ETX>" +
                                    "<STX>L34;o575,655;f0;l575;w1;<ETX>" +
                                    // VERTICAL: SERIAL/DATE
                                    "<STX>L31;o575,770;f1;l380;w1;<ETX>" +
                                    // DATE
                                    "<STX>H32;o595,655;f0;c26;w1;h1;k12;d3," + "MFG. DATE" + ";<ETX>" +
                                    "<STX>H33;o595,692;f0;c26;w1;h1;k12;d3," + DateTime.Now.ToString() + ";<ETX>" +
                                    "<STX>L36;o575,740;f0;l575;w1;<ETX>" +
                                    // DECOSTAR PART #
                                    "<STX>H37;o595,750;f0;c26;w1;h1;k12;d3," + "DECOSTAR # " + part.Number + "   " + sLocation + ";<ETX>" +
                                    // DECOSTAR
                                    "<STX>H20;o5,760;f0;c26;w1;h1;k8;d3,DECOSTAR INDUSTRIES, CARROLLTON GA 30117<ETX>";
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

            PrintHandler ph = new PrintHandler(this.sID);
            if (ph.SendToPrinter(sIPL, 2) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        protected override void printZPL(object label)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
