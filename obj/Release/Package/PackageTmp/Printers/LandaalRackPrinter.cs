using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace DecoAPI.Printers
{
    public class LandaalRackPrinter : Printer
    {
        #region Overloaded Constructor
        public LandaalRackPrinter(string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }
        #endregion

        #region Print Overrides
        protected override void printIPL(object o)
        {
            Models.AbstractRack rack = (Models.AbstractRack)o;
            Models.sPart part = new Models.Part().GetPartByPartNumber(rack.PartNumber);

            string partDesc1 = part.Description1;
            string partDesc2 = part.Description2;
            string colorDesc = part.DescColor;
            string rev = part.RevisionLevel;
            string custPart = part.CustomerNumber;
            string purchaseOrder = string.Empty;
            string plantDock = string.Empty;
            string materialCode = string.Empty;

            if (rack.ServiceRack)
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
                    reader.Read();
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
            double individualLB = Double.Parse(part.PartWeight); // PartWeight is in POUNDS
            double individualKG = individualLB / 2.2; // Convert to KILOGRAMS
            double quanity = rack.Quantity;
            //double iPalletKG = 21.77;
            //double iGrossWeight = Math.Round((iIndividualKG * iQuanity) + iPalletKG, 0);
            double grossWeight = Math.Round((individualKG * quanity), 0);

            // Build 2D Data
            string barcodeData = string.Empty;
            // Message Header
            barcodeData += "[)><RS>";
            // Format Header
            barcodeData += "06<GS>";
            // Part #
            barcodeData += "P" + part.CustomerNumber + "<GS>";
            // Quantity
            barcodeData += "Q" + rack.Quantity + "<GS>";
            // Serial #
            barcodeData += "1JUN129357955" + rack.RackID + "<GS>";
            // Material Handling Code
            barcodeData += "20L" + materialCode.Trim() + "<GS>";
            // Plant/Delivery Dock
            barcodeData += "21L" + plantDock.Trim() + "<GS>";
            // PO Number
            barcodeData += "K" + purchaseOrder.Trim() + "<GS>";
            // Container Type
            barcodeData += "BPCS71<GS>";
            // Gross Weight
            barcodeData += "7Q" + grossWeight + "GT";
            // Message Trailer
            barcodeData += "<RS><SUB><EOT>";

            // Build Date String
            string date = string.Empty;
            date += DateTime.Today.Day;
            date += DateTime.Now.ToString("MMMM").Substring(0, 3).ToUpper();
            date += DateTime.Today.Year;

            // Set Location
            string location1 = string.Empty;
            string location2 = string.Empty;
            string location3 = string.Empty;
            string location4 = string.Empty;

            location1 = address.Line1;
            location2 = address.Line2;
            location3 = address.Line3;
            location4 = address.Line4;            

            #region IPL Head
            string sIPLHead = "<STX><ESC>C1<ETX> " +
                    "<STX><ESC>k<ETX> " +
                    "<STX><SI>L1200<ETX> " +
                    "<STX><SI>W809<ETX> " +
                    "<STX><SI>S30<ETX> " +
                    "<STX><SI>d0<ETX> " +
                    "<STX><SI>l8<ETX> " +
                    "<STX><SI>I2<ETX> " +
                    "<STX><SI>F0<ETX> " +
                    "<STX><SI>D0<ETX> " +
                    "<STX><SI>t0<ETX> " +
                    "<STX><SI>k<ETX> " +
                    "<STX><SI>h0,0<ETX> " +
                    "<STX><SI>g1,567<ETX> " +
                    "<STX><SI>T1<ETX> " +
                    "<STX><SI>R1<ETX> " +
                    "<STX><SI>r0<ETX> " +
                    "<STX><ESC>P<ETX> " +
                    "<STX>E2;F2;<ETX> " +
                    "<STX>L1;<ETX> " +
                    "<STX>D1;<ETX> ";
            #endregion
            #region IPL Body
            string sIPLBody = "";
            if (DPI == "400")
            {
                sIPLBody =  // FROM AREA
                            "<STX>H01;o0,0;f0;c31;w1;h1;k8;d3,FROM:<ETX> "
                         + "<STX>H02;o0,50;f0;c31;w1;h1;k12;d3,DECOSTAR<ETX> "
                         + "<STX>H03;o0,100;f0;c31;w1;h1;k12;d3,1 DECOMA DRIVE<ETX> "
                         + "<STX>H04;o0,150;f0;c31;w1;h1;k12;d3,CARROLLTON, GA 30117<ETX> "

                            // FROM/TO VERTICAL SEPARATOR
                         + "<STX>L05;o650,400;;l400;f1;w1;<ETX> "

                            // TO AREA
                         + "<STX>H06;o675,0;f0;c31;w1;h1;k8;d3,TO:<ETX> "
                         + "<STX>H07;o675,50;f0;c31;w1;h1;k12;d3," + location1 + "<ETX> "
                         + "<STX>H08;o675,100;f0;c31;w1;h1;k12;d3," + location2 + "<ETX> "
                         + "<STX>H09;o675,150;f0;c31;w1;h1;k12;d3," + location3 + "<ETX> "
                         + "<STX>H10;o675,200;f0;c31;w1;h1;k12;d3," + location4 + "<ETX> "
                         + "<STX>H11;o675,250;f0;c31;w1;h1;k12;d3,PLANT/DOCK:<ETX> "
                         + "<STX>H12;o1000,215;f0;c31;w1;h1;k22;d3," + plantDock + "<ETX> "

                            // 2D BARCODE AREA
                         + "<STX>B13;o1700,20;f0;c12,0,6;i0;w3;h1;d3," + barcodeData + "<ETX> "

                            // FROM/QUANTITY HORIZONTAL SEPARATOR
                         + "<STX>L14;o0,400;;l2240;f0;w1;<ETX> "

                            // QUANTITY AREA
                         + "<STX>H15;o0,400;f0;c26;w1;h1;k8;d3,QUANTITY<ETX> "
                         + "<STX>H16;o50,405;f0;c26;w1;h1;k32;d3," + rack.Quantity + "<ETX> "

                            // QUANTITY/MATERIAL VERTICAL SEPARATOR
                         + "<STX>L17;o900,600;;l200;f1;w1;<ETX> "

                            // MATERIAL AREA
                         + "<STX>H18;o950,400;f0;c26;w1;h1;k8;d3,PURCHASE ORDER:<ETX> "
                         + "<STX>H19;o950,420;f0;c26;w1;h1;k22;d3," + purchaseOrder + "<ETX> "

                            // QUANTITY/PART# HORIZONTAL SEPARATOR
                         + "<STX>L22;o0,600;;l2240;f0;w1;<ETX> "

                            // PART# AREA
                         + "<STX>H23;o0,600;f0;c26;w1;h1;k8;d3,PART NUMBER<ETX> "
                         + "<STX>H24;o400,530;f0;c26;w1;h1;k45;d3," + rack.CustomerPartNumber + "<ETX> "

                            // PART#/SERIAL HORIZONTAL SEPARATOR
                         + "<STX>L25;o0,800;;l2240;f0;w1;<ETX> "

                            // SERIAL AREA
                         + "<STX>H26;o0,805;f0;c26;w1;h1;k8;d3,LICENSE PLATE (1J)<ETX> "
                         + "<STX>B27;o50,860;f0;c6,0;i0;w6;h200;d3,1JUN129357955" + rack.RackID + "<ETX> "
                         + "<STX>H28;o50,1050;f0;c26;w1;h1;k22;d3,UN 129357955 " + rack.RackID + "<ETX> "

                            // SERIAL/PRODUCTION VERTICAL SEPARATOR
                         + "<STX>L29;o1600,1200;;l400;f1;w1;<ETX> "

                            // PRODUCTION AREA
                         + "<STX>H30;o1610,810;f0;c26;w1;h1;k8;d3,PRODUCTION DATE:<ETX> "
                         + "<STX>H31;o1630,830;f0;c39;w1;h1;k12;d3," + date + "<ETX> "
                         + "<STX>H32;o1610,980;f0;c26;w1;h1;k8;d3,CONTAINTER TYPE:<ETX> "
                         + "<STX>H33;o1630,1020;f0;c26;w1;h1;k12;d3," + "PCS71" + "<ETX> "
                         + "<STX>H34;o1610,1080;f0;c26;w1;h1;k8;d3,GROSS WEIGHT:<ETX> "
                         + "<STX>H35;o1630,1120;f0;c26;w1;h1;k12;d3," + grossWeight + " KG" + "<ETX> "

                            // SERIAL/DESCRIPTION HORIZONTAL SEPARATOR
                         + "<STX>L36;o0,1200;;l2240;f0;w1;<ETX> "

                            // DESCRIPTION AREA
                         + "<STX>H37;o0,1210;f0;c26;w1;h1;k8;d3,PART DESC<ETX> "
                         + "<STX>H38;o0,1240;f0;c26;w1;h1;k21;d3," + part.Description2 + "<ETX> "
                         + "<STX>H39;o0,1330;f0;c26;w1;h1;k21;d3," + "" + "<ETX> "

                            // DECOSTAR PART #
                         + "<STX>H40;o0,1420;f0;c26;w1;h1;k12;d3," + "DECOSTAR # " + part.Number + ";<ETX>"


                         + "";

            }
            else if (DPI == "200")
            {
                sIPLBody =  // FROM AREA
                            "<STX>H01;o0,0;f0;c31;w1;h1;k8;d3,FROM:<ETX> "
                         + "<STX>H02;o0,25;f0;c31;w1;h1;k12;d3,DECOSTAR<ETX> "
                         + "<STX>H03;o0,50;f0;c31;w1;h1;k12;d3,1 DECOMA DRIVE<ETX> "
                         + "<STX>H04;o0,75;f0;c31;w1;h1;k12;d3,CARROLLTON, GA 30117<ETX> "

                            // FROM/TO VERTICAL SEPARATOR
                         + "<STX>L05;o325,200;;l200;f1;w1;<ETX> "

                            // TO AREA
                         + "<STX>H06;o337,0;f0;c31;w1;h1;k8;d3,TO:<ETX> "
                         + "<STX>H07;o337,25;f0;c31;w1;h1;k12;d3," + location1 + "<ETX> "
                         + "<STX>H08;o337,50;f0;c31;w1;h1;k12;d3," + location2 + "<ETX> "
                         + "<STX>H09;o337,75;f0;c31;w1;h1;k12;d3," + location3 + "<ETX> "
                         + "<STX>H10;o337,100;f0;c31;w1;h1;k12;d3," + location4 + "<ETX> "
                         + "<STX>H11;o337,125;f0;c31;w1;h1;k12;d3,PLANT/DOCK:<ETX> "
                         + "<STX>H12;o500,107;f0;c31;w1;h1;k22;d3," + plantDock + "<ETX> "

                            // 2D BARCODE AREA
                         + "<STX>B13;o850,10;f0;c12,0,3;i0;w1;h1;d3," + barcodeData + "<ETX> "


                            // FROM/QUANTITY HORIZONTAL SEPARATOR
                         + "<STX>L14;o0,200;;l1120;f0;w1;<ETX> "

                            // QUANTITY AREA
                         + "<STX>H15;o0,200;f0;c26;w1;h1;k8;d3,QUANTITY<ETX> "
                         + "<STX>H16;o25,202;f0;c26;w1;h1;k32;d3," + rack.Quantity + "<ETX> "

                            // QUANTITY/MATERIAL VERTICAL SEPARATOR
                         + "<STX>L17;o450,300;;l100;f1;w1;<ETX> "

                            // MATERIAL AREA
                         + "<STX>H18;o475,200;f0;c26;w1;h1;k8;d3,PURCHASE ORDER:<ETX> "
                         + "<STX>H19;o475,210;f0;c26;w1;h1;k22;d3," + purchaseOrder + "<ETX> "

                            // QUANTITY/PART# HORIZONTAL SEPARATOR
                         + "<STX>L22;o0,300;;l1120;f0;w1;<ETX> "

                            // PART# AREA
                         + "<STX>H23;o0,300;f0;c26;w1;h1;k8;d3,PART NUMBER<ETX> "
                         + "<STX>H24;o200,265;f0;c26;w1;h1;k45;d3," + rack.CustomerPartNumber + "<ETX> "

                            // PART#/SERIAL HORIZONTAL SEPARATOR
                         + "<STX>L25;o0,400;;l1120;f0;w1;<ETX> "

                            // SERIAL AREA
                         + "<STX>H26;o0,402;f0;c26;w1;h1;k8;d3,LICENSE PLATE (1J)<ETX> "
                         + "<STX>B27;o25,430;f0;c6,0;i0;w3;h100;d3,1JUN129357955" + rack.RackID + "<ETX> "
                         + "<STX>H28;o25,525;f0;c26;w1;h1;k22;d3,UN 129357955 " + rack.RackID + "<ETX> "

                            // SERIAL/PRODUCTION VERTICAL SEPARATOR
                         + "<STX>L29;o800,600;;l200;f1;w1;<ETX> "

                            // PRODUCTION AREA
                         + "<STX>H30;o805,405;f0;c26;w1;h1;k8;d3,PRODUCTION DATE:<ETX> "
                         + "<STX>H31;o810,415;f0;c39;w1;h1;k12;d3," + date + "<ETX> "
                         + "<STX>H32;o805,490;f0;c26;w1;h1;k8;d3,CONTAINTER TYPE:<ETX> "
                         + "<STX>H33;o810,510;f0;c26;w1;h1;k12;d3," + "PCS71" + "<ETX> "
                         + "<STX>H34;o805,540;f0;c26;w1;h1;k8;d3,GROSS WEIGHT:<ETX> "
                         + "<STX>H35;o810,560;f0;c26;w1;h1;k12;d3," + grossWeight + " KG" + "<ETX> "

                            // SERIAL/DESCRIPTION HORIZONTAL SEPARATOR
                         + "<STX>L36;o0,600;;l1120;f0;w1;<ETX> "

                            // DESCRIPTION AREA
                         + "<STX>H37;o0,605;f0;c26;w1;h1;k8;d3,PART DESC<ETX> "
                         + "<STX>H38;o0,620;f0;c26;w1;h1;k21;d3," + part.Description2 + "<ETX> "
                         + "<STX>H39;o0,665;f0;c26;w1;h1;k21;d3," + "" + "<ETX> "

                            // DECOSTAR PART NUMBER
                         + "<STX>H40;o0,710;f0;c26;w1;h1;k12;d3," + "DECOSTAR # " + part.Number + ";<ETX>"
                         + "";
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
            if (ph.SendToPrinter(sIPL) == false)
            {
                throw new Exception("Could not ping printer");
            }
        }

        protected override void printZPL(object _rack)
        {
            Models.AbstractRack rack = (Models.AbstractRack)_rack;
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}