using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DecoAPI.Controllers
{
    public class DDCServiceController : ApiController
    {
        [Route("DDCService/test")]
        [HttpGet]
        public bool Test()
        {
            return true;
        }        

        /// <summary>
        /// Method to print VW Service Unit Label from mold barcode scan
        /// VW Box Labels are the same label format, with a different quantity and Label Stock, so they go through this method as well
        /// </summary>
        /// <param name="message">A Message class object, with values String Printer, String MoldLabel, String Error, int Quantity, bool Success</param>
        [Route("DDCService/VWUnitLabel")]
        [HttpPost]
        public DDCService.Message VWUnitLabel([FromBody] DDCService.Message message)
        {
            message.Success = true;
            message.Notification = "";

            try
            {
                DDCService.VWServiceUnitLabel vwLabel = new DDCService.VWServiceUnitLabel(message);
            }
            catch (Exception e)
            {
                message.Success = false;
                message.Notification = e.Message.ToString();
                return message;
            }
            
            return message;
        }

        /// <summary>
        /// takes a message object with partlabel and printer
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Route("DDCService/VWChromeStrip")]
        [HttpPost]
        public DDCService.Message VWChromeStrip([FromBody] DDCService.Message message)
        {
            try
            {
                DDCService.VWChromeStrip vwChrome = new DDCService.VWChromeStrip(message);
                message.Success = true;
                message.Notification = "Label Printed";
            }
            catch (Exception e)
            {
                message.Success = false;
                message.Notification = e.Message;
            }

            return message;
        }
    }
}
