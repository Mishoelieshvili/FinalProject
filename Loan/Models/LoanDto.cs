namespace Loan.Models
{
    public class LoanDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public int Period { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; }
    }
}
