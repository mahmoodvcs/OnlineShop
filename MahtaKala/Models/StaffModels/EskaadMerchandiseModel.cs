using MahtaKala.Entities;
using MahtaKala.Entities.EskaadEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.StaffModels
{
	public class EskaadMerchandiseModel
	{
		public long ProductId_Mahta { get; set; }
		public long Id_Eskaad { get; set; }
		public string Code_Eskaad { get; set; }
		public string Code_Mahta { get; set; }
		public string Name_Eskaad { get; set; }		// This is the name of the product in Eskaad database
		public string Name_Mahta { get; set; }  // This is the title of the product as it appears in Mahta database
		public string Unit_Eskaad { get; set; }
		public double Count_Eskaad { get; set; }
		public int Quantity_Mahta { get; set; }
		public int YellowWarningThreshold_Mahta { get; set; }
		public int RedWarningThreshold_Mahta { get; set; }
		public string Place_Eskaad { get; set; }
		public double Price_Eskaad { get; set; }
		public byte Active_Eskaad { get; set; }
		public ProductStatus Status_Mahta { get; set; }
		public bool IsPublished_Mahta { get; set; }
		public bool PresentInEskaad { get; set; } = false;
		public bool PresentInMahta { get; set; } = false;
		public List<string> ItemSituation { get; set; } = new List<string>();

		public string PriorityString { get; set; }

		public string Validation_Eskaad { get; set; }
		public byte? Tax_Eskaad { get; set; }

		public EskaadMerchandiseModel()
		{ }

		public EskaadMerchandiseModel(EskaadMerchandise eskaadMerchandiseItem)
		{
			this.ProductId_Mahta = eskaadMerchandiseItem.ProductId_Mahta;
			this.Id_Eskaad = eskaadMerchandiseItem.Id_Eskaad;
			this.Code_Mahta = eskaadMerchandiseItem.Code_Mahta;
			this.Code_Eskaad = eskaadMerchandiseItem.Code_Eskaad;
			this.Name_Mahta = eskaadMerchandiseItem.Name_Mahta;
			this.Name_Eskaad = eskaadMerchandiseItem.Name_Eskaad;
			this.Unit_Eskaad = eskaadMerchandiseItem.Unit_Eskaad;
			this.Count_Eskaad = eskaadMerchandiseItem.Count_Eskaad;
			this.Quantity_Mahta = eskaadMerchandiseItem.Quantity_Mahta;
			this.YellowWarningThreshold_Mahta = eskaadMerchandiseItem.YellowWarningThreshold_Mahta;
			this.RedWarningThreshold_Mahta = eskaadMerchandiseItem.RedWarningThreshold_Mahta;
			this.Place_Eskaad = eskaadMerchandiseItem.Place_Eskaad;
			this.Price_Eskaad = eskaadMerchandiseItem.Price_Eskaad;
			this.Active_Eskaad = eskaadMerchandiseItem.Active_Eskaad;
			this.Status_Mahta = eskaadMerchandiseItem.Status_Mahta;
			this.IsPublished_Mahta = eskaadMerchandiseItem.IsPublished_Mahta;
			this.PresentInMahta = eskaadMerchandiseItem.PresentInMahta;
			this.PresentInEskaad = eskaadMerchandiseItem.PresentInEskaad;
			this.Validation_Eskaad = eskaadMerchandiseItem.Validation_Eskaad;
			this.Tax_Eskaad = eskaadMerchandiseItem.Tax_Eskaad;
		}
		public EskaadMerchandiseModel(Merchandise merchandiseItem)
		{
			SetEskaadValues(merchandiseItem);
		}

		public EskaadMerchandiseModel(Product productItem)
		{
			SetMahtaValues(productItem);
		}

		public EskaadMerchandiseModel(Merchandise merchandiseItem, Product productItem)
		{
			SetEskaadValues(merchandiseItem);
			SetMahtaValues(productItem);
		}

		public void SetItemProirity()
		{
			if (PresentInEskaad && !PresentInMahta)
			{
				PriorityString = "100";
			}
			else if (!PresentInEskaad && PresentInMahta)
			{
				PriorityString = "200";
			}
			else
			{
				PriorityString = "300";
			}
			
		}

		public void SetItemSituation()
		{
			if (string.IsNullOrWhiteSpace(PriorityString))
				return;
			if (ItemSituation == null)
			{ 
				// THIS CAN NOT BE! But, apparently, it IS be!! :S
			}
			ItemSituation.Clear();
			if (PriorityString.Equals("100"))
			{
				ItemSituation.Add("آیتم جدید! محصولی متناظر با این آیتم در دیتابیس مهتا موجود نیست!");
			}
			else if (PriorityString.Equals("200"))
			{
				ItemSituation.Add($"این محصول از دیتابیس اسکاد حذف شده است! کد محصول: {Code_Mahta}");
			}
			else if (PriorityString.Equals("300"))
			{
				if (!Name_Eskaad.Trim().ToLower().Equals(Name_Mahta.Trim().ToLower()))
				// The name of the product should be identical in these two databases, but, it's not!
				// The reason, probably, is, the same code has been used by Eskaad for two different products!!
				{
					ItemSituation.Add($"نام محصول در دو دیتابیس یکسان نیست؛ دیتابیس مهتا: {Name_Mahta} - دیتابیس اسکاد: {Name_Eskaad}");
				}
			}
		}

		public void SetEskaadValues(Merchandise merchandiseItem)
		{
			if (merchandiseItem == null)
				return;
			this.Id_Eskaad = merchandiseItem.Id;
			this.Code_Eskaad = merchandiseItem.Code.Trim();
			this.Name_Eskaad = merchandiseItem.Name.Trim();
			this.Unit_Eskaad = merchandiseItem.Unit;
			this.Count_Eskaad = merchandiseItem.Count;
			this.Place_Eskaad = merchandiseItem.Place;
			this.Price_Eskaad = merchandiseItem.Price;
			this.Active_Eskaad = merchandiseItem.Active;
			this.Tax_Eskaad = merchandiseItem.Tax;
			this.Validation_Eskaad = merchandiseItem.Validation;
			this.PresentInEskaad = true;
		}

		public void SetMahtaValues(Product productItem)
		{
			if (productItem == null)
				return;
			this.ProductId_Mahta = productItem.Id;
			this.Code_Mahta = productItem.Code;
			this.Name_Mahta = productItem.Title;
			if (productItem.Quantities != null && productItem.Quantities.Count > 0)
				this.Quantity_Mahta = productItem.Quantities.First().Quantity;
			this.YellowWarningThreshold_Mahta = productItem.YellowWarningThreshold;
			this.RedWarningThreshold_Mahta = productItem.RedWarningThreshold;
			this.Status_Mahta = productItem.Status;
			this.IsPublished_Mahta = productItem.Published;
			this.PresentInMahta = true;
		}
	}
}
