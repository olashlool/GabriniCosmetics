﻿using System.ComponentModel.DataAnnotations;

namespace GabriniCosmetics.Areas.Admin.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<CartDetail> CartDetails { get; set; }

    }
}
