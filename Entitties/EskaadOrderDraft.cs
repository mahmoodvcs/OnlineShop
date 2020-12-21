using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
	public class EskaadOrderDraft
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }
		public string EskaadCode { get; set; }
		public int Quantity { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CreatedDatePersian { get; set; }
		public long CreatedById { get; set; }
		public User CreatedBy { get; set; }
		public long? UpdatedById { get; set; }
		public User UpdatedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public bool OrderIsSealed { get; set; } = false;
	}
}
