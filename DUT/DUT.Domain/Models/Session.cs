﻿using System.ComponentModel.DataAnnotations;

namespace DUT.Domain.Models
{
    public class Session : BaseModel<int>
    {
        [Required, StringLength(150, MinimumLength = 1)]
        public AppModel App { get; set; }
        public DeviceInfo Device { get; set; }
        public Location Location { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public int DeactivatedBySessionId { get; set; }
        [StringLength(5000, MinimumLength = 5)]
        public string Token { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }

    public class AppModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}