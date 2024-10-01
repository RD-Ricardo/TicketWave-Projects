using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessPayment.Events
{
    public class TicketPaidEvent
    {
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public DateTime PaidAt { get; set; }
        public string Status { get; set; }
    }
}
