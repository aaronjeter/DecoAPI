using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DecoAPI.Controllers
{
    public class EmailController : ApiController
    {
        [Route("Email/SendMail")]
        [HttpPost]
        public void SendMail([FromBody] Email.Email email)
        {
            email.SendMail();
        }
    }
}
