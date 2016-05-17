using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DecoAPI.Models;
using DecoAPI.Unload;

namespace DecoAPI.Controllers
{
    public class UnloadController : ApiController
    {
        /// <summary>
        /// Method to open partial rack from Rack or paint label scan. Throws exception if no rack opened
        /// </summary>
        /// <param name="scan"></param>
        /// <returns></returns>
        [Route("Unload/OpenPartial")]
        [HttpPost]
        public Unload.Rack OpenPartial([FromBody] string scan)
        {
            Unload.Rack unloadRack = new Unload.Rack();

            try
            {
                AbstractRack rack = RackFactory.OpenPartial(scan);
                sPart part = new Part().GetPartByPartNumber(rack.PartNumber);

                unloadRack.RackID = "" + rack.RackID;
                unloadRack.PartNumber = rack.PartNumber;
                unloadRack.PartDescription = part.Description2;
                unloadRack.Parts = rack.GetPaintLabels();
            }
            catch (Exception e)
            {
                unloadRack.Error = true;
                unloadRack.Message = e.Message;
            }            
            
            return unloadRack;
        }

        [Route("Unload/GetMoldPartNumber")]
        [HttpPost]
        public Unload.Session GetMoldPartNumber([FromBody] string moldLabel)
        {
            Unload.Session session = new Session();
            RackCreator rc = new RackCreator(session);

            try
            {
                session.MoldPart = RackCreator.GetMoldPartNumber(moldLabel);
            }
            catch (Exception e)
            {
                session.ClearPart();
                session.Error = true;
                session.Message = e.Message;
            }            

            return session;
        }

        [Route("Unload/GetScanType/{paintLabel}")]
        [HttpGet]
        public string GetScanType(string paintLabel)
        {
            return Unload.RackCreator.GetScanType(paintLabel);
        }

        [Route("Unload/UpdateHoldStatus/{paintLabel}")]
        [HttpGet]
        public bool UpdateHoldStatus(string paintLabel)
        {
            return Unload.RackCreator.UpdateHoldStatus(paintLabel);
        }

        [Route("Unload/GetPartNumber")]
        [HttpPost]
        public string GetPartNumber([FromBody] string paintLabel, string moldPartNumber)
        {
            string returnValue = "";

            return returnValue;
        }

        /// <summary>
        /// Method to determine the single/double scan status of a label
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [Route("Unload/Process")]
        [HttpPost]
        public Unload.Session Process([FromBody] Unload.Session session)
        {   
            try
            {
                RackCreator rc = new RackCreator(session);
                rc.ProcessSession();
            }
            catch (Exception e)
            {
                session.ClearPart();
                session.Error = true;
                session.Message = e.Message;
            }

            session.Rack = null;

            return session;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [Route("Unload/PassLabel")]
        [HttpPost]
        public Unload.Session PassLabel([FromBody] Unload.Session session)
        {
            try
            {
                RackCreator rc = new RackCreator(session);
                rc.ProcessAdditionalPart();
            }
            catch (Exception e)
            {
                session.ClearPart();
                session.Error = true;
                session.Message = e.Message;
            }
            
            return session;
        }

        [Route("Unload/PrintRack")]
        [HttpPost]
        public Unload.Session PrintRack([FromBody] Unload.Session session)
        {
            try
            {
                RackCreator rc = new RackCreator(session);
                rc.PrintRack();
            }
            catch (Exception e)
            {
                session.Error = true;
                session.Message = e.Message;
            }
            
            session.Rack = null;

            return session;
        }        
    }
}
