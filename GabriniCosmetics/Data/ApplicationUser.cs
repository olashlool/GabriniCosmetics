﻿using Microsoft.AspNetCore.Identity;

namespace GabriniCosmetics.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

    }
}
