using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public long CategoryId { get; set; }
        public ProductCategory Category { get; set; }
        public long BrandId { get; set; }
        public Brand Brand { get; set; }
        public IList<Characteristic> Characteristics { get; set; }
        public IList<string> ImageList { get; set; }
    }



    public class CharacteristicValue
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    //public enum CharacteristicType
    //{
    //    String,
    //    Number,
    //    Date,
    //    Image,
    //}

    public class Characteristic
    {
        public string Name { get; set; }
        //public CharacteristicType Type { get; set; }
        public List<string> Values { get; set; }
    }
}
