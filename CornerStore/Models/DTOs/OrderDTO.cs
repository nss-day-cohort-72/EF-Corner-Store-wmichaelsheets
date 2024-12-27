
namespace CornerStore.Models.DTOs;

public class OrderDTO
{
 public int Id { get; set; }
    
    public int CashierId { get; set; }
    public OrderProduct[] OrderProducts { get; set; }
    public decimal Total
    {
        get
        {
            return OrderProducts?.Sum(op => op.Quantity * op.Product.Price) ?? 0;
        }
    }
    public DateTime PaidOnDate { get; set; }
}