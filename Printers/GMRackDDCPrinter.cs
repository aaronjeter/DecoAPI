using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DecoAPI.Printers
{
    public class GMRackDDCPrinter : Printer
    {
        #region Overloaded Constructor
        public GMRackDDCPrinter (string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }

        #endregion

        #region Print Overrides
        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            Models.sPart part = new Models.Part().GetPartByPartNumber(rack.PartNumber);
            
            string partDesc1 = part.Description1;
            string partDesc2 = part.Description2;
            string colorDesc = part.DescColor;
            string revision = part.RevisionLevel;
            string custPart = part.CustomerNumber;
            string purchaseOrder = string.Empty;
            string plantDock = string.Empty;
            string materialCode = string.Empty;

            if (rack.ServiceRack && rack.ShipLocation != "BowlingService")
            {
                purchaseOrder = rack.PurchaseOrder;
            }
            else
            {
                // Find the Purchase Order based on the Location & Part Number.
                //  The information is stored in RackRecords.dbo.PurchaseOrders
                Models.Database database = new Models.Database();
                string query = " select purchase_order, "
                              + "        plant_dock, "
                              + "        material_code "
                              + " from RackRecords.dbo.PurchaseOrders po, "
                              + " RackRecords.dbo.ShippingAddresses sa "
                              + " where po.part_number = '" + part.Number + "' "
                              + " and sa.ship_location_txt = '" + rack.ShipLocation + "' "
                              + " and po.address_id = sa.address_id ";
                try
                {
                    SqlDataReader reader = database.RunCommandReturnReader(query);
                    purchaseOrder = reader["purchase_order"].ToString();
                    plantDock = reader["plant_dock"].ToString();
                    materialCode = reader["material_code"].ToString();
                }
                catch
                {
                    throw new Exception("PO #, Dock, or Material Code not set up in system.");
                }
            }

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
            sBarcodeData += "1JUN830616160" + rack.RackID + "<GS>";
            // Material Handling Code
            sBarcodeData += "20L" + materialCode.Trim() + "<GS>";
            // Plant/Delivery Dock
            sBarcodeData += "21L" + plantDock.Trim() + "<GS>";
            // PO Number
            sBarcodeData += "K" + purchaseOrder.Trim() + "<GS>";
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
                sIPLBody =  // FROM AREA
                            "<STX>H01;o0,0;f0;c31;w1;h1;k8;d3,FROM:<ETX> "
                         +  "<STX>H02;o0,25;f0;c31;w1;h1;k12;d3,DECOSTAR<ETX> "
                         +  "<STX>H03;o0,50;f0;c31;w1;h1;k12;d3,1 DECOMA DRIVE<ETX> "
                         +  "<STX>H04;o0,75;f0;c31;w1;h1;k12;d3,CARROLLTON, GA 30117<ETX> "

                            // FROM/TO VERTICAL SEPARATOR
                         +  "<STX>L05;o325,200;;l200;f1;w1;<ETX> "

                            // TO AREA
                         +  "<STX>H06;o337,0;f0;c31;w1;h1;k8;d3,TO:<ETX> "
                         +  "<STX>H07;o337,25;f0;c31;w1;h1;k12;d3," + sLocation1 + "<ETX> "
                         +  "<STX>H08;o337,50;f0;c31;w1;h1;k12;d3," + sLocation2 + "<ETX> "
                         +  "<STX>H09;o337,75;f0;c31;w1;h1;k12;d3," + sLocation3 + "<ETX> "
                         +  "<STX>H10;o337,100;f0;c31;w1;h1;k12;d3," + sLocation4 + "<ETX> "
                         +  "<STX>H11;o337,125;f0;c31;w1;h1;k12;d3,PLANT/DOCK:<ETX> "
                         +  "<STX>H12;o500,107;f0;c31;w1;h1;k22;d3," + plantDock + "<ETX> "

                            // 2D BARCODE AREA
                         +  "<STX>B13;o850,10;f0;c12,0,3;i0;w1;h1;d3," + sBarcodeData + "<ETX> "


                            // FROM/QUANTITY HORIZONTAL SEPARATOR
                         +  "<STX>L14;o0,200;;l1120;f0;w1;<ETX> "

                            // QUANTITY AREA
                         +  "<STX>H15;o0,200;f0;c26;w1;h1;k8;d3,QUANTITY<ETX> "
                         +  "<STX>H16;o25,202;f0;c26;w1;h1;k32;d3," + rack.Quantity + "<ETX> "

                            // QUANTITY/MATERIAL VERTICAL SEPARATOR
                         +  "<STX>L17;o450,300;;l100;f1;w1;<ETX> "

                            // MATERIAL AREA
                         +  "<STX>H18;o475,200;f0;c26;w1;h1;k8;d3,MATERIAL HANDLING CODE:<ETX> "
                         +  "<STX>H19;o475,210;f0;c26;w1;h1;k22;d3," + materialCode + "<ETX> "

                            // MATERIAL/REFERENCE VERTICAL SEPARTATOR
                         +  "<STX>L20;o800,300;;l100;f1;w1;<ETX> "

                            // REFERENCE AREA
                         //+  "<STX>H20;o825,200;f0;c26;w1;h1;k8;d3,REFERENCE:<ETX> "

                            // QUANTITY/PART# HORIZONTAL SEPARATOR
                         +  "<STX>L22;o0,300;;l1120;f0;w1;<ETX> "

                            // PART# AREA
                         +  "<STX>H23;o0,300;f0;c26;w1;h1;k8;d3,PART NUMBER<ETX> "
                         +  "<STX>H24;o200,265;f0;c26;w1;h1;k45;d3," + part.CustomerNumber + "<ETX> "

                            // PART#/SERIAL HORIZONTAL SEPARATOR
                         +  "<STX>L25;o0,400;;l1120;f0;w1;<ETX> "

                            // SERIAL AREA
                         +  "<STX>H26;o0,402;f0;c26;w1;h1;k8;d3,LICENSE PLATE (1J)<ETX> "
                         +  "<STX>B27;o25,430;f0;c6,0;i0;w3;h100;d3,1JUN830616160" + rack.RackID + "<ETX> "
                         +  "<STX>H28;o25,525;f0;c26;w1;h1;k22;d3,UN 830616160 " + rack.RackID + "<ETX> "

                            // SERIAL/PRODUCTION VERTICAL SEPARATOR
                         +  "<STX>L29;o800,600;;l200;f1;w1;<ETX> "

                            // PRODUCTION AREA
                         +  "<STX>H30;o805,405;f0;c26;w1;h1;k8;d3,PRODUCTION DATE:<ETX> "
                         +  "<STX>H31;o810,415;f0;c39;w1;h1;k12;d3," + sDate + "<ETX> "
                         +  "<STX>H32;o805,490;f0;c26;w1;h1;k8;d3,CONTAINTER TYPE:<ETX> "
                         +  "<STX>H33;o810,510;f0;c26;w1;h1;k12;d3," + "PCS71" + "<ETX> "
                         +  "<STX>H34;o805,540;f0;c26;w1;h1;k8;d3,GROSS WEIGHT:<ETX> "
                         +  "<STX>H35;o810,560;f0;c26;w1;h1;k12;d3," + iGrossWeight + " KG" + "<ETX> "

                            // SERIAL/DESCRIPTION HORIZONTAL SEPARATOR
                         +  "<STX>L36;o0,600;;l1120;f0;w1;<ETX> "

                            // DESCRIPTION AREA
                         +  "<STX>H37;o0,605;f0;c26;w1;h1;k8;d3,PART DESC<ETX> "
                         +  "<STX>H38;o0,620;f0;c26;w1;h1;k21;d3," + part.Description2 + "<ETX> "
                         +  "<STX>H39;o0,665;f0;c26;w1;h1;k21;d3," + "" + "<ETX> "
                         
                            // DECOSTAR PART NUMBER
                         +  "<STX>H40;o0,710;f0;c26;w1;h1;k12;d3," + "DECOSTAR # " + part.Number + ";<ETX>"
                         +  "";
            }
            else
            {
                throw new Exception("Printer dpi not a valid value.");
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

        protected override void printZPL(object rack)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}