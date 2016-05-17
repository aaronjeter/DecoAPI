using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    class BMWE7RackPrinter : Printer
    {
        #region Overloaded Constructor
        public BMWE7RackPrinter(string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }
        #endregion

        #region Print Overrides

        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            Models.sPart part = new Models.Part().GetPartByPartNumber(rack.PartNumber);

            string sPtDesc2 = part.Description2;

            if (sPtDesc2.Length > 24)
            {
                sPtDesc2 = sPtDesc2.Substring(0, 24);
            }

            string sAddress1 = string.Empty;
            string sAddress2 = string.Empty;
            string sAddress3 = string.Empty;

            if (rack.ShipLocation == "ARD Logistics")
            {
                sAddress1 = "ARD LOGISTICS";
                sAddress2 = "3001 INTERSTATE CIRCLE";
                sAddress3 = "COTTONDATE, AL 35453";
            }
            else if (rack.ShipLocation == "Magna Mirrors")
            {
                sAddress1 = "MAGNA MIRRORS";
                sAddress2 = "320 JOHN MARTIN RD";
                sAddress3 = "SPARTANBURG, SC 29303";
            }
            else if (part.Description1.Substring(0, 5) == "02F25")
            {
                sAddress1 = "PLASTIC OMNIUM AUTO EXTERIOR";
                sAddress2 = "5100 OLD PEARMAN DAIRY RD";
                sAddress3 = "ANDERSON, SC 29265";
            }
            else
            {
                sAddress1 = "PLASTIC OMNIUM AUTO EXTERIOR";
                sAddress2 = "50 TYGER RIVER ROAD";
                sAddress3 = "DUNCAN, UNITED STATES";
            }

            #region IPL Head
            string sIPLHead = "<STX><ESC>C<ETX>"
                            + "<STX><ESC>P<ETX>"
                            + "<STX>E1;F1<ETX>"
                            + "<STX><SI>g0,420<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";
            
            if (true)
            {
                sIPLBody =          // PART NO. FIELD (1)
                                    "<STX>H11;o0,0;f0;c26;w1;h1;k10;d3,PART NUMBER (P);<ETX>" +
                                    "<STX>H12;o25,8;f0;c26;w1;h1;k32;d3," + part.CustomerNumber + ";<ETX>" +
                                    "<STX>B13;o25,103;f0;c0,0;i0;w3;h80;d3,P" + part.CustomerNumber + ";<ETX>" +

                                    // PART NO. HORIZONTAL LINE
                                   "<STX>L14;o0,198;f0;l609;w2;<ETX>" +

                                    // QUANTITY/SUPPLIER FIELD (2)
                                    "<STX>H21;o0,195;f0;c26;w1;h1;k10;d3,QUANTITY (Q);<ETX>" +
                                    "<STX>H22;o25,200;f0;c26;w1;h1;k32;d3," + rack.Quantity + ";<ETX>" +
                                    "<STX>B23;o25,295;f0;c0,0;i0;w3;h80;d3,Q" + rack.Quantity + ";<ETX>" +

                                    /******************************************
                                    "<STX>H24;o421,195;f0;c26;w1;h1;k10;d3,SUPPLIER;<ETX>" +
                                    "<STX>H25;o421,250;f0;c26;w1;h1;k14;d3,?????;<ETX>" +
                                    ******************************************/

                                    // QUANTITY/SUPPLIER HORIZONTAL LINE
                                    "<STX>L26;o0,390;f0;l609;w10;<ETX>" +

                                    /******************************************
                                    // QUANTITY/SUPLLIER VERTICAL LINE
                                    "<STX>L27;o406,390;f1;l193;w2;<ETX>" +
                                    ******************************************/

                                    /******************************************
                                    // PO FIELD (3)
                                    "<STX>H31;o0,390;f0;c26;w1;h1;k10;d3,PO # (K);<ETX>" +
                                    "<STX>H32;o25,400;f0;c26;w1;h1;k32;d3,??????;<ETX>" +
                                    "<STX>B33;o25,495;f0;c0,0;i0;w5;h160;d3,K??????;<ETX>" +
                                    ******************************************/

                                    // PO HORIZONTAL LINE
                                    "<STX>L34;o0,590;f0;l609;w2;<ETX>" +

                                    // SERIAL NUMBER FIELD (4)
                                    "<STX>H41;o0,590;f0;c26;w1;h1;k10;d3,PKG ID/SERIAL NUMBER (S);<ETX>" +
                                    "<STX>H42;o25,600;f0;c26;w1;h1;k32;d3," + rack.RackID + ";<ETX>" +
                                    "<STX>B43;o25,695;f0;c0,0;i0;w3;h80;d3,S" + rack.RackID + ";<ETX>" +

                                    // SERIAL NUMBER HORIZONTAL LINE
                                    "<STX>L44;o0,795;f0;l609;w10;<ETX>" +

                                    // DESCRIPTION FIELD (5)
                                    "<STX>H51;o624,0;f0;c26;w1;h1;k10;d3,DESCRIPTION;<ETX>" +

                                    "<STX>H52;o635,25;f0;c26;w1;h1;k14;d3," + sPtDesc2 + ";<ETX>" +
                                    "<STX>H53;o635,60;f0;c26;w1;h1;k14;d3,;<ETX>" +
                                    "<STX>H54;o635,95;f0;c26;w1;h1;k14;d3,;<ETX>" +

                                    // DESCRIPTION HORIZONTAL LINE
                                    "<STX>L55;o609,198;f0;l609;w2;<ETX>" +

                                    // EC/FIFO/LOC FIELDS (6)
                                    "<STX>H61;o624,198;f0;c26;w1;h1;k10;d3,EC LEVEL;<ETX>" +
                                    "<STX>H62;o624,250;f0;c26;w1;h1;k15;d3," + part.RevisionLevel + ";<ETX>" +

                                    "<STX>H63;o835,198;f0;c26;w1;h1;k10;d3,FIFO DATE;<ETX>" +
                                    "<STX>H64;o835,250;f0;c26;w1;h1;k15;d3," + DateTime.Now.ToShortDateString() + ";<ETX>" +
                                    "<STX>H65;o875,300;f0;c26;w1;h1;k15;d3," + DateTime.Now.ToShortTimeString() + ";<ETX>" +

                                    "<STX>H66;o1015,185;f0;c26;w1;h1;k10;d3,;<ETX>" +

                                    // EC/FIFO/LOC HORIZONTAL LINE
                                    "<STX>L67;o609,390;f0;l605;w2;<ETX>" +

                                    // EC/LOT/FIFO VERTICAL LINES
                                    "<STX>L68;o820,390;f1;l193;w2;<ETX>" +
                                    //"<STX>L69;o1000,390;f1;l205;w2;<ETX>" +

                                    // FROM FIELD (7)
                                    "<STX>H71;o624,395;f0;c26;w1;h1;k10;d3,FROM:;<ETX>" +

                                    "<STX>H72;o634,425;f0;c26;w1;h1;k10;d3,DECOSTAR;<ETX>" +
                                    "<STX>H73;o634,450;f0;c26;w1;h1;k10;d3,1 DECOMA DRIVE;<ETX>" +
                                    "<STX>H74;o634,475;f0;c26;w1;h1;k10;d3,CARROLLTON, GA 30117;<ETX>" +

                                    // FROM HORIZONTAL LINE
                                    "<STX>L75;o609,525;f0;l609;w2;<ETX>" +

                                    // TO FIELD (8)
                                    "<STX>H81;o624,530;f0;c26;w1;h1;k10;d3,TO:;<ETX>" +

                                    "<STX>H82;o634,585;f0;c26;w1;h1;k12;d3," + sAddress1 + ";<ETX>" +
                                    "<STX>H83;o634,635;f0;c26;w1;h1;k12;d3," + sAddress2 + ";<ETX>" +
                                    "<STX>H84;o634,685;f0;c26;w1;h1;k12;d3," + sAddress3 + ";<ETX>" +

                                    // TO HORIZONTAL LINE
                                    "<STX>L85;o609,750;f0;l609;w2;<ETX>" +

                                    // DECOSTAR FIELD (9)
                                    "<STX>H91;o624,750;f0;c26;w1;h1;k10;d3,DECOSTAR PART #: " + part.Number + ";<ETX>" +

                                    // VERTICAL LINE (0)
                                    "<STX>L01;o609,795;f1;l788;w2;<ETX>" +

                                    // END
                                    "";
            }
            #endregion
            #region IPL Tail
            string sIPLTail = "<STX>R;<ETX><STX><CAN><ETX><STX><ESC>E1<ETX><STX><CAN><ETX><STX><ETB><ETX>";
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
