using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
	public class EskaadOrdersToBePlaced
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }
		public string EskaadCode { get; set; }
		public int Quantity { get; set; }
		public DateTime CreatedDate { get; set; }
		public long CreatedById { get; set; }
		public User CreatedBy { get; set; }
	}
}
