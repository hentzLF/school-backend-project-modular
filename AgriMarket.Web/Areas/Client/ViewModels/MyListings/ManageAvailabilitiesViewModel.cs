using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Web.Areas.Client.ViewModels.MyListings
{
    internal class ManageAvailabilitiesViewModel
    {
        public Guid ListingId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;

        public List<AvailabilityItemViewModel> Availabilities { get; set; } = new();

        [Required]
        [Display(Name = "Start Time")]
        public DateTime AddStartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime AddEndTime { get; set; }
    }
}