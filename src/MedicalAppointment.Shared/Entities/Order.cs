namespace MedicalAppointment.Shared.Entities
{
    public class Order : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";           // Pending, Confirmed, Completed, Cancelled
        public string? CancellationReason { get; set; }           // Lý do hủy
        public string PaymentMethod { get; set; } = "Cash";       // Cash, VNPay, VietQR
        public string PaymentStatus { get; set; } = "Unpaid";     // Unpaid, Paid
        public string? TransactionId { get; set; } // Mã giao dịch VNPay

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
