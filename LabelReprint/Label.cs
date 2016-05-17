using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DecoAPI.LabelReprint
{
    public class Label
    {   /// <summary>
        /// Name of Customer
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// Name of Program
        /// </summary>
        public string Program { get; set; }

        /// <summary>
        /// Object containing information about a paintlabel's selected Style
        /// </summary>
        public StyleInfo Style { get; set; }

        /// <summary>
        /// Object containing information about a paintlabel's selected Color
        /// </summary>
        public ColorInfo Color { get; set; }

        /// <summary>
        /// Object containing information about a moldlabel's selected Part
        /// </summary>
        public PartInfo Part { get; set; }

        /// <summary>
        /// Quantity of labels to be printed
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Type of label to be printed (mold or paint)
        /// </summary>
        public string LabelType { get; set; }

        /// <summary>
        /// name of user who authorized printing of label
        /// </summary>
        public string UserName { get; set; }

        public string Initials { get; set; }

        /// <summary>
        /// ID number of printer labels should print at
        /// </summary>
        public string PrinterID { get; set; }

        /// <summary>
        /// Boolean value for Ribbon Printing
        /// True for Ribbon, False for Thermal
        /// </summary>
        public bool Ribbon { get; set; }

        public string Reason { get; set; }

        public int shotCount { get; set; }

        public string Badge { get; set; }

        public Label()
        {

        }
    }

    public class PartInfo
    {
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Side { get; set; }

        public PartInfo()
        {

        }
    }

    public class StyleInfo
    {
        #region fields
        private string sDescription;
        private int iPlantNumber;
        #endregion

        #region constructors
        public StyleInfo()
        { }

        public StyleInfo(string sDescription, int iPlantNumber)
        {
            this.sDescription = sDescription;
            this.iPlantNumber = iPlantNumber;
        }

        #endregion

        #region properties
        public string Description
        {
            get
            {
                return sDescription;
            }
            set
            {
                sDescription = value;
            }
        }
        public int PlantNumber
        {
            get
            {
                return iPlantNumber;
            }
            set
            {
                iPlantNumber = value;
            }
        }
        #endregion

    }

    public class ColorInfo
    {
        #region fields
        private string sDescription;
        private int iPlantNumber;
        #endregion

        #region constructors
        public ColorInfo()
        {

        }
        public ColorInfo(string sDescription, int iPlantNumber)
        {
            this.sDescription = sDescription;
            this.iPlantNumber = iPlantNumber;
        }

        #endregion

        #region properties
        public string Description
        {
            get
            {
                return sDescription;
            }
            set
            {
                sDescription = value;
            }
        }
        public int PlantNumber
        {
            get
            {
                return iPlantNumber;
            }
            set
            {
                iPlantNumber = value;
            }
        }
        #endregion
    }
}
