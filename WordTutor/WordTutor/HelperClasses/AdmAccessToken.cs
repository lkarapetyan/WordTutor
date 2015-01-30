using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordTutor.Helpers
{
    public class AdmAccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }

        public AdmAccessToken()
        {
            access_token = "";
            token_type = "";
            expires_in = "";
            scope = "";
        }
    }
}