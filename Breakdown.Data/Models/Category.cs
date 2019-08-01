using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Breakdown.Data.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        [Required]
        public string Name { get; set; }

        public Category Parent { get; set; }
        public ICollection<Category> Children { get; set; }
    }
}
