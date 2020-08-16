﻿using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class UserAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public long CityId { get; set; }
        public City City { get; set; }
        [StringLength(10)]
        public string PostalCode { get; set; }
        public string Details { get; set; }
        public Point Location { get; set; }
    }
}