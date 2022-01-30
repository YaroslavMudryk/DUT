﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DUT.Domain.Models
{
    public class BaseModel
    {
        [Required]
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedFromIP { get; set; }

        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedFromIP { get; set; }
    }
    public class BaseModel<T> : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public T Id { get; set; }
    }

    public class BaseModelWithoutIdentity<T> : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public T Id { get; set; }
    }
}