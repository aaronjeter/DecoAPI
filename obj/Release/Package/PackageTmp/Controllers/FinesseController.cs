using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DecoAPI.Employees;

namespace DecoAPI.Controllers
{
    public class FinesseController : ApiController
    {
        /// <summary>
        /// Method to validate Employee Badge Number
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        [Route("Finesse/ValidateUser")]
        [HttpPost]
        public bool ValidateUser([FromBody] string badge)
        {
            Employee e = new Employee();

            if (e.ValidateEmployee(badge))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to get all available Finesse defect categories
        /// </summary>
        /// <returns></returns>
        [Route("Finesse/GetDefectCategories")]
        [HttpGet]
        public List<Finesse.DefectCategory> GetDefectCategories()
        {
            return Finesse.Finesse.GetDefectCategories();
        }

        /// <summary>
        /// Method to get all defect types for a specifies defect category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [Route("Finesse/GetDefectTypes/{category}")]
        [HttpGet]
        public List<Finesse.DefectType> GetDefectTypes([FromUri] int category)
        {
            return Finesse.Finesse.GetDefectTypes(category);
        }


    }
}
