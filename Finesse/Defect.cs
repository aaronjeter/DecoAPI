using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DecoAPI.Finesse
{
    public class Defect
    {
        public int ID { get; set; }
        public DateTime TimeStamp { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public string PartNumber { get; set; }

        public string EmployeeId { get; set; }
        public int DefectCategory { get; set; }
        public int DefectType { get; set; }

        public string Operation { get; set; }
    }
}