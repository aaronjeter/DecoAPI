using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DecoAPI.LabelReprint
{
    public class CustomerLabel
    {
        public string PartNumber { get; set; }

        public int PartsPerLabel { get; set; }

        public int LabelQuantity { get; set; }

        public string Printer { get; set; }

        public bool Error { get; set; }

        public string Message { get; set; }
    }
}