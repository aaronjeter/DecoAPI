using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    class CCRackPrinter : Printer
    {
        #region Overloaded Constructor

        public CCRackPrinter(string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) 
        {
            sPO = "136493";
        }

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

            // Calculate Shift
            //Wasn't being used, commented out.
            //double shift = 0;
            //if (System.DateTime.Now.Hour >= 7.5 && System.DateTime.Now.Hour < 15.5)
            //{
            //    shift = 1;
            //}
            //else if (System.DateTime.Now.Hour >= 15.5 && System.DateTime.Now.Hour < 23.5)
            //{
            //    shift = 2;
            //}
            //else
            //{
            //    shift = 3;
            //}

            #region IPL Head
            string sIPLHead =   "<STX><SI>t0<ETX>" +
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
                sIPLBody =          // PART NO. FIELD
                                    "<STX>H1;o10,50;f0;c26;w1;h1;k10;d3,PART NO.;<ETX>" +
                                    "<STX>H2;o10,100;f0;c26;w1;h1;k10;d3,(P);<ETX>" +
                                    "<STX>H3;o300,0;f0;c26;w1;h1;k36;d3," + rack.CustomerPartNumber + ";<ETX>" +
                                    "<STX>B4;o100,220;f0;c0,0;i0;w6;h160;d3,P" + rack.CustomerPartNumber + ";<ETX>" +
                                    "<STX>L5;o0,395;f0;l2300;w1;<ETX>" +
                                    // QUANTITY FIELD
                                    "<STX>H6;o10,400;f0;c26;w1;h1;k10;d3,QUANTITY;<ETX>" +
                                    "<STX>H7;o10,450;f0;c26;w1;h1;k10;d3,(Q);<ETX>" +
                                    "<STX>H8;o300,350;f0;c26;w1;h1;k36;d3," + rack.Quantity + ";<ETX>" +
                                    "<STX>B9;o100,580;f0;c0,0;i0;w6;h160;d3,Q" + rack.Quantity + ";<ETX>" +
                                    "<STX>L10;o0,780;f0;l2300;w1;<ETX>" +
                                    // SUPPLIER FIELD
                                    "<STX>H11;o10,785;f0;c26;w1;h1;k10;d3,SUPPLIER<ETX>" +
                                    "<STX>H12;o10,840;f0;c26;w1;h1;k10;d3,(V)<ETX>" +
                                    "<STX>H13;o300,765;f0;c26;w1;h1;k22;d3,004580;<ETX>" +
                                    "<STX>B14;o100,940;f0;c0,0;i0;w6;h160;d3,V004580;<ETX>" +
                                    "<STX>L15;o0,1120;f0;l2300;w1;<ETX>" +
                                    // SERIAL FIELD
                                    "<STX>H16;o10,1125;f0;c26;w1;h1;k10;d3,SERIAL<ETX>" +
                                    "<STX>H17;o10,1185;f0;c26;w1;h1;k10;d3,(S)<ETX>" +
                                    "<STX>H18;o280,1120;f0;c26;w1;h1;k22;d3,DQ" + rack.RackID + ";<ETX>" +
                                    "<STX>B19;o100,1300;f0;c0,0;i0;w6;h160;d3,SDQ" + rack.RackID + ";<ETX>" +
                                    // DECOSTAR
                                    "<STX>H20;o10,1520;f0;c26;w1;h1;k10;d3,DECOSTAR INDUSTRIES, CARROLLTON GA 30117<ETX>" +
                                    // VERTICAL: Supplier/Part Description
                                    "<STX>L21;o1020,1120;f1;l340;w1;<ETX>" +
                                    // PURCHASE ORDER FIELD                 
                                    "<STX>B22;o1020,420;f0;c0,0;i0;w6;h160;d3,K" + "P" + sPO + "<ETX>" +
                                    "<STX>H23;o1020,570;f0;c26;w1;h1;k10;d3,PURCHASE ORDER NO.<ETX>" +
                                    "<STX>H24;o1025,615;f0;c26;w1;h1;k10;d3,(K)<ETX>" +
                                    "<STX>H25;o1560,610;f0;c26;w1;h1;k20;d3," + "P" + sPO + "<ETX>" +
                                    // PART DESCRIPTION FIELD
                                    "<STX>H26;o1090,790;f0;c26;w1;h1;k10;d3,PART DESCRIPTION;<ETX>" +
                                    "<STX>H27;o1090,850;f0;c26;w1;h1;k12;d3," + part.DescFull + ";<ETX>" +
                                    "<STX>L28;o1025,945;f0;l1275;w1;<ETX>" +
                                    // CHANGE FIELD
                                    "<STX>H29;o1090,960;f0;c26;w1;h1;k10;d3,ENGINEERING CHANGE<ETX>" +
                                    "<STX>H30;o1200,1000;f0;c26;w1;h1;k15;d3," + "A" + ";<ETX>" +
                                    // VERTICAL: Change/Color
                                    "<STX>L39;o1780,1120;f1;l170;w1;<ETX>" +
                                    // COLOR FIELD
                                    "<STX>H40;o1850,960;f0;c26;w1;h1;k10;d3,COLOR<ETX>" +
                                    "<STX>H41;o1850,1000;f0;c26;w1;h1;k15;d3," + part.DescColor + ";<ETX>" +
                                    // VERTICAL: SERIAL/DELIVERY
                                    "<STX>L31;o1400,1540;f1;l420;w1;<ETX>" +
                                    // DECOSTAR INFORMATION FIELD
                                    "<STX>H32;o1470,1125;f0;c26;w1;h1;k10;d3,DECOSTAR # " + rack.PartNumber + ";<ETX>" +
                                    "<STX>H33;o1470,1170;f0;c26;w1;h1;k10;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>L34;o1400,1250;f0;l900;w1;<ETX>" +
                                    // DELIVERY FIELD
                                    "<STX>H35;o1470,1255;f0;c26;w1;h1;k10;d3,DELIVERY LOCATION;<ETX>" +
                                    "<STX>H36;o1490,1305;f0;c26;w1;h1;k10;d3,CLUB CAR, INC.;<ETX>" +
                                    "<STX>H37;o1490,1370;f0;c26;w1;h1;k10;d3,4126 WASHINGTON ROAD;<ETX>" +
                                    "<STX>H38;o1490,1430;f0;c26;w1;h1;k10;d3,EVANS, GA 30809;<ETX>";
            }
            else if (DPI == "200")
            {
                    sIPLBody =      // PART NO. FIELD
                                    "<STX>H1;o5,25;f0;c26;w1;h1;k10;d3,PART NO.;<ETX>" +
                                    "<STX>H2;o5,50;f0;c26;w1;h1;k10;d3,(P);<ETX>" +
                                    "<STX>H3;o150,0;f0;c26;w1;h1;k36;d3," + rack.CustomerPartNumber + ";<ETX>" +
                                    "<STX>B4;o50,110;f0;c0,0;i0;w3;h80;d3,P" + rack.CustomerPartNumber + ";<ETX>" +
                                    "<STX>L5;o0,197;f0;l1150;w1;<ETX>" +
                                    // QUANTITY FIELD
                                    "<STX>H6;o5,200;f0;c26;w1;h1;k10;d3,QUANTITY;<ETX>" +
                                    "<STX>H7;o5,225;f0;c26;w1;h1;k10;d3,(Q);<ETX>" +
                                    "<STX>H8;o150,175;f0;c26;w1;h1;k36;d3," + rack.Quantity + ";<ETX>" +
                                    "<STX>B9;o50,290;f0;c0,0;i0;w3;h80;d3,Q" + rack.Quantity + ";<ETX>" +
                                    "<STX>L10;o0,390;f0;l1150;w1;<ETX>" +
                                    // SUPPLIER FIELD
                                    "<STX>H11;o5,392;f0;c26;w1;h1;k10;d3,SUPPLIER<ETX>" +
                                    "<STX>H12;o5,420;f0;c26;w1;h1;k10;d3,(V)<ETX>" +
                                    "<STX>H13;o150,382;f0;c26;w1;h1;k22;d3,004580;<ETX>" +
                                    "<STX>B14;o50,470;f0;c0,0;i0;w3;h80;d3,V004580;<ETX>" +
                                    "<STX>L15;o0,560;f0;l1150;w1;<ETX>" +
                                    // SERIAL FIELD
                                    "<STX>H16;o5,562;f0;c26;w1;h1;k10;d3,SERIAL<ETX>" +
                                    "<STX>H17;o5,592;f0;c26;w1;h1;k10;d3,(S)<ETX>" +
                                    "<STX>H18;o140,560;f0;c26;w1;h1;k22;d3,DQ" + rack.RackID + ";<ETX>" +
                                    "<STX>B19;o50,650;f0;c0,0;i0;w3;h80;d3,SDQ" + rack.RackID + ";<ETX>" +
                                    // DECOSTAR
                                    "<STX>H20;o5,760;f0;c26;w1;h1;k10;d3,DECOSTAR INDUSTRIES, CARROLLTON GA 30117<ETX>" +
                                    // VERTICAL: Supplier/Part Description
                                    "<STX>L21;o510,560;f1;l170;w1;<ETX>" +
                                    // PURCHASE ORDER FIELD                 
                                    "<STX>B22;o560,210;f0;c0,0;i0;w3;h80;d3,K" + "P" + sPO + "<ETX>" +
                                    "<STX>H23;o560,285;f0;c26;w1;h1;k10;d3,PURCHASE ORDER NO.<ETX>" +
                                    "<STX>H24;o512,307;f0;c26;w1;h1;k10;d3,(K)<ETX>" +
                                    "<STX>H25;o933,305;f0;c26;w1;h1;k20;d3," + "P" + sPO + "<ETX>" +
                                    // PART DESCRIPTION FIELD
                                    "<STX>H26;o545,395;f0;c26;w1;h1;k10;d3,PART DESCRIPTION;<ETX>" +
                                    "<STX>H27;o545,425;f0;c26;w1;h1;k12;d3," + part.DescFull + ";<ETX>" +
                                    "<STX>L28;o512,472;f0;l637;w1;<ETX>" +
                                    // CHANGE FIELD
                                    "<STX>H29;o545,480;f0;c26;w1;h1;k10;d3,ENGINEERING CHANGE<ETX>" +
                                    "<STX>H30;o600,500;f0;c26;w1;h1;k15;d3," + "A" + ";<ETX>" +
                                    // VERTICAL: Change/Color
                                    "<STX>L39;o890,560;f1;l85;w1;<ETX>" +
                                    // COLOR FIELD
                                    "<STX>H40;o925,480;f0;c26;w1;h1;k10;d3,COLOR<ETX>" +
                                    "<STX>H41;o925,500;f0;c26;w1;h1;k15;d3," + part.DescColor + ";<ETX>" +
                                    // VERTICAL: SERIAL/DELIVERY
                                    "<STX>L31;o700,770;f1;l210;w1;<ETX>" +
                                    // DECOSTAR INFORMATION FIELD
                                    "<STX>H32;o735,562;f0;c26;w1;h1;k10;d3,DECOSTAR # " + rack.PartNumber + ";<ETX>" +
                                    "<STX>H33;o735,585;f0;c26;w1;h1;k10;d3," + DateTime.Now + ";<ETX>" +
                                    "<STX>L34;o700,625;f0;l450;w1;<ETX>" +
                                    // DELIVERY FIELD
                                    "<STX>H35;o735,627;f0;c26;w1;h1;k10;d3,DELIVERY LOCATION;<ETX>" +
                                    "<STX>H36;o745,652;f0;c26;w1;h1;k10;d3,CLUB CAR, INC.;<ETX>" +
                                    "<STX>H37;o745,685;f0;c26;w1;h1;k10;d3,4126 WASHINGTON ROAD;<ETX>" +
                                    "<STX>H38;o745,715;f0;c26;w1;h1;k10;d3,EVANS, GA 30809;<ETX>";
            }
            #endregion

            #region IPL Tail
            string sIPLTail =   "<STX>R<ETX> " +
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
