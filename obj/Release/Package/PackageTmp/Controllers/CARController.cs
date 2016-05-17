using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DecoAPI.Controllers
{
    public class CARController : ApiController
    {
        /// <summary>
        /// Method to validate username for currently not implemented auto-login feature
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [Route("CAR/ValidateUser")]
        [HttpPost]
        public CAR.User ValidateUser([FromBody] string username)
        {
            return new CAR.User(username);
        }

        [Route("CAR/SendCarAlerts")]
        [HttpGet]
        public bool SendCarAlerts()
        {
            new CAR.CarAlert().SendCARAlerts();
            return true;
        }

        /// <summary>
        /// Method to get CAR object by id number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("CAR/GetCar/{id}")]
        [HttpGet]
        public CAR.CAR GetCar(string id)
        {
            int carId = Int32.Parse(id);
            return new CAR.CAR(carId);
        }

        /// <summary>
        /// Method to create Corrective action request in database from CAR object
        /// </summary>
        /// <param name="car"></param>
        [Route("CAR/CreateCar")]
        [HttpPost]
        public void CreateCar([FromBody] CAR.CAR car)
        {
            CAR.CAR.CreateCar(car); //Does this line have enough cars in it?
        }

    }
}
