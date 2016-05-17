using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    public class InternalRackPrinter : Printer
    {
        #region Overloaded Constructor

        public InternalRackPrinter(string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }

        #endregion

        #region Print Overrides
        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            Models.RackType type = rack.Type;

            if (type != Models.RackType.GMT900Rack && type != Models.RackType.MercedesRack && type != Models.RackType.KIARack && type != Models.RackType.EZGoRack && type != Models.RackType.CCRack && type != Models.RackType.BMWRack && type != Models.RackType.BMWE7Rack)
            {
                throw new Exception("Part not set for Internal Rack");
            }
            
            Models.sPart part = new Models.Part().GetPartByPartNumber(rack.PartNumber);

			string partDesc1 = part.Description1;
			string partDesc2 = part.Description2;
			string colorDesc = part.DescColor;
			string revision		  = part.RevisionLevel;
			int quantity = 2;


            #region IPL Head
            string sIPLHead = "<STX><ESC>C1<ETX> " +
                    "<STX><ESC>k<ETX> " +
                    "<STX><SI>L1200<ETX> " +
                    "<STX><SI>W809<ETX> " +
                    "<STX><SI>S30<ETX> " +
                    "<STX><SI>d0<ETX> " +
                    "<STX><SI>l8<ETX> " +
                    "<STX><SI>I2<ETX> " +
                    //"<STX><SI>F0<ETX> " +
                    "<STX><SI>D0<ETX> " +
                    "<STX><SI>t0<ETX> " +
                    "<STX><SI>k<ETX> " +
                    "<STX><SI>h0,0<ETX> " +
                    //"<STX><SI>g1,567<ETX> " +
                    "<STX><SI>T1<ETX> " +
                    "<STX><SI>R1<ETX> " +
                    "<STX><SI>r0<ETX> " +
                    "<STX><ESC>P<ETX> " +
                    "<STX>E2;F2;<ETX> " +
                    "<STX>L1;<ETX> " +
                    "<STX>D1;<ETX> ";
            // Check if this is an internal Do Not Ship label to go outside on the south pad pending rework
            if ((application == "rack_internal_single.aspx") || (application == "rack_internal_double.aspx"))
                sIPLHead += "<STX><SI>g0,420<ETX> ";
            else
                sIPLHead += "<STX><SI>g1,567<ETX> ";
            #endregion

            #region IPL Body
            string sIPLBody = "";
            if (DPI == "400")
            {
                sIPLBody =  "<STX>B1;o212,352;f0;c6,0;i0;w8;h130;d3," + rack.PartNumber + "<ETX> " +
                            //"<STX>H2;o148,2;f0;c26;w1;h1;k12;d3,Part #;<ETX> " +
                            "<STX>H3;o148,576;f0;c26;w1;h1;k12;d3,Part Description;<ETX> " +
                            "<STX>B4;o512,1200;f3;c6,0;i0;w8;h160;d3," + rack.Quantity + "<ETX> " +
                            "<STX>H5;o148,1088;f0;c26;w1;h1;k12;d3,Quantity;<ETX> " +
                            "<STX>B6;o600,1245;f0;c6,0;i0;w9;h360;d3,|RCK|" + rack.RackID + "<ETX> " +
                            "<STX>H7;o1550,1088;f0;c26;w1;h1;k12;d3,Rack #;<ETX> " +
                            "<STX>H8;o148,672;f0;c26;w1;h1;k27;d3," + partDesc1 + "<ETX> " +
                            "<STX>H9;o580,1020;f0;c26;w1;h1;k36;d3," + rack.RackID + "<ETX> " +
                            "<STX>H10;o1440,128;f0;c26;w1;h1;k27;d3,DO NOT SHIP;<ETX> " +
                            "<STX>H11;o148,832;f0;c26;w1;h1;k27;d3," + partDesc2 + "<ETX> " +
                            "<STX>L12;o84,544;;l2240;f0;w1;<ETX> " +
                            "<STX>L13;o84,1056;;l2240;f0;w1;<ETX> " +
                            "<STX>H14;o70,1;f0;c26;w200;h200;k56;d3," + rack.PartNumber + "<ETX> " +
                            "<STX>H15;o60,1225;f0;c26;w1;h1;k36;d3," + rack.Quantity + "<ETX> " +
                            "<STX>H16;o1472,320;f0;c26;w1;h1;k12;d3," + DateTime.Now.ToString() + "<ETX> " +
                            "<STX>L17;o540,1056;;l540;f3;w1;<ETX> " +
                            "<STX>L18;o1400,32;;l512;f3;w1;<ETX> " +
                            "<STX>H19;o1108,576;f0;c26;w1;h1;k18;d3," + colorDesc + "<ETX> " +
                            "<STX>H22;o52,768;f3;c26;w1;h1;k8;d3," + application + "<ETX> " +
                            "<STX>H23;o1950,1088;f0;c26;w1;h1;k12;d3,Revision;<ETX> " +
                            "<STX>H24;o1950,1225;f0;c26;w1;h1;k36;d3," + revision + ";<ETX> " +
                            "<STX>L25;o1900,1056;;l512;f3;w1;<ETX> ";
            }
            else if (DPI == "200")
            {
                sIPLBody = "<STX>B1;o101,176;f0;c6,0;i0;w4;h80;d3," + rack.PartNumber + "<ETX> " +
                            //"<STX>H2;o74,32;f0;c26;w1;h1;k12;d3,Part #;<ETX> " +
                            "<STX>H3;o74,288;f0;c26;w1;h1;k12;d3,Part Description;<ETX> " +
                            "<STX>B4;o256,600;f3;c6,0;i0;w4;h80;d3," + rack.Quantity + "<ETX> " +
                            "<STX>H5;o74,544;f0;c26;w1;h1;k12;d3,Quantity;<ETX> " +
                            "<STX>B6;o300,623;f0;c6,0;i0;w4;h130;d3,|RCK|" + rack.RackID + "<ETX> " +
                            "<STX>H7;o780,544;f0;c26;w1;h1;k12;d3,Rack #;<ETX> " +
                            "<STX>H8;o74,336;f0;c26;w1;h1;k27;d3," + partDesc1 + "<ETX> " +
                            "<STX>H9;o290,510;f0;c26;w1;h1;k36;d3," + rack.RackID + "<ETX> " +
                            "<STX>H10;o720,64;f0;c26;w1;h1;k27;d3,DO NOT SHIP;<ETX> " +
                            "<STX>H11;o74,416;f0;c26;w1;h1;k27;d3," + partDesc2 + "<ETX> " +
                            "<STX>L12;o42,272;;l1120;f0;w1;<ETX> " +
                            "<STX>L13;o42,523;;l1120;f0;w1;<ETX> " +
                            "<STX>H14;o35,1;f0;c26;w200;h200;k56;d3," + rack.PartNumber + "<ETX> " +
                            "<STX>H15;o30,612;f0;c26;w1;h1;k36;d3," + rack.Quantity + "<ETX> " +
                            "<STX>H16;o736,160;f0;c26;w1;h1;k12;d3," + DateTime.Now.ToString() + "<ETX> " +
                            "<STX>L17;o270,528;;l270;f3;w1;<ETX> " +
                            "<STX>L18;o700,16;;l256;f3;w1;<ETX> " +
                            "<STX>H19;o554,288;f0;c26;w1;h1;k18;d3," + colorDesc + "<ETX> " +
                            "<STX>H22;o26,384;f3;c26;w1;h1;k8;d3," + application + "<ETX> " +
                            "<STX>H23;o975,544;f0;c26;w1;h1;k12;d3,Revision;<ETX> " +
                            "<STX>H24;o975,612;f0;c26;w1;h1;k36;d3," + revision + ";<ETX> " +
                            "<STX>L25;o950,528;;l256;f3;w1;<ETX> ";
            }
            else
            {
                throw new Exception("Printer dpi not a valid value.");
            }
            #endregion

            #region IPL Tail
            string sIPLTail = "<STX>R<ETX>  " +
                    "<STX><ESC>E2<ETX> " +
                    "<STX><CAN><ETX>  " +
                    "<STX><ETX> " +
                    "<STX><RS>1<ETX>  " +
                    "<STX><ETB><ETX> ";
            #endregion

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

            PrintHandler ph = new PrintHandler(this.sID);
            if (ph.SendToPrinter(sIPL, quantity) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        protected override void printZPL(object _rack)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}
