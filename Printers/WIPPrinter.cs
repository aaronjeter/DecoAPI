using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    public class WIPPrinter : Printer
    {
        #region Overloaded Constructor

        public WIPPrinter(string sID, string application, Models.AbstractRack oRack) : base(sID, application, oRack) { }

        #endregion

        #region Print Overrides

        protected override void printIPL(object rack)
        {
            Models.objWIPRack wipRack;
            if (rack is Models.objWIPRack)
            {
                wipRack = (Models.objWIPRack)rack;
            }
            else
            {
                throw new Exception("Can only print WIP Racks");
            }
            Models.sPart WIPPart = new Models.sPart();
            //part partInfo = new part();
            WIPPart = new Models.Part().GetPartByPartNumber(wipRack.PartNumber);

            string sFirstFullDesc = WIPPart.DescFull;
            if (sFirstFullDesc.Length > 24)
            {
                sFirstFullDesc = sFirstFullDesc.Substring(0, 24);
            }

            // Make Molded Date
            string sMoldedDate = wipRack.MoldTime.Month + "/" + wipRack.MoldTime.Day + " " + wipRack.MoldTime.ToShortTimeString();

            // Get Type of WIP Label
            string sWIPType = wipRack.Type.ToString().ToUpper();
            string sColorType = string.Empty;
            
            string sPrefix = "|WIP|";
            if (wipRack.Type == Models.WIPRackType.REWORK)
                sWIPType = "READY FOR REPAINT";
            if (wipRack.Type == Models.WIPRackType.FINESSE)
                sWIPType = "READY FOR FINESSE";
            if (wipRack.Type == Models.WIPRackType.LightColor)
            {
                sWIPType = "READY FOR REPAINT";
                sColorType = "Light Color";
                sPrefix = "|RWK|";
            }
            if (wipRack.Type == Models.WIPRackType.DarkColor)
            {
                sWIPType = "READY FOR REPAINT";
                sColorType = "Dark Color";
                sPrefix = "|RWK|";
            }

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
            if (DPI == "1000")
            {
                sIPLBody = "<STX>H1;o60,65;f0;c26;w1;h1;k12;d3,Part Description;<ETX>" +
                            "<STX>H2;o60,120;f0;c26;w1;h1;k27;d3," + sFirstFullDesc + ";<ETX>" +
                            "<STX>H3;o60,275;f0;c26;w1;h1;k27;d3," + WIPPart.Description2 + ";<ETX>" +
                            "<STX>L4;o80,525;;l2245;f0;w1;<ETX>" +
                            "<STX>H5;o50,768;f3;c26;w1;h1;k8;d3," + application + ";<ETX>" +
                            "<STX>H6;o60,530;f0;c26;w1;h1;k12;d3,Quantity;<ETX>" +
                            "<STX>H7;o80,550;f0;c26;w1;h200;k46;d3," + wipRack.Quantity + ";<ETX>" +
                            "<STX>L9;o630,525;;l485;f3;w1;<ETX>" +
                            "<STX>L10;o80,850;;l1625;f0;w1;<ETX>" +
                            "<STX>H12;o750,500;f0;c26;w1;h1;k30;d3," + sWIPType + "<ETX>";
                            if (wipRack.Type == Models.WIPRackType.LightColor || wipRack.Type == Models.WIPRackType.DarkColor)
                            {
                                sIPLBody += "<STX>H14;o650,700;f0;c26;w1;h1;k23;d3," + sColorType + "<ETX>";
                            }
                            if (wipRack.Type == Models.WIPRackType.FINISHED)
                            {
                                sIPLBody += "<STX>H13;o650,670;f0;c26;w1;h1;k11;d3,Molded:;<ETX>" +
                                            "<STX>H14;o650,700;f0;c26;w1;h1;k23;d3," + sMoldedDate + "<ETX>";
                            }
                sIPLBody += "<STX>H15;o60,855;f0;c26;w1;h1;k12;d3,Part #;<ETX>" +
                            "<STX>H16;o60,950;f0;c26;w1;h1;k72;d3," + WIPPart.Number + "<ETX>" +
                            "<STX>L18;o1720,525;;l1000;f3;w1;<ETX>" +
                            "<STX>L19;o80,1460;;l1625;f0;w1;<ETX>" +
                            "<STX>H20;o60,1440;f0;c26;w1;h1;k21;d3," + DateTime.Now.ToString() + ";<ETX>" +
                            "<STX>H21;o1740,1575;f1;c26;w1;h1;k12;d3,Serial Number;<ETX>" +
                            "<STX>H22;o1760,1575;f1;c26;w1;h1;k36;d3," + wipRack.id.ToString() + "<ETX>" +
                            "<STX>B23;o1990,1575;f1;c6,0;i0;w6;h350;d3," + sPrefix + wipRack.id.ToString() + "<ETX>";
            }
            else if (DPI == "200" || DPI == "400")
            {
                sIPLBody = "<STX>H1;o30,32;f0;c26;w1;h1;k12;d3,Part Description;<ETX>" +
                            "<STX>H2;o30,60;f0;c26;w1;h1;k27;d3," + sFirstFullDesc + ";<ETX>" +
                            "<STX>H3;o30,137;f0;c26;w1;h1;k27;d3," + WIPPart.Description2 + ";<ETX>" +
                            "<STX>L4;o40,262;;l1122;f0;w1;<ETX>" +
                            "<STX>H5;o25,384;f3;c26;w1;h1;k8;d3," + application + ";<ETX>" +
                            "<STX>H6;o30,265;f0;c26;w1;h1;k12;d3,Quantity;<ETX>" +
                            "<STX>H7;o40,275;f0;c26;w1;h200;k46;d3," + wipRack.Quantity + ";<ETX>" +
                            "<STX>L9;o315,262;;l242;f3;w1;<ETX>" +
                            "<STX>L10;o40,425;;l812;f0;w1;<ETX>" +
                            "<STX>H12;o375,250;f0;c26;w1;h1;k30;d3," + sWIPType + "<ETX>";
                            if (wipRack.Type == Models.WIPRackType.LightColor || wipRack.Type == Models.WIPRackType.DarkColor)
                            {
                                sIPLBody += "<STX>H13;o325,335;f0;c26;w1;h1;k11;d3,Molded:;<ETX>" +
                                            "<STX>H14;o325,350;f0;c26;w1;h1;k23;d3," + sColorType + "<ETX>";
                            }
                            if (wipRack.Type == Models.WIPRackType.FINISHED)
                            {
                                sIPLBody += "<STX>H13;o325,335;f0;c26;w1;h1;k11;d3,Molded:;<ETX>" +
                                            "<STX>H14;o325,350;f0;c26;w1;h1;k23;d3," + sMoldedDate + "<ETX>";
                            }
                sIPLBody += "<STX>H15;o30,427;f0;c26;w1;h1;k12;d3,Part #;<ETX>" +
                            "<STX>H16;o30,475;f0;c26;w1;h1;k72;d3," + WIPPart.Number + "<ETX>" +
                            "<STX>L18;o860,262;;l500;f3;w1;<ETX>" +
                            "<STX>L19;o40,730;;l812;f0;w1;<ETX>" +
                            "<STX>H20;o30,720;f0;c26;w1;h1;k21;d3," + DateTime.Now.ToString() + ";<ETX>" +
                            "<STX>H21;o870,787;f1;c26;w1;h1;k12;d3,Serial Number;<ETX>" +
                            "<STX>H22;o880,787;f1;c26;w1;h1;k36;d3," + wipRack.id.ToString() + "<ETX>" +
                            "<STX>B23;o995,787;f1;c6,0;i0;w3;h175;d3," + sPrefix + wipRack.id.ToString() + "<ETX>";
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
            Models.objWIPRack wipRack = (Models.objWIPRack)rack;
            Models.sPart WIPPart = new Models.sPart();
            WIPPart = new Models.Part().GetPartByPartNumber(wipRack.PartNumber);

            string sFirstFullDesc = WIPPart.DescFull;
            if (sFirstFullDesc.Length > 24)
            {
                sFirstFullDesc = sFirstFullDesc.Substring(0, 24);
            }

            Random rnd = new Random();
            int RandomNumber = rnd.Next(9999);
            string sDropFile = "\\\\decos07t\\RackLabel\\";
            sDropFile += "wip" + DateTime.Now.ToString().Replace("/", "_").Replace(":", "_").Replace(" ", "_") + RandomNumber.ToString();
            sDropFile += ".xml";

            System.IO.StreamWriter sw = new System.IO.StreamWriter(sDropFile, false);
            sw.WriteLine("<?xml version=\"1.0\" standalone=\"no\"?>");
            sw.WriteLine("<!DOCTYPE labels>");
            sw.WriteLine("<labels _FORMAT=\"template_ZPLWIPLabel.xml\" _PRINTERNAME=\"" + name + "\" _QUANTITY=\"2\">");
            sw.WriteLine("<label>");
            sw.WriteLine("<variable name=\"AppName\">" + application + "</variable>");
            sw.WriteLine("<variable name=\"QADPartDescription1\">" + WIPPart.DescFull.Substring(0, 24) + "</variable>");
            sw.WriteLine("<variable name=\"QADPartDescription2\">" + WIPPart.Description2 + "</variable>");
            sw.WriteLine("<variable name=\"quantity\">" + wipRack.Quantity + "</variable>");
            sw.WriteLine("<variable name=\"quantity1\">" + wipRack.Quantity + "</variable>");
            sw.WriteLine("<variable name=\"type\">" + wipRack.Type + "</variable>");
            sw.WriteLine("<variable name=\"partID\">" + WIPPart.Number + "</variable>");
            sw.WriteLine("<variable name=\"partID1\">" + WIPPart.Number + "</variable>");
            sw.WriteLine("<variable name=\"dateTimeStamp\">" + System.DateTime.Now.ToString() + "</variable>");
            sw.WriteLine("<variable name=\"serialNumber\">" + wipRack.id.ToString() + "</variable>");
            sw.WriteLine("<variable name=\"serialNumberWithPrefix\">|WIP|" + wipRack.id.ToString() + "</variable>");
            sw.WriteLine("<variable name=\"MoldTime\">" + wipRack.MoldTime.ToString() + "</variable>");
            sw.WriteLine("</label>");
            sw.WriteLine("</labels>");
            sw.Close();


        }

        #endregion
    }
}
