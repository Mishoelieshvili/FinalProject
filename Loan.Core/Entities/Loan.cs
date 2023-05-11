using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loan.Core.Entities
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public int Period { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
