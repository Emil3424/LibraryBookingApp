using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBookingApp.DataBases
{
    public class Reservation
    {
        public Reservation() { }
        public int ID { get; set; }
        public string ReservationDate { get; set; }
        public string Date { get; set; }
        public string ReservationStatus {get;set;}
        public int UserId { get; set; }
        public int BookId { get; set; }
    }
}
