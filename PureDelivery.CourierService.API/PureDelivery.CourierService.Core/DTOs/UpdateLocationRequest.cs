using System.ComponentModel.DataAnnotations;

namespace PureDelivery.CourierService.Core.DTOs
{
    public class UpdateLocationRequest
    {
        [Required, Range(-90, 90)]
        public double Latitude  { get; set; }

        [Required, Range(-180, 180)]
        public double Longitude { get; set; }

        /// <summary>true = курьер онлайн и готов брать заказы.</summary>
        public bool IsOnline { get; set; } = true;
    }
}
