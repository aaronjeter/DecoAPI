using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DecoAPI.Unload
{
    public class Rack
    {
        public Rack()
        {
            Parts = new List<string>();
        }

        public bool IsDuplicate(string paintLabel)
        {
            foreach (string s in Parts)
            {
                if (s.Equals(paintLabel))
                {
                    return true;
                }
            }
            return false;
        }

        public string RackID { get; set; }

        public string ScanType { get; set; }

        public string PartNumber { get; set; }

        public string PartDescription { get; set; }

        public string MoldPartNumber { get; set; }

        public List<string> Parts { get; set; }

        public bool Error { get; set; }

        public string Message { get; set; }
    }
}