using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DecoAPI.LabelReprint
{
    public class GMLabel
    {
        public string PartNumber { get; set; }
        public int Quantity { get; set; }
        public string ShipLocation { get; set; }
        public string PurchaseOrder { get; set; }

        public string Printer { get; set; }

        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }

        public bool Error { get; set; }
        public string Message { get; set; }
    }
}