using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Domain.DTOs
{
    public class BankCards
    {
        public string CardNumber { get; set; }
        public string Cvv { get; set; }
        public int ExpiryDateMonth { get; set; }
        public int ExpiryDateYear { get; set; }
    }
}
