using System.ComponentModel.DataAnnotations.Schema;

namespace MicroRabbit.Banking.Domain.Models
{
    //Representa una cuenta bancaria
    public class Account
    {
        public int Id { get; set; }
        public string AccountType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal AccountBalance { get; set; }
    }
}
