using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    class RahauRackPrinter : Printer
    {
        #region Overloaded Constructor

        public RahauRackPrinter (string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }

        #endregion

        #region Print Overrides
        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            if (rack.Type != Models.RackType.MercedesRack)
            {
                throw new Exception("Can only print Mercedes Racks");
            }
            
            Models.sPart part = new Models.Part().GetPartByPartNumber(rack.PartNumber);
            string sPtDesc2 = part.Description2;

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

			string partDesc1 = part.Description1;
            string partDesc2 = part.Description2;
            string colorDesc = part.DescColor;
            string revision = part.RevisionLevel;
			int quantity = 2;

            string sIPL = "<STX><SI>t0<ETX>" +
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
                "<STX>E1;F1;<ETX> " +
                // PART NO. FIELD
                "<STX>H1;o10,50;f0;c26;w1;h1;k10;d3,PART NO.;<ETX>" +
                "<STX>H2;o10,100;f0;c26;w1;h1;k10;d3,(P);<ETX>" +
                "<STX>H3;o300,0;f0;c26;w1;h1;k36;d3," + rack.RahauPartNumber + ";<ETX>" +
                "<STX>B4;o100,220;f0;c0,0;i0;w6;h160;d3,P" + rack.RahauPartNumber + ";<ETX>" +
                "<STX>L5;o0,395;f0;l2300;w1;<ETX>" +
                // QUANTITY FIELD
                "<STX>H6;o10,400;f0;c26;w1;h1;k10;d3,QUANTITY;<ETX>" +
                "<STX>H7;o10,450;f0;c26;w1;h1;k10;d3,(Q);<ETX>" +
                "<STX>H8;o300,350;f0;c26;w1;h1;k36;d3," + rack.Quantity + ";<ETX>" +
                "<STX>B9;o100,580;f0;c0,0;i0;w6;h160;d3,Q" + rack.Quantity + ";<ETX>" +
                "<STX>L10;o0,780;f0;l2300;w1;<ETX>" +
                // PURCHASE ORDER FIELD
                "<STX>H11;o10,785;f0;c26;w1;h1;k10;d3,PO<ETX>" +
                "<STX>H12;o10,840;f0;c26;w1;h1;k10;d3,(O)<ETX>" +
                "<STX>H13;o300,765;f0;c26;w1;h1;k22;d3," + rack.PurchaseOrder + ";<ETX>" +
                "<STX>B14;o100,940;f0;c0,0;i0;w6;h160;d3,O" + rack.PurchaseOrder + ";<ETX>" +
                "<STX>L15;o0,1120;f0;l2300;w1;<ETX>" +
                "<STX>L16;o1020,1120;f1;700;w1;<ETX>" +
                // SERIAL FIELD
                "<STX>H17;o10,1125;f0;c26;w1;h1;k10;d3,SERIAL<ETX>" +
                "<STX>H18;o10,1185;f0;c26;w1;h1;k10;d3,(S)<ETX>" +
                "<STX>H19;o280,1120;f0;c26;w1;h1;k22;d3," + rack.RackID + ";<ETX>" +
                "<STX>B20;o100,1300;f0;c0,0;i0;w6;h160;d3,S" + rack.RackID + ";<ETX>" +
                "<STX>L21;o1500,1600;f1;l478;w1;<ETX>" +
                // DECOSTAR
                "<STX>H22;o10,1520;f0;c26;w1;h1;k10;d3,DECOSTAR INDUSTRIES, CARROLLTON GA 30117<ETX>" +
                // VERTICAL DIVIDER
                "<STX>L23;o920,920;f1;l526;w1;<ETX>" +
                // STYLE FIELD
                "<STX>H24;o1020,420;f0;c26;w1;h1;k20;d3," + sPtDesc2 + "<ETX>" +
                "<STX>H25;o1020,600;f0;c26;w1;h1;k20;d3," + part.DescColor + "<ETX>" +
                // DATE FIELD
                "<STX>H26;o1000,770;f0;c26;w1;h1;k12;d3,FIFO DATE;<ETX>" +
                "<STX>H27;o940,830;f0;c26;w1;h1;k10;d3," + System.DateTime.Now.ToString() + ";<ETX>" +
                "<STX>L28;o1700,965;f1;l185;w1;<ETX>" +
                "<STX>L29;o920,925;f0;l1400;w1;<ETX>" +
                // CHANGE FIELD
                "<STX>H30;o1750,850;f0;c26;w1;h1;k10;d3,ENG. CHANGE;<ETX>" +
                "<STX>H31;o2200,830;f0;c26;w1;h1;k15;d3," + rack.Revision + ";<ETX>" +
                // DECOSTAR FIELD
                "<STX>H32;o1090,990;f0;c26;w1;h1;k10;d3,DECOSTAR #<ETX>" +
                "<STX>H33;o1500,965;f0;c26;w1;h1;k20;d3," + rack.PartNumber + ";<ETX>" +
                // INSP FIELD
                "<STX>H34;o1570,1255;f0;c26;w1;h1;k10;d3,INSP.;<ETX>" +
                "<STX>H35;o1590,1295;f0;c26;w1;h1;k15;d3," + " " + ";<ETX>" +
                // SHIFT FIELD
                "<STX>H36;o1920,1255;f0;c26;w1;h1;k10;d3,SHIFT;<ETX>" +
                "<STX>H37;o1940,1295;f0;c26;w1;h1;k15;d3," + shift + ";<ETX>" +
                "<STX>R<ETX> " +
                "<STX><ESC>E1<ETX> " +
                "<STX><CAN><ETX> " +
                "<STX><ETX> " +
                "<STX><ETB><ETX>";

            PrintHandler ph = new PrintHandler(this.sID);
            if (ph.SendToPrinter(sIPL, quantity) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        protected override void printZPL(object o)
        {            
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}
