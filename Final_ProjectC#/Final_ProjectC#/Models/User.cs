using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_ProjectCS.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public List<string> PurchaseHistory { get; set; } = new List<string>();
        public Dictionary<string, int> Cart { get; set; } = new Dictionary<string, int>();
    }
}


