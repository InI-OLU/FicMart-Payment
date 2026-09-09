using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Domain.Enums
{
    public enum PaymentStatus
    {
        Authorized,
        Captured,
        Voided ,
        Refunded
    }
}
