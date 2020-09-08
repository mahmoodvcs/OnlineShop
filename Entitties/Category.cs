using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace MahtaKala.Entities
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [StringLength(255)]
        [Display(Name ="نام")]
        [Required(ErrorMessage = "نام را وارد کنید")]
        public string Title { get; set; }
        public string Image { get; set; }
        [Display(Name ="دسته ی والد")]
        public long? ParentId { get; set; }
        [JsonIgnore]
        [Display(Name ="دسته ی والد")]
        public Category Parent { get; set; }
        public IList<Category> Children { get; set; }
        public IList<ProductCategory> ProductCategories { get; set; }
        [Display(Name ="غیر فعال")]
        public bool Disabled { get; set; }
        [Display(Name ="منتشر شده")]
        public bool Published { get; set; }
        [Display(Name ="ترتیب")]
        public int Order { get; set; }
    }
}
