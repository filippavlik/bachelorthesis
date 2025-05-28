﻿using System.ComponentModel.DataAnnotations;

namespace AdminPartDevelop.DTOs
{
    public class RefereeAddRequest : IRefereeDto
    {
        public string? FacrId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Surname is required")]
        public string Surname { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "League is required")]
        [Range(0, 10, ErrorMessage = "League must be between 0 and 10")]
        public int League { get; set; }

        [Required(ErrorMessage = "Age is required")]
        public int Age { get; set; }

        public bool Ofs { get; set; }
        public string? Place { get; set; }

        public string? Note { get; set; }       

        public bool CarAvailability { get; set; }
    }
}
