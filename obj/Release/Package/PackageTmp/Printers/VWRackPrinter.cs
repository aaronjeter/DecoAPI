using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    public class VWRackPrinter : Printer
    {
        #region Overloaded Constructor
        public VWRackPrinter (string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }

        //[Obsolete ("No longer used. Address is pulled from Rack.ShipLocation instead.")]
        //public VWRackPrinter(string sID, string application, Rack.Rack oRack, Address stAddress) 
        //    : base(sID, application, oRack) 

        //{
        //    this.stAddress = stAddress;
        //}

        #endregion

        #region Print Overrides
        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            #region Part Info
            Models.sPart part = new Models.Part().GetPartByPartNumber(rack.PartNumber);
            
            string sPartDesc1 = part.Description1;
            string sPartDesc2 = part.Description2;
            string sColorDesc = part.DescColor;
            string sRev = part.RevisionLevel;
            string sCustPart = part.CustomerNumber;
            string sPurchaseOrder = string.Empty;
            string sPlantDock = string.Empty;
            string sMaterialCode = string.Empty;
            #endregion

            #region Fatty Bits
            // Calculate Weight (kg)
            // The weight of the part INCLUDES the weight of the pallet/rack divided by the maximum
            //   number of parts on said pallet/rack. Thus, the weight calculation is only right if
            //   the rack is filled to capacity. Keep this in mind when calculating weight.
            double iIndividualLB = Double.Parse(part.PartWeight); // PartWeight is in POUNDS
            double iIndividualKG = iIndividualLB / 2.2; // Convert to KILOGRAMS
            double iQuanity = rack.Quantity;
            //double iPalletKG = 21.77;
            //double iGrossWeight = Math.Round((iIndividualKG * iQuanity) + iPalletKG, 0);
            double iGrossWeight = Math.Round((iIndividualKG * iQuanity),0);

            // Build 2D Data
            string sBarcodeData = string.Empty;
            // Message Header
            sBarcodeData += "[)><RS>";
            // Format Header
            sBarcodeData += "06<GS>";
            // Part #
            sBarcodeData += "P" + part.CustomerNumber + "<GS>";
            // Quantity
            sBarcodeData += "Q" + rack.Quantity + "<GS>";
            // Serial #
            sBarcodeData += "1JUN129357955" + rack.RackID + "<GS>";
            // Material Handling Code
            sBarcodeData += "20L" + sMaterialCode.Trim() + "<GS>";
            // Plant/Delivery Dock
            sBarcodeData += "21L" + sPlantDock.Trim() + "<GS>";
            // PO Number
            sBarcodeData += "K" + sPurchaseOrder.Trim() + "<GS>";
            // Container Type
            sBarcodeData += "BPCS71<GS>";
            // Gross Weight
            sBarcodeData += "7Q" + iGrossWeight + "GT";
            // Message Trailer
            sBarcodeData += "<RS><SUB><EOT>";

            // Build Date String
            string sDate = string.Empty;
            sDate += DateTime.Today.Day;
            sDate += DateTime.Now.ToString("MMMM").Substring(0, 3).ToUpper();
            sDate += DateTime.Today.Year;

            // Set Location
            string sLocation1 = string.Empty;
            string sLocation2 = string.Empty;
            string sLocation3 = string.Empty;
            string sLocation4 = string.Empty;

            sLocation1 = address.Line1;
            sLocation2 = address.Line2;
            sLocation3 = address.Line3;
            sLocation4 = address.Line4;
            #endregion

            #region IPL Head
            string sIPLHead = "<STX><ESC>C<ETX>"
                            + "<STX><ESC>P<ETX>"
                            + "<STX>E1;F1<ETX>"
                            + "<STX><SI>g0,420<ETX>";
            #endregion
            #region IPL Body
            string sIPLBody = "";

            sIPLBody =  

                    /////////////////
                    ///// VLINE /////
                    /////////////////

                       "<STX>L200;o1000,0;;l850;f3;w1;<ETX> "

                    /////////////////
                    ///// ROW 1 /////
                    /////////////////

                        // FROM AREA
                    + "<STX>H001;o01,00;f0;c26;w1;h1;k8;d3,From<ETX> "
                    + "<STX>H002;o70,00;f0;c26;w1;h1;k9;d3,DECOSTAR<ETX> "
                    + "<STX>H003;o70,40;f0;c26;w1;h1;k9;d3,1 DECOMA DRIVE<ETX> "
                    + "<STX>H004;o70,80;f0;c26;w1;h1;k9;d3,CARROLLTON, GA 30117<ETX> "
                    + "<STX>H005;o01,130;f0;c26;w1;h1;k8;d3,Made in<ETX> "
                    + "<STX>H006;o120,125;f0;c26;w1;h1;k10;d3,USA<ETX> "

                        // FROM/TO VERTICAL SEPARATOR
                    + "<STX>L007;o375,0;;l180;f3;w1;<ETX> "

                        // TO AREA
                    + "<STX>H008;o380,00;f0;c26;w1;h1;k8;d3,To<ETX> "
                    + "<STX>H009;o400,025;f0;c26;w1;h2;k12;d3," + "VWGoA, 120 Intermodel Parkway" + "<ETX> "
                    + "<STX>H010;o400,100;f0;c26;w1;h2;k12;d3," + "Fort Worth, TX 76177" + "<ETX> "

                        // SMI AREA
                    + "<STX>H011;o1010,0;f0;c26;w1;h1;k8;d3,SMI<ETX> "
                    + "<STX>H012;o1100,0;f0;c26;w5;h1;k35;d3,M<ETX> "

                        // SMI/PLANT Horizontal Separator
                    + "<STX>L013;o1000,120;;l225;f0;w1;<ETX> "

                        // PLANT AREA
                    + "<STX>H014;o1010,120;f0;c26;w1;h1;k8;d3,Plant<ETX> "
                    + "<STX>H015;o1100,130;f0;c26;w1;h1;k12;d3,0515<ETX> "

                        // ROW 1/2 DIVIDER
                    +  "<STX>L016;o0,180;;l1225;f0;w1;<ETX> "


                    /////////////////
                    ///// ROW 2 /////
                    /////////////////

                        // MATERIAL AREA
                    + "<STX>H020;o000,193;f0;c26;w1;h1;k9;d3,Material<ETX> "
                    + "<STX>H021;o150,190;f0;c26;w1;h1;k9;d3," + part.Description2 + "<ETX> "
                    + "<STX>B022;o210,235;f0;c6,0;i0;w3;h80;d3," + part.CustomerNumber + ";<ETX>"
                    + "<STX>H023;o150,315;f0;c26;w1;h1;k20;d3," + part.CustomerNumber + ";<ETX> "

                        // GROSS WT. AREA
                    + "<STX>H024;o1010,190;f0;c26;w1;h1;k8;d3,Gross Wt.<ETX> "
                    + "<STX>H025;o1010,230;f0;c26;w1;h1;k10;d3,12345678 Lbs<ETX> "

                        // WT/DATE HORIZONTAL SEPERATOR
                    + "<STX>L026;o1000,295;;l225;f0;w1;<ETX> "
                        
                        // DATE AREA
                    + "<STX>H027;o1010,300;f0;c26;w1;h1;k8;d3,Shipment Date<ETX> "
                    + "<STX>H028;o1020,340;f0;c26;w1;h1;k10;d3," + DateTime.Now.ToShortDateString() + "<ETX> "

                        // ROW 2/3 DIVIDER
                    + "<STX>L029;o0,410;;l1225;f0;w1;<ETX> "


                    /////////////////
                    ///// ROW 3 /////
                    /////////////////

                        // SUPPLIER AREA
                    + "<STX>H030;o000,455;f0;c26;w1;h1;k9;d3,Supplier<ETX> "
                    + "<STX>H031;o220,435;f0;c26;w1;h1;k18;d3,1234567890<ETX> "
                    + "<STX>B032;o580,435;f0;c6,0;i0;w4;h80;d3,1234567890;<ETX>"

                        // QUANTITY AREA
                    + "<STX>H033;o1010,415;f0;c26;w1;h1;k9;d3,Qty per Pack<ETX> "
                    + "<STX>H034;o1025,445;f0;c26;w1;h1;k18;d3," + rack.Quantity + "<ETX> "

                        // SUPPLIER/SHIPMENT HORIZONTAL SEPARATOR
                    + "<STX>L035;o0,540;;l1225;f0;w1;<ETX> "

                    /////////////////
                    ///// ROW 4 /////
                    /////////////////

                        // SHIPMENT AREA
                    + "<STX>H040;o000,585;f0;c26;w1;h1;k9;d3,Shipment<ETX> "
                    + "<STX>B041;o220,565;f0;c6,0;i0;w4;h80;d3," + rack.PurchaseOrder + ";<ETX>"
                    + "<STX>H042;o650,565;f0;c26;w1;h1;k18;d3," + rack.PurchaseOrder + "<ETX> "

                        // SHIPMENT/SERIAL HORIZONTAL SEPARATOR
                    + "<STX>L043;o0,670;;l1225;f0;w1;<ETX> "

                    /////////////////
                    ///// ROW 5 /////
                    /////////////////

                        // LICENCE PLATE AREA
                    + "<STX>H050;o000,715;f0;c26;w1;h1;k9;d3,License Plate<ETX> "
                    + "<STX>H051;o220,695;f0;c26;w1;h1;k18;d3," + rack.RackID + ";<ETX> "
                    + "<STX>B052;o580,695;f0;c6,0;i0;w4;h80;d3," + rack.RackID + ";<ETX>"

                        // EXPIRATION AREA
                    + "<STX>H053;o1010,675;f0;c26;w1;h1;k9;d3,Expiry Date<ETX> "
                    + "<STX>H054;o1025,705;f0;c26;w1;h1;k18;d3,2020<ETX> "

                    +  "";
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

        protected override void printZPL(object rack)
        {            
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}
