using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    class EZGoRackPrinter : Printer
    {
        #region Overloaded Constructor

        public EZGoRackPrinter(string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }

        #endregion

        #region Print Overrides

        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack) o;
            Models.sPart EZGoPart = new Models.Part().GetPartByPartNumber(rack.PartNumber);

            string sPtDesc2 = EZGoPart.Description2;

            if (sPtDesc2.Length > 20)
            {
                sPtDesc2 = sPtDesc2.Substring(0, 20);
            }
            
            // Calculate Shift
            double shift = 0;
            if (System.DateTime.Now.Hour >= 7.5 && System.DateTime.Now.Hour < 15.5)
            {
                shift = 1;
            }
            else if (System.DateTime.Now.Hour >= 15.5 && System.DateTime.Now.Hour < 23.5)
            {
                shift = 2;
            } 
            else
            {
                shift = 3;
            }

            string sIPLHead, sIPLBody, sIPLTail;

            #region Head
            sIPLHead =  "<STX><SI>t0<ETX>" +
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

            #region Body
            if (DPI == "400")
            {
                sIPLBody =  // PART NO. FIELD
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
                            "<STX>H13;o300,765;f0;c26;w1;h1;k22;d3,82331;<ETX>" +
                            "<STX>B14;o100,940;f0;c0,0;i0;w6;h160;d3,V82331;<ETX>" +
                            "<STX>L15;o0,1120;f0;l2300;w1;<ETX>" +
                            // SERIAL FIELD
                            "<STX>H16;o10,1125;f0;c26;w1;h1;k10;d3,SERIAL<ETX>" +
                            "<STX>H17;o10,1185;f0;c26;w1;h1;k10;d3,(S)<ETX>" +
                            "<STX>H18;o280,1120;f0;c26;w1;h1;k22;d3," + rack.RackID + ";<ETX>" +
                            "<STX>B19;o100,1300;f0;c0,0;i0;w6;h160;d3,S" + rack.RackID + ";<ETX>" +
                            "<STX>L20;o1500,1600;f1;l478;w1;<ETX>" +
                            // DECOSTAR
                            "<STX>H21;o10,1520;f0;c26;w1;h1;k10;d3,DECOSTAR INDUSTRIES, CARROLLTON GA 30117<ETX>" +
                            // VERTICAL DIVIDER
                            "<STX>L22;o920,1120;f1;l726;w1;<ETX>" +
                            // STYLE FIELD
                            "<STX>H23;o1020,420;f0;c26;w1;h1;k20;d3," + sPtDesc2 +"<ETX>" +
                            "<STX>H24;o1020,600;f0;c26;w1;h1;k20;d3," + EZGoPart.DescColor + "<ETX>" +
                            // DATE FIELD
                            "<STX>H25;o1000,770;f0;c26;w1;h1;k12;d3,FIFO DATE;<ETX>" +
                            "<STX>H27;o940,880;f0;c26;w1;h1;k10;d3," + System.DateTime.Now.ToString() + ";<ETX>" +
                            "<STX>L28;o1600,965;f1;l185;w1;<ETX>" +
                            "<STX>L29;o920,975;f0;l1400;w1;<ETX>" +
                            // CHANGE FIELD
                            "<STX>H30;o1650,850;f0;c26;w1;h1;k10;d3,ENG. CHANGE;<ETX>" +
                            "<STX>H31;o2150,830;f0;c26;w1;h1;k15;d3," + rack.Revision + ";<ETX>" +
                            // DECOSTAR FIELD
                            "<STX>H32;o1090,990;f0;c26;w1;h1;k10;d3,DECOSTAR #<ETX>" +
                            "<STX>H33;o1500,965;f0;c26;w1;h1;k20;d3," + rack.PartNumber + ";<ETX>" +
                            // INSP FIELD
                            "<STX>H34;o1570,1255;f0;c26;w1;h1;k10;d3,INSP.;<ETX>" +
                            "<STX>H35;o1590,1295;f0;c26;w1;h1;k15;d3," + " " + ";<ETX>" +
                            // SHIFT FIELD
                            "<STX>H36;o1920,1255;f0;c26;w1;h1;k10;d3,SHIFT;<ETX>" +
                            "<STX>H37;o1940,1295;f0;c26;w1;h1;k15;d3," + shift + ";<ETX>";
            }
            else if (DPI == "200")
            {
                sIPLBody =  // PART NO. FIELD
                            "<STX>H1;o5,25;f0;c26;w1;h1;k10;d3,PART NO.;<ETX>" +
                            "<STX>H2;o5,50;f0;c26;w1;h1;k10;d3,(P);<ETX>" +
                            "<STX>H3;o150,0;f0;c26;w1;h1;k36;d3," + rack.CustomerPartNumber + ";<ETX>" +
                            "<STX>B4;o50,110;f0;c0,0;i0;w3;h80;d3,P" + rack.CustomerPartNumber + ";<ETX>" +
                            "<STX>L5;o0,200;f0;l1150;w1;<ETX>" +
                            // QUANTITY FIELD
                            "<STX>H6;o5,200;f0;c26;w1;h1;k10;d3,QUANTITY;<ETX>" +
                            "<STX>H7;o5,225;f0;c26;w1;h1;k10;d3,(Q);<ETX>" +
                            "<STX>H8;o150,175;f0;c26;w1;h1;k36;d3," + rack.Quantity + ";<ETX>" +
                            "<STX>B9;o50,290;f0;c0,0;i0;w3;h80;d3,Q" + rack.Quantity + ";<ETX>" +
                            "<STX>L10;o0,390;f0;l1150;w1;<ETX>" +
                            // SUPPLIER FIELD
                            "<STX>H11;o5,392;f0;c26;w1;h1;k10;d3,SUPPLIER<ETX>" +
                            "<STX>H12;o5,420;f0;c26;w1;h1;k10;d3,(V)<ETX>" +
                            "<STX>H13;o150,382;f0;c26;w1;h1;k22;d3,82331;<ETX>" +
                            "<STX>B14;o50,470;f0;c0,0;i0;w3;h80;d3,V82331;<ETX>" +
                            "<STX>L15;o0,560;f0;l1150;w1;<ETX>" +
                            // SERIAL FIELD
                            "<STX>H16;o5,562;f0;c26;w1;h1;k10;d3,SERIAL<ETX>" +
                            "<STX>H17;o5,592;f0;c26;w1;h1;k10;d3,(S)<ETX>" +
                            "<STX>H18;o140,560;f0;c26;w1;h1;k22;d3," + rack.RackID + ";<ETX>" +
                            "<STX>B19;o50,650;f0;c0,0;i0;w3;h80;d3,S" + rack.RackID + ";<ETX>" +
                            "<STX>L20;o750,800;f1;l239;w1;<ETX>" +
                            // DECOSTAR
                            "<STX>H21;o10,1520;f0;c26;w1;h1;k10;d3,DECOSTAR INDUSTRIES, CARROLLTON GA 30117<ETX>" +
                            // VERTICAL DIVIDER
                            "<STX>L22;o450,560;f1;l363;w1;<ETX>" +
                            // STYLE FIELD
                            "<STX>H23;o510,210;f0;c26;w1;h1;k20;d3," + sPtDesc2 +"<ETX>" +
                            "<STX>H24;o510,300;f0;c26;w1;h1;k20;d3," + EZGoPart.DescColor + "<ETX>" +
                            // DATE FIELD
                            "<STX>H25;o500,385;f0;c26;w1;h1;k12;d3,FIFO DATE;<ETX>" +
                            "<STX>H27;o470,440;f0;c26;w1;h1;k10;d3," + System.DateTime.Now.ToString() + ";<ETX>" +
                            "<STX>L28;o800,482;f1;l92;w1;<ETX>" +
                            "<STX>L29;o460,487;f0;l700;w1;<ETX>" +
                            // CHANGE FIELD
                            "<STX>H30;o825,425;f0;c26;w1;h1;k10;d3,ENG. CHANGE;<ETX>" +
                            "<STX>H31;o1075,415;f0;c26;w1;h1;k15;d3," + rack.Revision + ";<ETX>" +
                            // DECOSTAR FIELD
                            "<STX>H32;o545,495;f0;c26;w1;h1;k10;d3,DECOSTAR #<ETX>" +
                            "<STX>H33;o750,482;f0;c26;w1;h1;k20;d3," + rack.PartNumber + ";<ETX>" +
                            // INSP FIELD
                            "<STX>H34;o785,627;f0;c26;w1;h1;k10;d3,INSP.;<ETX>" +
                            "<STX>H35;o795,647;f0;c26;w1;h1;k15;d3," + " " + ";<ETX>" +
                            // SHIFT FIELD
                            "<STX>H36;o960,627;f0;c26;w1;h1;k10;d3,SHIFT;<ETX>" +
                            "<STX>H37;o970,647;f0;c26;w1;h1;k15;d3," + shift + ";<ETX>";
            }
            else
            {
                throw new Exception("Printer dpi not a valid value.");
            }
            #endregion

            #region Tail
            sIPLTail =  "<STX>R<ETX> " +
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
