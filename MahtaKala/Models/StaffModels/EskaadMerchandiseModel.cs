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
		public long Id_Mahta { get; set; }
		public long Id { get; set; }
		public string Code { get; set; }
		public string Code_Mahta { get; set; }
		public string Name { get; set; }		// This is the name of the product in Eskaad database
		public string Name_Mahta { get; set; }  // This is the title of the product as it appears in Mahta database
		public string Unit { get; set; }
		public double Count { get; set; }
		public int Quantity_Mahta { get; set; }
		public string Place { get; set; }
		public double Price { get; set; }
		public byte Active { get; set; }
		public ProductStatus Status_Mahta { get; set; }
		public bool IsPublished_Mahta { get; set; }
		public bool PresentInEskaad { get; set; } = false;
		public bool PresentInMahta { get; set; } = false;
		public List<string> ItemSituation { get; set; } = new List<string>();

		public string PriorityString { get; set; }

		public EskaadMerchandiseModel()
		{ }

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
				if (!Name.Trim().ToLower().Equals(Name_Mahta.Trim().ToLower()))
				// The name of the product should be identical in these two databases, but, it's not!
				// The reason, probably, is, the same code has been used by Eskaad for two different products!!
				{
					ItemSituation.Add(string.Format("نام محصول در دیتابیس مهتا: {0} - نام محصول در دیتابیس اسکاد: {1}",
						Name_Mahta, Name));
				}
			}
			
		}

		public void SetEskaadValues(Merchandise merchandiseItem)
		{
			this.Id = merchandiseItem.Id;
			this.Code = merchandiseItem.Code;
			this.Name = merchandiseItem.Name;
			this.Unit = merchandiseItem.Unit;
			this.Count = merchandiseItem.Count;
			this.Place = merchandiseItem.Place;
			this.Price = merchandiseItem.Price;
			this.Active = merchandiseItem.Active;
			this.PresentInEskaad = true;
		}

		public void SetMahtaValues(Product productItem)
		{
			this.Id_Mahta = productItem.Id;
			this.Code_Mahta = productItem.Code;
			this.Name_Mahta = productItem.Title;
			if (productItem.Quantities != null && productItem.Quantities.Count > 0)
				this.Quantity_Mahta = productItem.Quantities.First().Quantity;
			this.Status_Mahta = productItem.Status;
			this.IsPublished_Mahta = productItem.Published;
			this.PresentInMahta = true;
		}
	}
}
