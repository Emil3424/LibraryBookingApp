using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBookingApp.DataBases
{
    public class User
    {
        public User() { }

        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string ReaderNumber { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }
}
