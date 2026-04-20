namespace PureDelivery.CourierService.Core.DTOs
{
    public class PagedRatingsDto
    {
        public List<CourierRatingDto> Items      { get; set; } = [];
        public int                   TotalCount  { get; set; }
        public int                   Page        { get; set; }
        public int                   PageSize    { get; set; }
        public double                AverageScore { get; set; }
    }
}
