﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSystem.Events
{
    /// <summary>
    /// The event arguments for the login action.
    /// </summary>
    public class LoginEventArgs : MainEventArgs
    {
        private bool success;

        public LoginEventArgs(bool success) : base("login")
        {
            this.success = success;
        }

        /// <summary>
        /// Determines if the login was successfull or not.
        /// </summary>
        public bool Success
        {
            get { return success; }
            set { success = value; }
        }
    }
}