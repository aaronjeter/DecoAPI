using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DecoAPI.Unload
{
    public class Session
    {
        public string Printer { get; set; }

        public string PassPrinter { get; set; }

        public string PaintLabel { get; set; }
        public List<string> PaintLabels { get; set; }

        public string MoldPart { get; set; }

        public int RackID { get; set; }
        public int RackQuantity { get; set; }
        public string PartDescription { get; set; }

        public Models.AbstractRack Rack { get; set; }

        public bool Error { get; set; }

        public string Message { get; set; }

        public string PartNumber { get; set; }

        public string Style { get; set; }

        public bool SingleScan { get; set; }

        public bool HasRack()
        {
            if (RackID != 0)
            {
                return true;
            }
            return false;
        }

        public void ClearPart()
        {
            PaintLabel = "";
            MoldPart = "";
        }       
 
        public void ClearRack()
        {
            RackID = 0;
            RackQuantity = 0;
            PartDescription = "";
            Rack = null;
        }
    }   
}