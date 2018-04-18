using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmissionsApiInteraction
{
    /// <summary>
    /// Represents the JSON structure of the result returned from https://api.watttime.org/api/v2/login/ 
    /// </summary>
    public class LoginResult
    {
        public string Token { get; set; }

        /// <summary>
        /// Create instance of LoginResult
        /// </summary>
        /// <param name="token"></param>
        public LoginResult(string token)
        {
            this.Token = token;
        }
    }
}
