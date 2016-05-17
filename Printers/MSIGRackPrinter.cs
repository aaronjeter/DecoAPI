using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    public class MSIGRackPrinter : Printer
    {
        #region Overloaded Constructor

        public MSIGRackPrinter (string ID, string application, Models.AbstractRack rack) : base(ID, application, rack) { }

        #endregion

        #region Print Overrides
        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            Models.sPart thisPart = new Models.Part().GetPartByPartNumber(rack.PartNumber);

			string sPartDesc1 = thisPart.Description1;
			string sPartDesc2 = thisPart.Description2;
			string sColorDesc = thisPart.DescColor;
			string sRev		  = thisPart.RevisionLevel;
			int quantity = 2;
            string sIPL = "";

            if ((thisPart.Description1.PadRight(9, ' ').Substring(0, 9) == "01A6F R L" || thisPart.Description1.PadRight(9, ' ').Substring(0, 9) == "01A6F R R") && (thisPart.PartType == "PAINT" || thisPart.PartType == "FG"))
            {
                sIPL = "<STX>R<ETX><LF>" +
                    "<LF><STX><ESC>k<ETX><LF>" + 
                    "<LF><STX><ESC>C0<ETX><LF>" + 
                    "<LF><STX><ESC>P;<ETX><LF>" + 
                    "<LF><STX>E*;F*;<ETX><LF>" + 
                    "<LF><STX>L39;D0;<ETX><LF>" + 
                    "<LF><STX>L0;o0,608;f0;l1218;w1;D39;<ETX><LF>" + 
                    "<LF><STX>L1;o0,405;f0;l1218;w1;<ETX><LF>" + 
                    "<LF><STX>L2;o508,202;f0;l710;w1;<ETX><LF>" + 
                    "<LF><STX>L3;o506,0;f0;l2;w812;<ETX><LF>" + 
                    "<LF><STX>B4,REV_LEVE;o1136,103;f2;h82;w3;c0,3;i0;r1;d0,9;<ETX><LF>" + 
                    "<LF><STX>B5,RACK_NO ;o1136,304;f2;h82;w3;c0,3;i0;r1;d0,9;<ETX><LF>" + 
                    "<LF><STX>B6,QTY     ;o1136,507;f2;h82;w3;c0,3;i0;r1;d0,5;<ETX><LF>" + 
                    "<LF><STX>B7,PART_NO ;o1136,699;f2;h82;w3;c0,3;i0;r1;d0,9;<ETX><LF>" + 
                    "<LF><STX>H8,Text0029;o493,199;f2;h11;w11;c26;r0;b0;d3,Decostar Part Desc.;<ETX><LF>" + 
                    "<LF><STX>H9,Decostar;o493,155;f2;h17;w17;c26;r0;b0;d0,27;<ETX><LF>" + 
                    "<LF><STX>H10,Decostar;o493,304;f2;h30;w30;c26;r0;b0;d0,11;<ETX><LF>" + 
                    "<LF><STX>H11,Text0026;o493,347;f2;h11;w11;c26;r0;b0;d3,Decostar Part No.;<ETX><LF>" + 
                    "<LF><STX>H12,REV_LEVE;o1126,201;f2;h34;w33;c26;r0;b0;d0,9;<ETX><LF>" + 
                    "<LF><STX>H13,RACK_NO ;o1126,404;f2;h34;w33;c26;r0;b0;d0,9;<ETX><LF>" +     
                    "<LF><STX>H14,QTY     ;o1126,605;f2;h34;w33;c26;r0;b0;d0,5;<ETX><LF>" + 
                    "<LF><STX>H15,PART_NO ;o1126,795;f2;h34;w33;c26;r0;b0;d0,9;<ETX><LF>" + 
                    "<LF><STX>H16,Labe0021;o1209,588;f2;h11;w11;c26;r0;b0;d3,(Q);<ETX><LF>" + 
                    "<LF><STX>H17,Labe0020;o1209,786;f2;h11;w11;c26;r0;b0;d3,(P);<ETX><LF>" + 
                    "<LF><STX>H18,Labe0019;o1209,187;f2;h11;w11;c26;r0;b0;d3,(H);<ETX><LF>" + 
                    "<LF><STX>H19,Labe0018;o1209,388;f2;h11;w11;c26;r0;b0;d3,(S);<ETX><LF>" + 
                    "<LF><STX>H20,Labe0015;o1206,210;f2;h11;w11;c26;r0;b0;d3,Revision Level/Batch;<ETX><LF>" + 
                    "<LF><STX>H21,Labe0014;o1206,412;f2;h11;w11;c26;r0;b0;d3,Serial;<ETX><LF>" + 
                    "<LF><STX>H22,Labe0013;o1204,810;f2;h11;w11;c26;r0;b0;d3,Part No;<ETX><LF>" + 
                    "<LF><STX>H23,Labe0012;o1207,614;f2;h11;w11;c26;r0;b0;d3,Qty;<ETX><LF>" + 
                    "<LF><STX>H24,Part_Des;o496,717;f2;h17;w17;c26;r0;b0;d0,27;<ETX><LF>" + 
                    "<LF><STX>H25,RehauPar;o496,800;f2;h25;w24;c26;r0;b0;d0,11;<ETX><LF>" + 
                    "<LF><STX>H26,Text0009;o331,579;f2;h20;w19;c26;r0;b0;d0,11;<ETX><LF>" + 
                    "<LF><STX>H27,Labe0007;o483,482;f2;h14;w13;c26;r0;b0;d3,Decostar Ind,Carrollton,30117;<ETX><LF>" + 
                    "<LF><STX>H28,Labe0006;o347,524;f2;h11;w11;c26;r0;b0;d3,MFG/Ship Location;<ETX><LF>" + 
                    "<LF><STX>H29,Labe0005;o325,609;f2;h11;w11;c26;r0;b0;d3,MFG/Ship Date;<ETX><LF>" + 
                    "<LF><STX>R<ETX><LF>" + 
                    "<LF><STX>R<ETX><LF>" + 
                    "<LF><STX><ESC>k<ETX><LF>" + 
                    "<LF><STX><ESC>C0<LF>" + 
                    "<LF><SI>L1218<LF>" + 
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
                    "<LF><SI>W812;<SI>S40<LF>" + 
                    "<LF><SI>d0<LF>" + 
                    "<LF><ETX><LF>" + 
                    "<LF><STX><ESC>E*<CAN><ETX><LF>" + 
                    "<LF><STX><ESC>F4<LF>" + 
                    "H" + thisPart.RevisionLevel + "<ETX><LF>" + 
                    "<LF><STX><ESC>F5<LF>" + 
                    "S" + rack.RackID + "<ETX><LF>" + 
                    "<LF><STX><ESC>F6<LF>" + 
                    "Q" + rack.Quantity + "<ETX><LF>" + 
                    "<LF><STX><ESC>F7<LF>" + 
                    "P" + thisPart.CustomerNumber + "<ETX><LF>" + 
                    "<LF><STX><ESC>F9<LF>" + 
                    thisPart.Description2 + "<ETX><LF>" + 
                    "<LF><STX><ESC>F10<LF>" + 
                    rack.PartNumber + "<ETX><LF>" + 
                    "<LF><STX><ESC>F12<LF>" + 
                    thisPart.RevisionLevel + "<ETX><LF>" + 
                    "<LF><STX><ESC>F13<LF>" + 
                    rack.RackID + "<ETX><LF>" + 
                    "<LF><STX><ESC>F14<LF>" + 
                    rack.Quantity + "<ETX><LF>" + 
                    "<LF><STX><ESC>F15<LF>" + 
                    rack.CustomerPartNumber + "<ETX><LF>" + 
                    "<LF><STX><ESC>F24<LF>" + 
                    thisPart.Description2 + "<ETX><LF>" + 
                    "<LF><STX><ESC>F25<LF>" + 
                    rack.CustomerPartNumber + "<ETX><LF>" + 
                    "<LF><STX><ESC>F26<LF>" + 
                    DateTime.Now.ToShortDateString() + "<ETX><LF>" + 
                    "<LF><STX><RS>1<ETX><STX><US>1<ETX><STX><ETB><ETX><LF><LF>";
            }
            else
            {
                #region IPL Header
                string sIPLHead = "<STX><ESC>C<ETX>"
                                + "<STX><ESC>P<ETX>"
                                + "<STX>E1;F1<ETX>";
                if (application == "wip_label.aspx")
                    sIPLHead += "<STX><SI>g1,567<ETX>";
                else
                    sIPLHead += "<STX><SI>g0,420<ETX>";
                #endregion
                #region IPL Body
                string sIPLBody = "";
                if (DPI == "200" || DPI == "400")
                {
                    sIPLBody = "<STX>B1;o101,176;f0;c6,0;i0;w4;h80;d3," + rack.PartNumber + "<ETX> " +
                                    "<STX>H3;o74,288;f0;c26;w1;h1;k12;d3,Part Description;<ETX> " +
                                    "<STX>B4;o256,600;f3;c6,0;i0;w4;h80;d3," + rack.Quantity + "<ETX> " +
                                    "<STX>H5;o74,544;f0;c26;w1;h1;k12;d3,Quantity;<ETX> " +
                                    "<STX>B6;o300,623;f0;c6,0;i0;w4;h130;d3,|RCK|" + rack.RackID + "<ETX> " +
                                    "<STX>H7;o780,544;f0;c26;w1;h1;k12;d3,Rack #;<ETX> " +
                                    "<STX>H8;o74,336;f0;c26;w1;h1;k27;d3," + sPartDesc1 + "<ETX> " +
                                    "<STX>H9;o290,510;f0;c26;w1;h1;k36;d3," + rack.RackID + "<ETX> " +
                                    "<STX>H10;o746,64;f0;c26;w1;h1;k27;d3,Decostar;<ETX> " +
                                    "<STX>H11;o74,416;f0;c26;w1;h1;k27;d3," + sPartDesc2 + "<ETX> " +
                                    "<STX>L12;o42,272;;l1120;f0;w1;<ETX> " +
                                    "<STX>L13;o42,523;;l1120;f0;w1;<ETX> " +
                                    "<STX>H14;o35,1;f0;c26;w200;h200;k56;d3," + rack.PartNumber + "<ETX> " +
                                    "<STX>H15;o30,612;f0;c26;w1;h1;k36;d3," + rack.Quantity + "<ETX> " +
                                    "<STX>H16;o736,160;f0;c26;w1;h1;k12;d3," + DateTime.Now.ToString() + "<ETX> " +
                                    "<STX>L17;o270,528;;l270;f3;w1;<ETX> " +
                                    "<STX>L18;o650,16;;l256;f3;w1;<ETX> " +
                                    "<STX>H19;o554,288;f0;c26;w1;h1;k18;d3," + sColorDesc + "<ETX> " +
                                    "<STX>H22;o26,384;f3;c26;w1;h1;k8;d3," + application + "<ETX> " +
                                    "<STX>H23;o975,544;f0;c26;w1;h1;k12;d3,Revision;<ETX> " +
                                    "<STX>H24;o975,560;f0;c26;w1;h1;k36;d3," + sRev + ";<ETX> " +
                                    "<STX>L25;o950,528;;l256;f3;w1;<ETX> ";
                    if ((thisPart.Description1.Substring(3, 2) == "HR"
                        && thisPart.Description1.Substring(6, 1) == "T"))
                        sIPLBody += "<STX>H26;o300,760;f0;c26;w1;h1;k12;d3,CUSTOMER#" + rack.CustomerPartNumber + ";<ETX>";
                }
                else
                {
                    throw new Exception("Printer dpi not a valid value.");
                }
                #endregion
                #region IPL Tail
                string sIPLTail = "<STX>R;<ETX><STX><CAN><ETX><STX><ESC>E1<ETX><STX><CAN><ETX><STX><ETB><ETX>";

                sIPL = sIPLHead + sIPLBody + sIPLTail;
                #endregion
            }

            PrintHandler ph = new PrintHandler(this.sID);
            if (ph.SendToPrinter(sIPL, quantity) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        protected override void printZPL(object rack)
        {            
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}
