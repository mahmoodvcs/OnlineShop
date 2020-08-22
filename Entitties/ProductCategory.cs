using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace MahtaKala.Entities
{
    public class ProductCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [StringLength(255)]
        public string Title { get; set; }
        public string Image { get; set; }
        public long? ParentId { get; set; }
        [JsonIgnore]
        public ProductCategory Parent { get; set; }
    }
}
