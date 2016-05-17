using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

using DecoAPI.Models;
using DecoAPI.Employees;
using DecoAPI.LabelReprint;

namespace DecoAPI.Controllers
{
    public class LabelReprintController : ApiController
    {
        #region LabelReprint Methods

        /// <summary>
        /// Validate a user for LabelReprint Application
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        [Route("LabelReprint/ValidateUser/{badge}")]
        [HttpGet]
        public bool ValidateUser(string badge)
        {
            badge = HttpUtility.UrlDecode(badge);

            Employee e = new Employee();

            if (e.CheckLabelReprintUserList(badge))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method to Return List of Customers for Label Reprinting
        /// </summary>
        /// <returns></returns>
        [Route("LabelReprint/GetCustomers")]
        [HttpGet]
        public List<string> GetCustomers()
        {
            return new LabelReprint.DataHandler().GetCustomerList();
        }

        /// <summary>
        /// Method to Return List of Programs for a Customer
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>    
        [Route("LabelReprint/GetPrograms")]
        [HttpPost]
        public List<string> GetPrograms([FromBody] LabelReprint.Label label)
        {
            string customer = label.Customer;
            return new LabelReprint.DataHandler().GetProgramList(customer);
        }

        /// <summary>
        /// Method to return List of Styles for a customer
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>    
        [Route("LabelReprint/GetStyles")]
        [HttpPost]
        public List<StyleInfo> GetStyles([FromBody] LabelReprint.Label label)
        {
            string customer = label.Customer;
            return new LabelReprint.DataHandler().GetStyleList(customer);
        }

        /// <summary>
        /// Method to return Mold parts for a Customer/Program combination
        /// Currently returns Mold (mold), Mold Purchased (moldp), Mold Assembled (molda), and Mold Punched (punc100) parts marked active in pt master sql
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>        
        [Route("LabelReprint/GetParts")]
        [HttpPost]
        public List<PartInfo> GetParts([FromBody] LabelReprint.Label label)
        {
            string customer = label.Customer;
            string program = label.Program;
            return new LabelReprint.DataHandler().GetPartList(customer, program);
        }

        /// <summary>
        /// Method to return list of colors for a program
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        [Route("LabelReprint/GetColors")]
        [HttpPost]
        public List<ColorInfo> GetColors([FromBody] LabelReprint.Label label)
        {
            string program = label.Program;
            return new LabelReprint.DataHandler().GetColorList(program);
        }        

        /// <summary>
        /// Method to print paint/mold label given a filled out label object
        /// </summary>
        /// <param name="label"></param>
        [Route("LabelReprint/PrintLabels")]
        [HttpPost]
        public void PrintLabels([FromBody] LabelReprint.Label label)
        {
            new Printers.PrintHandler(label);
        }

        #endregion

        #region CustomerLabelPrinter Methods

        /// <summary>
        /// Method to print Small Honda Part Labels for CustomerLabelPrinter application
        /// </summary>
        /// <param name="label"></param>
        [Route("LabelReprint/PrintSmallHondaPartLabel")]
        [HttpPost]
        public LabelReprint.CustomerLabel PrintSmallHondaPartLabel([FromBody] LabelReprint.CustomerLabel label)
        {
            try 
            {
                for (int i = 0; i < label.LabelQuantity; i++)
                {
                    Printers.MiscellaneousLabelPrinter.PrintSmallHondaPartLabel(label.PartNumber, label.Printer);
                }

                label.Error = false;
                label.Message = "Labels Printed";
                    
            }
            catch (Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;
        }

        /// <summary>
        /// Method to print Large Honda Part Labels for CustomerLabelPrinter application
        /// </summary>
        /// <param name="label"></param>
        [Route("LabelReprint/PrintLargeHondaPartLabel")]
        [HttpPost]
        public LabelReprint.CustomerLabel PrintLargeHondaPartLabel([FromBody] LabelReprint.CustomerLabel label)
        {
            try
            {
                for (int i = 0; i < label.LabelQuantity; i++)
                {
                    Printers.MiscellaneousLabelPrinter.PrintLargeHondaPartLabel(label.PartNumber, label.Printer);
                }

                label.Error = false;
                label.Message = "Labels Printed";
            }
            catch (Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;            
        }

        /// <summary>
        /// Method to print Nissan Service Labels for CustomerLabelPrinter application
        /// </summary>
        /// <param name="label"></param>
        [Route("LabelReprint/PrintNissanServiceLabel")]
        [HttpPost]
        public LabelReprint.CustomerLabel PrintNissanServiceLabel([FromBody] LabelReprint.CustomerLabel label)
        {
            try
            {
                for (int i = 0; i < label.LabelQuantity; i++)
                {
                    Printers.MiscellaneousLabelPrinter.PrintNissanServiceLabel(label.PartNumber, label.Printer);
                }

                label.Error = false;
                label.Message = "Labels Printed";
            }
            catch (Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;
        }

        /// <summary>
        /// Method to print Small GM Service Labels for CustomerLabelPrinter application
        /// </summary>
        /// <param name="label"></param>
        [Route("LabelReprint/PrintSmallGMServiceLabel")]
        [HttpPost]
        public LabelReprint.CustomerLabel PrintSmallGMServiceLabel([FromBody] LabelReprint.CustomerLabel label)
        {
            try
            {
                for (int i = 0; i < label.LabelQuantity; i++)
                {
                    Printers.MiscellaneousLabelPrinter.PrintSmallGMServiceLabel(label.PartNumber, label.PartsPerLabel, label.Printer);
                }

                label.Error = false;
                label.Message = "Labels Printed";
            }
            catch (Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;
        }

        [Route("LabelReprint/PrintLargeGMServiceLabel")]
        [HttpPost]
        public LabelReprint.CustomerLabel PrintLargeGMServiceLabel([FromBody] LabelReprint.CustomerLabel label)
        {
            try
            {
                for (int i = 0; i < label.LabelQuantity; i++)
                {
                    Printers.MiscellaneousLabelPrinter.PrintLargeGMServiceLabel(label.PartNumber, label.PartsPerLabel, label.Printer);
                }

                label.Error = false;
                label.Message = "Labels Printed";
            }
            catch (Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;
        }

        [Route("LabelReprint/PrintSmallGMDOTLabel")]
        [HttpPost]
        public LabelReprint.CustomerLabel PrintSmallGMDOTLabel([FromBody] LabelReprint.CustomerLabel label)
        {
            try
            {
                for (int i = 0; i < label.LabelQuantity; i++)
                {
                    Printers.MiscellaneousLabelPrinter.PrintSmallGMDOTLabel(label.PartNumber, label.PartsPerLabel, label.Printer);
                }

                label.Error = false;
                label.Message = "Labels Printed";
            }
            catch (Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;
        }

        [Route("LabelReprint/PrintLargeGMDOTLabel")]
        [HttpPost]
        public LabelReprint.CustomerLabel PrintLargeGMDOTLabell([FromBody] LabelReprint.CustomerLabel label)
        {  
            try
            {
                for (int i = 0; i < label.LabelQuantity; i++)
                {
                    Printers.MiscellaneousLabelPrinter.PrintLargeGMDOTLabel(label.PartNumber, label.PartsPerLabel, label.Printer);
                }

                label.Error = false;
                label.Message = "Labels Printed";
            }
            catch (Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;
        }

        [Route("LabelReprint/PrintDOTLabel")]
        [HttpPost]
        public LabelReprint.CustomerLabel PrintDOTLabell([FromBody] LabelReprint.CustomerLabel label)
        {
            try
            {
                for (int i = 0; i < label.LabelQuantity; i++)
                {
                    Printers.MiscellaneousLabelPrinter.PrintDOTLabel(label.PartsPerLabel, label.Printer);
                }

                label.Error = false;
                label.Message = "Labels Printed";
            }
            catch (Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;
        }

        #endregion

        #region GMSHippingLabelPrinter Methods

        [Route("LabelReprint/GetGMParts")]
        [HttpGet]
        public List<LabelReprint.GMPart> GetGMParts()
        {
            return new LabelReprint.DataHandler().GetGMParts();
        }

        [Route("LabelReprint/GetGMAddresses")]
        [HttpGet]
        public List<Printers.Address> GetGMAddresses()
        {
            return new LabelReprint.DataHandler().GetGMAddresses();
        }

        [Route("LabelReprint/PrintGMLabel")]
        [HttpPost]
        public LabelReprint.GMLabel PrintGMLabel([FromBody] LabelReprint.GMLabel label)
        {
            try
            {
                LabelReprint.DataHandler dh = new LabelReprint.DataHandler();
                dh.PrintGMRack(label);
                label.Error = false;
                label.Message = "Label Printed";

            }
            catch(Exception e)
            {
                label.Error = true;
                label.Message = e.Message;
            }

            return label;
        }

        #endregion
    }
}
