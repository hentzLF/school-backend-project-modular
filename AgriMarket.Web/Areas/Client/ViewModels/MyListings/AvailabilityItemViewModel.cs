using System;

namespace AgriMarket.Web.Areas.Client.ViewModels.MyListings
{
    internal class AvailabilityItemViewModel
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsBooked { get; set; }
    }
}