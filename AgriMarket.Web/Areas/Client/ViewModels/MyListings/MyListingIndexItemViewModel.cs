using System;

namespace AgriMarket.Web.Areas.Client.ViewModels.MyListings
{
    internal class MyListingIndexItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string CategoryName { get; set; } = default!;
        public decimal PricePerHectare { get; set; }
        public bool IsActive { get; set; }
    }
}