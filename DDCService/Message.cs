using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DecoAPI.DDCService
{
    public class Message
    {
        public string UnitPrinter { get; set; }
        public string BoxPrinter { get; set; }
        public string MoldLabel { get; set; }
        public string MoldPartNumber { get; set; }
        public bool Success { get; set; }
        public int Quantity { get; set; }
        public string Notification { get; set; }
        public int MaxRackQuantity { get; set; }
    }
}