using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sabio.Models.Requests.Locations
{
    public class LocationAddRequest
    { 
        [Required]
        [Range(1, int.MaxValue)]
        public int LocationTypeId { get; set; }

        [Required]
        [StringLength(225, MinimumLength = 2)]
        public string LineOne { get; set; }

        [StringLength(255, MinimumLength = 2)]
        public string LineTwo { get; set; }

        [Required]
        [StringLength(225, MinimumLength = 2)]
        public string City { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string Zip { get; set; }

        [Required]
        [Range(1, 51)]
        public int StateId { get; set; }

        [Required]
        [Range(-90.00000, 90.00000)]
        public double Latitude { get; set; }
        [Required]
        [Range(-179.99999, 180.00000)]
        public double Longitude { get; set; }
      

    }
}

