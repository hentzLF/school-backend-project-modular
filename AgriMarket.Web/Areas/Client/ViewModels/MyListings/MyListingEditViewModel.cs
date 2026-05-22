using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AgriMarket.Web.Areas.Client.ViewModels.MyListings
{
    internal class MyListingEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Category")]
        public Guid ServiceCategoryId { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price per hectare must be greater than 0.")]
        [Display(Name = "Price Per Hectare")]
        public decimal PricePerHectare { get; set; }

        public bool IsActive { get; set; }
    }
}