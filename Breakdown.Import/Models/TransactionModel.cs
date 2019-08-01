using System;
using System.Collections.Generic;
using System.Text;

namespace Breakdown.Import.Models
{
    public class TransactionModel
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal Local_Amount { get; set; }
        public string Local_Currency { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }
    }
}
