using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    [Display(Name = "آدرس")]
    public class UserAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        [Display(Name = "کد شهر")]
        public long CityId { get; set; }
        [Display(Name = "کد شهر")]
        public City City { get; set; }
        [StringLength(10)]
        public string PostalCode { get; set; }
        public string Details { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public bool Disabled { get; set; }
    }
}
