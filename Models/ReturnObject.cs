using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DecoAPI.Models
{
    public class ReturnObject
    {
        bool success;
        string message;

        public ReturnObject(bool success, string message)
        {
            this.success = success;
            this.message = message;
        }

        public bool Success
        {
            get { return success; }
            set { success = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}