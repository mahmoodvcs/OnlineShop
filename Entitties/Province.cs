using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahtaKala.Entities
{
    public partial class Province
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        [Required]
        [StringLength(255)]
        [Display(Name ="نام استان")]
        public string Name { get; set; }

        public IList<City> Cities { get; set; }
        //public static Province[] GetAll()
        //{
        //    using (var db = new DbEntities())
        //        return db.Provinces.Cacheable().ToArray();
        //}
    }
}
