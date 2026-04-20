using System.ComponentModel.DataAnnotations;

namespace PureDelivery.CourierService.Core.DTOs
{
    public class SubmitRatingRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid RatedByCustomerId { get; set; }

        [Required, Range(1, 5)]
        public int Score { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; } = string.Empty;
    }
}
