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
        public DateTime ExpiryDate { get; set; }
        public long Balance { get; set; }
    }
}
