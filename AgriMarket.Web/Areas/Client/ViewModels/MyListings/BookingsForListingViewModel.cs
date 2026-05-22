using System;
using System.Collections.Generic;

namespace AgriMarket.Web.Areas.Client.ViewModels.MyListings
{
    internal class BookingsForListingViewModel
    {
        public Guid ListingId { get; set; }
        public string ListingTitle { get; set; } = default!;
        public IEnumerable<BookingsForListingItemViewModel> Bookings { get; set; } = new List<BookingsForListingItemViewModel>();
    }
}