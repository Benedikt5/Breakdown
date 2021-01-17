using CsvHelper.Configuration.Attributes;
using System;

namespace Breakdown.Import.Models
{
    public class TransactionModel
    {
        [Name("Transaction ID")]
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }
        [Name("Local amount")]
        public decimal Local_Amount { get; set; }
        [Name("Local currency")]
        public string Local_Currency { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        [Name("Notes and #tags")]
        public string Notes { get; set; }

        public DateTime Created =>
            new DateTime(Date.Year, Date.Month, Date.Day, Time.Hour, Time.Minute, Time.Second);
    }
}
