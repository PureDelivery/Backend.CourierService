namespace PureDelivery.CourierService.Core.Models
{
    public class CourierRating
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CourierId         { get; set; }
        public Guid OrderId           { get; set; }
        public Guid RatedByCustomerId { get; set; }

        /// <summary>1–5</summary>
        public int    Score     { get; set; }
        public string Comment   { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Courier? Courier { get; set; }
    }
}
