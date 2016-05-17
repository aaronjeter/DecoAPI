using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DecoAPI.Controllers
{
    public class PartLookupController : ApiController
    {
        /// <summary>
        /// test call
        /// </summary>
        /// <returns></returns>
        [Route("PartLookup/Test")]
        [HttpGet]
        public bool Test()
        {
            return true;
        }

        /// <summary>
        /// Determine Single/Double scan status for part
        /// </summary>
        /// <param name="paintLabel"></param>
        /// <returns></returns>
        [Route("PartLookup/GetScanType/{paintLabel}")]
        [HttpGet]
        public string GetScanType(string paintLabel)
        {
            return Unload.RackCreator.GetScanType(paintLabel);
        }

        [Route("PartLookup/GetPaintLabel/{paintlabel}")]
        [HttpGet]
        public PartLookup.PaintLabel GetPaintLabel(string paintLabel)
        {
            return new PartLookup.PaintLabel(paintLabel);
        }

        [Route("PartLookup/GetStyle/{stylecode}")]
        [HttpGet]
        public PartLookup.Style GetStyle(int stylecode)
        {
            return new PartLookup.Style(stylecode);
        }

        [Route("PartLookup/GetColor/{plantcolor}")]
        [HttpGet]
        public PartLookup.Color GetColor(int plantcolor)
        {
            return new PartLookup.Color(plantcolor);
        }
    }
}
