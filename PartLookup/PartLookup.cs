using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;
using DecoAPI.Models;

namespace DecoAPI.PartLookup
{
    public class PartLookup
    {
        //class to look up part by paint/mold label, and return all available information

        public string GetScanType(string paintlabel)
        {
            return Unload.RackCreator.GetScanType(paintlabel);
        }

        public sPart SingleScan(string paintBarcode)
        {
            PaintLabel paintlabel = new PaintLabel(paintBarcode);
            Style style = new Style(paintlabel.Style);
            Color color = new Color(paintlabel.Color);
            sPart part = part = new Part().GetPaintedPart(style.BasePartNumber, paintBarcode);
            return part;
        }

        public void GetPartData(string paintBarcode, string moldBarcode = "")
        {
            PaintLabel label;
            Style style;
            Color color;

            //verify that label exists
            try
            {
                label = new PaintLabel(paintBarcode);
            }
            catch (Exception)
            {
                throw new Exception("Paint Label Does not Exist");
            }

            //see if style is setup in styles_xref table
            try
            {
                style = new Style(label.Style);
            }
            catch (Exception)
            {
                throw new Exception("Paint Label Style not set up");
            }

            //see if color is setup in xref table
            try
            {
                color = new Color(label.Color);
            }
            catch(Exception)
            {
                throw new Exception("Paint Label Color not set up");
            }

            if (style.ScanType == "single")
            {
                //process single scan part
            }
            else
            {
                if (moldBarcode == "")
                {
                    throw new Exception("This is a double scan part. Please scan a mold label for more information");
                }
                else
                {
                    //process double scan part
                }
            }
            
        }
    }
}