using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahtaKala.Entities
{
    public partial class City
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name ="نام استان")]
        public long ProvinceId { get; set; }

        [Display(Name = "نام استان")]
        public Province Province { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name ="نام شهر")]
        public string Name { get; set; }
        public bool IsCenter { get; set; }

        //public static City[] GetAll(int provinceId)
        //{
        //    return GetAll().Where(c => c.ProvinceId == provinceId).ToArray();
        //}
        //public static City[] GetAll()
        //{
        //    using (var db = new DataContext())
        //        return db.Cities.Cacheable().ToArray();
        //}
    }
}
