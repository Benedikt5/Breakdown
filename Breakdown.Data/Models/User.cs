using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Breakdown.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
