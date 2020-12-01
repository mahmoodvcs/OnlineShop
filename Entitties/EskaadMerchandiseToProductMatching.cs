using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
	public class EskaadMerchandiseToProductMatching
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }
		public long ProductId { get; set; }
		public string Code_Eskaad { get; set; }
		public string Code_Mahta { get; set; }
		public MatchingMethod MatchingMethod { get; set; }
		public DateTime CreatedDate { get; set; }
	}

	public enum MatchingMethod
	{
		[Description("تساوی کدها")]
		CodesAreIdentical = 1,
		[Description("شباهت نام​ها")]
		NamesAreSimilar = 2,
		[Description("طبق نظر اپراتور")]
		DeterminedByOperator = 3,
		[Description("نامشخص")]
		NotSpecified = 0
	}

}
