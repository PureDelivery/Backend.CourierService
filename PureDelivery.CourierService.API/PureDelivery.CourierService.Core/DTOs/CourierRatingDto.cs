namespace PureDelivery.CourierService.Core.DTOs
{
    public class CourierRatingDto
    {
        public Guid     Id                { get; set; }
        public Guid     CourierId         { get; set; }
        public Guid     OrderId           { get; set; }
        public Guid     RatedByCustomerId { get; set; }
        public int      Score             { get; set; }
        public string   Comment           { get; set; } = string.Empty;
        public DateTime CreatedAt         { get; set; }
    }
}
