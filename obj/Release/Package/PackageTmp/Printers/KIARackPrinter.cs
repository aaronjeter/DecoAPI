using System;
using System.Globalization;     
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DecoAPI.Printers
{
    public class KIARackPrinter : Printer
    {
        #region Overloaded Constructor

        public KIARackPrinter (string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }

        #endregion

        #region Print Overrides
        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            if (rack.Type != Models.RackType.KIARack)
            {
                throw new Exception("Unassigned Rack");
            }
                        
            Models.sPart thisPart = new Models.Part().GetPartByPartNumber(rack.PartNumber);

			string partDesc1 = thisPart.Description1;
			string partDesc2 = thisPart.Description2;
			string colorDesc = thisPart.DescColor;
			string revision	 = thisPart.RevisionLevel;
            string partType  = thisPart.PartType;
			int quantity = 3;

            string PGN = string.Empty;
            string PAC = string.Empty;
            string PONum = string.Empty;
            string Dock = string.Empty;
            string MatCode = string.Empty;
            
            Models.Database database = new Models.Database();

            if (partType != "SVFG" && partType != "SVPAINT")
            {
                #region PGN/PAC
                // Search Based on decostar part number
                string query = " select    * "
                              + " from      RackRecords.dbo.KIA_PGN_PAC_xref "
                              + " where     deco_partnum = '" + rack.PartNumber + "' ";
                
                SqlDataReader reader = database.RunCommandReturnReader(query);
                reader.Read();
                PGN = reader["kia_pgn"].ToString();
                PAC = reader["kia_pac"].ToString();
                database.CloseConnection();
                #endregion
            }
            if (thisPart.Description1.Substring(2, 2) == "QF" || thisPart.Description1.Substring(2, 2) == "UM" || thisPart.Description1.Substring(2, 2) == "LF")
            {
                #region PO Number
                // Search based on Decostar Part Number
                string query = " select  * "
                                + " from    RackRecords.dbo.PurchaseOrders "
                                + " where   Part_Number = '" + rack.PartNumber + "' ";

                // Open DB Connection
                SqlDataReader reader = database.RunCommandReturnReader(query);
                reader.Read();
                PONum = reader["Purchase_Order"].ToString();
                Dock = reader["Plant_Dock"].ToString();
                MatCode = reader["Material_Code"].ToString();
                database.CloseConnection();
                #endregion
            }

            string sIPLHead = string.Empty;
            string sIPLBody = string.Empty;
            string sIPLTail = string.Empty;

            // Create IPL string for new format KIA labels
            if (thisPart.Description1.Substring(2, 2) == "QF" || thisPart.Description1.Substring(2, 2) == "UM" || thisPart.Description1.Substring(2, 2) == "JF")
            {
                #region IPL Header
                sIPLHead = "<STX>R<ETX>" 
                                + "<STX><ESC>k<ETX>" 
                                + "<STX><ESC>C0<ETX>" 
                                + "<STX><ESC>P;<ETX>"
                                + "<STX>E*;F*;<ETX>;";
                #endregion
                #region IPL Body
                if (DPI == "200" || DPI == "400")
                {
                    sIPLBody = "<STX>L39;D0;<ETX>" 
                            + "<STX>L0;o5,606;f0;l1211;w2;D39;<ETX>" 
                            + "<STX>L1;o5,492;f0;l1210;w3;<ETX>"
                            + "<STX>L2;o204,265;f0;l1011;w2;<ETX>"
                            + "<STX>L3;o913,4;f0;l2;w804;<ETX>"
                            + "<STX>L4;o203,3;f0;l2;w805;<ETX>"
                            + "<STX>L5;o7,684;f0;l197;w2;<ETX>"
                            + "<STX>L6;o204,368;f0;l711;w2;<ETX>"
                            + "<STX>L7;o583,267;f0;l2;w101;<ETX>"
                            + "<STX>H8,Supplier;o306,616;f2;h21;w20;c26;r0;b0;d3,ALQE;<ETX>" 
                            + "<STX>H9,Labe0026;o910,486;f2;h11;w11;c26;r0;b0;d3,Decostar Part #;<ETX>" 
                            + "<STX>H10,Labe0014;o906,368;f2;h11;w11;c26;r0;b0;d3,Production Date;<ETX>" 
                            + "<STX>H11,Labe0021;o7,462;f1;h17;w17;c26;r0;b0;d3,Decostar Rack #;<ETX>" 
                            + "<STX>H12,Labe0018;o859,583;f2;h28;w27;c26;r0;b0;d3,Decostar Industries;<ETX>" 
                            + "<STX>H13,Labe0016;o721,263;f2;h11;w11;c26;r0;b0;d3,Container Handling Unit;<ETX>" 
                            + "<STX>H14,Labe0015;o503,368;f2;h11;w11;c26;r0;b0;d3,Production Time;<ETX>" 
                            + "<STX>H15,Labe0013;o200,603;f2;h11;w11;c26;r0;b0;d3,Dock No.;<ETX>" 
                            + "<STX>H16,Labe0012;o200,691;f2;h11;w11;c26;r0;b0;d3,Packing Type;<ETX>" 
                            + "<STX>H17,Labe0011;o907,605;f2;h11;w11;c26;r0;b0;d3,Supplier;<ETX>" 
                            + "<STX>H18,Labe0010;o200,801;f2;h11;w11;c26;r0;b0;d3,Quantity;<ETX>" 
                            + "<STX>H19,Labe0009;o907,803;f2;h11;w11;c26;r0;b0;d3,Part Number / Description;<ETX>" 
                            + "<STX>H20,Labe0008;o1201,265;f2;h11;w11;c26;r0;b0;d3,Part Assembly Code;<ETX>" 
                            + "<STX>H21,Labe0007;o1201,486;f2;h11;w11;c26;r0;b0;d3,P.G.No/Description;<ETX>" 
                            + "<STX>H22,Labe0006;o1202,603;f2;h11;w11;c26;r0;b0;d3,PO type (Lot Size);<ETX>" 
                            + "<STX>H23,Labe0005;o1202,765;f2;h11;w11;c26;r0;b0;d3,Material Group;<ETX>" 
                            + "<STX>B24,DecoRack;o45,478;f1;h102;w2;c6;i0;d0,12;<ETX>" 
                            + "<STX>B25,Supplbcd;o736,156;f2;h4;w2;i0;c12,6,5,0;d0,503;<ETX>" 
                            + "<STX>H26,Qty     ;o159,797;f2;h42;w42;c26;r0;b0;d0,5;<ETX>" 
                            + "<STX>H27,Supplier;o900,783;f2;h55;w43;c26;r0;b0;d0,18;<ETX>" 
                            + "<STX>H28,PAC     ;o1218,236;f2;h56;w55;c26;r0;b0;d0,7;<ETX>" 
                            + "<STX>H29,PGN     ;o1218,462;f2;h56;w55;c26;r0;b0;d0,8;<ETX>" 
                            + "<STX>H30,DecoRack;o136,440;f1;h30;w30;c26;r0;b0;d0,10;<ETX>" 
                            + "<STX>H31,Part_Des;o905,655;f2;h20;w20;c26;r0;b0;d0,27;<ETX>" 
                            + "<STX>H32,Material;o1201,728;f2;h30;w30;c26;r0;b0;d0,11;<ETX>" 
                            + "<STX>H33,DecoPart;o898,464;f2;h30;w30;c26;r0;b0;d0,10;<ETX>" 
                            + "<STX>H34,ProdDate;o906,350;f2;h25;w25;c26;r0;b0;d0,10;<ETX>" 
                            + "<STX>H35,ProdTime;o520,350;f2;h30;w30;c26;r0;b0;d0,10;<ETX>" 
                            + "<STX>H36,POType  ;o1201,580;f2;h30;w30;c26;r0;b0;d0,4;<ETX>" 
                            + "<STX>H37,PackType;o190,668;f2;h25;w25;c26;r0;b0;d0,5;<ETX>" 
                            + "<STX>H38,DockNo  ;o159,587;f2;h42;w42;c26;r0;b0;d0,5;<ETX>" 
                            + "<STX>H39,CustomPO;o721,238;f2;h25;w25;c26;r0;b0;d0,25;<ETX>";
                }
                else
                {
                    throw new Exception("Printer dpi not a valid value.");
                }   
                #endregion
                #region IPL Tail
                sIPLTail = "<STX>R<ETX>" 
                            + "<STX>R<ETX>" 
                            + "<STX><ESC>k<ETX>" 
                            + "<STX><ESC>C0" 
                            + "<SI>L1218" 
                            + "<SI>g0,424" 
                            //+ "<SI>g1,567"
                            + "<SI>T1" 
                            + "<SI>R1<SI>r0" 
                            + "<SI>D0" 
                            + "<SI>c0<SI>t0" 
                            + "<SI>l0" 
                            + "<SI>F2" 
                            + "<SI>f0"
                            + "<SI>L1000" 
                            + "<SI>I5" 
                            + "<SI>i0" 
                            + "<SI>W812;<SI>S40" 
                            + "<SI>d0" 
                            + "<ETX><STX><ESC>E*<CAN><ETX>" 
                            + "<STX><ESC>F24<LF>" 
                            + "|RCK|" + rack.RackID + "<ETX>"
                            + "<STX><ESC>F25<LF>"
                            + "[)>*06:3SALQE" + rack.RackID + ":P" + rack.CustomerPartNumber + ":PG" + PGN + ":PA" + PAC + ":7Q" + rack.Quantity + "EA:VALQE*EOT<ETX>" 
                            + "<STX><ESC>F26<LF>" 
                            + rack.Quantity + "<ETX>" 
                            + "<STX><ESC>F27<LF>" 
                            + rack.CustomerPartNumber + "<ETX>" 
                            + "<STX><ESC>F28<LF>" 
                            + PAC + "<ETX>" 
                            + "<STX><ESC>F29<LF>" 
                            + PGN + "<ETX>" 
                            + "<STX><ESC>F30<LF>" 
                            + rack.RackID + "<ETX>" 
                            + "<STX><ESC>F31<LF>" 
                            + partDesc2 + "<ETX>" 
                            + "<STX><ESC>F32<LF>" 
                            + MatCode + "<ETX>"
                            + "<STX><ESC>F33<LF>" 
                            + rack.PartNumber + "<ETX>" 
                            + "<STX><ESC>F34<LF>" 
                            + DateTime.Now.ToString("MM/dd/yyyy") + "<ETX>" 
                            + "<STX><ESC>F35<LF>"
                            + DateTime.Now.ToString("hh:mm tt") + "<ETX>"
                            + "<STX><ESC>F36<LF>" 
                            + "DJIT<ETX>" 
                            + "<STX><ESC>F37<LF>" 
                            + "Rack<ETX>" 
                            + "<STX><ESC>F38<LF>" 
                            + Dock + "<ETX>" 
                            + "<STX><ESC>F39<LF>" 
                            + PONum + "<ETX>" 
                            + "<STX><RS>1<ETX><STX><US>1<ETX><STX><ETB><ETX>" 
                            + "<STX><FF><ETX>";
            #endregion
            }

            // Create IPL string for new LFa labels
            else if (thisPart.Description1.Substring(2, 2) == "LF")
            {
                #region IPL Header
                sIPLHead = "<STX>R<ETX>"
                                + "<STX><ESC>k<ETX>"
                                + "<STX><ESC>C0<ETX>"
                                + "<STX><ESC>P;<ETX>"
                                + "<STX>E*;F*;<ETX>;";
                #endregion
                #region IPL Body
                if (DPI == "200" || DPI == "400")
                {
                    sIPLBody = "<STX>L39;D0;<ETX>" 
                        + "<STX>L0;o5,606;f0;l1211;w2;D39;<ETX>" 
                        + "<STX>L1;o205,492;f0;l711;w3;<ETX>" 
                        + "<STX>L2;o204,265;f0;l1011;w2;<ETX>" 
                        + "<STX>L3;o913,4;f0;l2;w604;<ETX>" 
                        + "<STX>L4;o203,3;f0;l2;w605;<ETX>" 
                        + "<STX>L5;o583,368;f0;l626;w2;<ETX>" 
                        + "<STX>L6;o583,267;f0;l2;w225;<ETX>" 
                        + "<STX>H7,Supplier;o306,616;f2;h21;w20;c26;r0;b0;d3,ALQE;<ETX>" 
                        + "<STX>H8,Labe0026;o910,486;f2;h11;w11;c26;r0;b0;d3,Decostar Part #;<ETX>" 
                        + "<STX>H9,Labe0014;o906,368;f2;h11;w11;c26;r0;b0;d3,Production Date;<ETX>" 
                        + "<STX>H10,Labe0021;o7,462;f1;h17;w17;c26;r0;b0;d3,Decostar Rack #;<ETX>" 
                        + "<STX>H11,Labe0018;o859,583;f2;h28;w27;c26;r0;b0;d3,Decostar Industries;<ETX>" 
                        + "<STX>H12,Labe0016;o721,263;f2;h11;w11;c26;r0;b0;d3,Container Handling Unit;<ETX>" 
                        + "<STX>H13,Labe0015;o1201,368;f2;h11;w11;c26;r0;b0;d3,Production Time;<ETX>" 
                        + "<STX>H14,LabelQty;o503,486;f2;h11;w11;c26;r0;b0;d3,Quantity;<ETX>" 
                        + "<STX>H15,Labe0011;o907,605;f2;h11;w11;c26;r0;b0;d3,Supplier;<ETX>" 
                        + "<STX>H16,Labe0009;o907,803;f2;h11;w11;c26;r0;b0;d3,Part Number / Description;<ETX>" 
                        + "<STX>H17,Labe0008;o1201,265;f2;h11;w11;c26;r0;b0;d3,Part Assembly Code;<ETX>" 
                        + "<STX>H18,Labe0007;o1201,603;f2;h11;w11;c26;r0;b0;d3,P.G.No/Description;<ETX>" 
                        + "<STX>B19,DecoRack;o45,538;f1;h102;w3;c6;i0;d0,12;<ETX>" 
                        + "<STX>B20,Supplbcd;o736,216;f2;h4;w2;i0;c12,6,5,0;d0,503;<ETX>" 
                        + "<STX>H21,Qty     ;o520,497;f2;h100;w100;c26;r0;b0;d0,5;<ETX>" 
                        + "<STX>H22,Supplier;o1200,835;f2;h95;w70;c26;r0;b0;d0,18;<ETX>" 
                        + "<STX>H23,PAC     ;o1218,236;f2;h56;w55;c26;r0;b0;d0,7;<ETX>" 
                        + "<STX>H24,PGN     ;o1200,600;f2;h100;w100;c26;r0;b0;d0,8;<ETX>" 
                        + "<STX>H25,DecoRack;o136,440;f1;h30;w30;c26;r0;b0;d0,10;<ETX>" 
                        + "<STX>H26,Part_Des;o905,655;f2;h20;w20;c26;r0;b0;d0,27;<ETX>" 
                        + "<STX>H27,DecoPart;o898,464;f2;h30;w30;c26;r0;b0;d0,10;<ETX>" 
                        + "<STX>H28,ProdDate;o906,350;f2;h25;w25;c26;r0;b0;d0,10;<ETX>" 
                        + "<STX>H29,ProdTime;o1218,350;f2;h30;w30;c26;r0;b0;d0,10;<ETX>";
                }
                else
                {
                    throw new Exception("Printer dpi not a valid value.");
                }
                #endregion
                #region IPL Tail
                sIPLTail = "<STX>R<ETX>" 
                        + "<STX>R<ETX>" 
                        + "<STX><ESC>k<ETX>" 
                        + "<STX><ESC>C0" 
                        + "<SI>L1218" 
                        + "<SI>g0,424" 
                        //+ "<SI>g1,567" 
                        + "<SI>T1" 
                        + "<SI>R1<SI>r0" 
                        + "<SI>D0" 
                        + "<SI>c0<SI>t0" 
                        + "<SI>l0" 
                        + "<SI>F2" 
                        + "<SI>f0" 
                        + "<SI>L1000" 
                        + "<SI>I5" 
                        + "<SI>i0" 
                        + "<SI>W812;<SI>S40" 
                        + "<SI>d0" 
                        + "<ETX>" 
                        + "<STX><ESC>E*<CAN><ETX>" 
                        + "<STX><ESC>F19<LF>" 
                        + "|RCK|" + rack.RackID + "<ETX>" 
                        + "<STX><ESC>F20<LF>" 
                        + "[)>*06:3SALQE" + rack.RackID + ":P" + rack.CustomerPartNumber + ":PG" + PGN + ":PA" + PAC + ":7Q" + rack.Quantity + "EA:VALQE*EOT<ETX>" 
                        + "<STX><ESC>F21<LF>" 
                        + rack.Quantity + "<ETX>" 
                        + "<STX><ESC>F22<LF>" 
                        + rack.CustomerPartNumber + "<ETX>" 
                        + "<STX><ESC>F23<LF>" 
                        + PAC + "<ETX>" 
                        + "<STX><ESC>F24<LF>" 
                        + PGN + "<ETX>" 
                        + "<STX><ESC>F25<LF>" 
                        + rack.RackID + "<ETX>" 
                        + "<STX><ESC>F26<LF>" 
                        + thisPart.Description1 + "<ETX>" 
                        + "<STX><ESC>F27<LF>"
                        + rack.PartNumber + "<ETX>" 
                        + "<STX><ESC>F28<LF>"
                        + DateTime.Now.ToString("MM/dd/yyyy") + "<ETX>" 
                        + "<STX><ESC>F29<LF>"
                        + DateTime.Now.ToString("hh:mm tt") + "<ETX>" 
                        + "<STX><RS>1<ETX><STX><US>2<ETX><STX><ETB><ETX>" 
                        + "<STX><FF><ETX>";
                #endregion
            }

            // Create IPL string for old format KIA labels
            else
            {
                #region IPL Header
                sIPLHead = "<STX><ESC>C<ETX>"
                            + "<STX><ESC>P<ETX>"
                            + "<STX>E1;F1<ETX>"
                            + "<STX><SI>g0,420<ETX>";
            #endregion
                #region IPL Body            
            if (DPI == "200" || DPI == "400")
            {
                sIPLBody = "<STX>B1;o80,176;f0;c6,0;i0;w4;h80;d3," + rack.PartNumber + "<ETX> " +
                            //"<STX>H2;o74,32;f0;c26;w1;h1;k12;d3,Part #;<ETX> " +
                            "<STX>H3;o74,288;f0;c26;w1;h1;k12;d3,Part Description;<ETX> " +
                            "<STX>B4;o256,600;f3;c6,0;i0;w4;h80;d3," + rack.Quantity + "<ETX> " +
                            "<STX>H5;o74,544;f0;c26;w1;h1;k12;d3,Quantity;<ETX> " +
                            "<STX>B6;o300,623;f0;c6,0;i0;w4;h130;d3,|RCK|" + rack.RackID + "<ETX> " +
                            "<STX>H7;o780,544;f0;c26;w1;h1;k12;d3,Rack #;<ETX> " +
                            "<STX>H8;o74,336;f0;c26;w1;h1;k27;d3," + partDesc1 + "<ETX> " +
                            "<STX>H9;o290,510;f0;c26;w1;h1;k36;d3," + rack.RackID + "<ETX> " +
                            "<STX>H10;o746,64;f0;c26;w1;h1;k27;d3,Decostar;<ETX> " +
                            "<STX>H11;o74,416;f0;c26;w1;h1;k27;d3," + partDesc2 + "<ETX> " +
                            "<STX>L12;o42,272;;l1120;f0;w1;<ETX> " +
                            "<STX>L13;o42,523;;l1120;f0;w1;<ETX> " +
                            "<STX>H14;o35,1;f0;c26;w200;h200;k56;d3," + rack.PartNumber + "<ETX> " +
                            "<STX>H15;o30,612;f0;c26;w1;h1;k42;d3," + rack.Quantity + "<ETX> " +
                            "<STX>H16;o736,160;f0;c26;w1;h1;k12;d3," + DateTime.Now.ToString() + "<ETX> " +
                            "<STX>L17;o270,528;;l270;f3;w1;<ETX> " +
                            "<STX>L18;o650,16;;l256;f3;w1;<ETX> " +
                            "<STX>H19;o554,288;f0;c26;w1;h1;k18;d3," + colorDesc + "<ETX> " +
                            "<STX>H22;o26,384;f3;c26;w1;h1;k8;d3," + application + "<ETX> " +
                            "<STX>H23;o960,544;f0;c26;w1;h1;k12;d3,Revision;<ETX> " +
                            "<STX>H24;o960,560;f0;c26;w1;h1;k36;d3," + revision + ";<ETX> " +
                            "<STX>L25;o930,528;;l256;f3;w1;<ETX> ";
                            if (thisPart.Description1.Substring(2, 2) == "XM" || thisPart.Description1.Substring(2, 2) == "LF" || thisPart.Description1.Substring(2, 2) == "QF" || thisPart.Description1.Substring(2, 2) == "AN" || thisPart.Description1.Substring(2, 2) == "UM")
                            {
                                sIPLBody += "<STX>H26;o660,225;f0;c26;w1;h1;k12;d3,(P) CUSTOMER# " + rack.CustomerPartNumber + ";<ETX> " +
                                    "<STX>H10;o746,20;f0;c26;w1;h1;k27;d3,Decostar;<ETX> " +
                                    "<STX>H16;o736,110;f0;c26;w1;h1;k12;d3," + DateTime.Now.ToString() + "<ETX> " +
                                    "<STX>B27;o730,155;f0;c6,0;i0;w2;h60;d3,P" + rack.CustomerPartNumber + "<ETX> " + 
                                    "<STX>L30;o940,680;;l220;f0;w1;<ETX> ";
                            }
                            if (partType != "SVFG" && partType != "SVPAINT" && thisPart.Description1.Substring(2, 2) != "LF") // oRack.PartNumber != "9710007" && oRack.PartNumber != "9710008"
                            {
                                sIPLBody += "<STX>H28;o960,690;f0;c26;w1;h1;k12;d3,PGN / PAC;<ETX> " +
                                    "<STX>H29;o950,740;f0;c26;w1;h1;k18;d3," + PGN + " " + PAC + ";<ETX> ";
                            }
            }
            else
            {
                throw new Exception("Printer dpi not a valid value.");
            }
            #endregion
                #region IPL Tail
                sIPLTail = "<STX>R;<ETX><STX><CAN><ETX><STX><ESC>E1<ETX><STX><CAN><ETX><STX><ETB><ETX>";
                #endregion
            }

            string sIPL = sIPLHead + sIPLBody + sIPLTail;

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
