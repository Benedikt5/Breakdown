using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Breakdown.Data.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        public string OuterId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public void DecreaseAmount(decimal amt)
        {
            Amount -= amt;
            if (Amount < 0)
                Amount = 0;
        }
    }
}
