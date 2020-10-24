using Aspose.Cells;
using EFSecondLevelCache.Core;
using MahtaKala.Entities;
using MahtaKala.GeneralServices.Exceptions;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Http.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices
{
    public class ImportService
    {
        private readonly DataContext db;

        public ImportService(DataContext db)
        {
            this.db = db;
        }

        static ImportService()
        {
            AsposeLicenser.SetLicense();
        }

        public ImportEnumerable GetExcelRows(Stream excelStream)
        {
            return new ImportEnumerable(excelStream);
        }

        string[] identifiers = new string[]
        {
                "شناسه",
                "کد",
                "کد محصول",
                "نام"
        };
        public async Task<int> ImportProductsInfo(Stream excelStream)
        {
            List<Tuple<string, Func<IQueryable<Product>, string, Task<Product>>>> finders = new List<Tuple<string, Func<IQueryable<Product>, string, Task<Product>>>>();
            finders.Add(new Tuple<string, Func<IQueryable<Product>, string, Task<Product>>>("شناسه", async (q, n) => await q.FirstOrDefaultAsync(a => a.Id == int.Parse(n))));
            finders.Add(new Tuple<string, Func<IQueryable<Product>, string, Task<Product>>>("کد", async (q, n) => await q.FirstOrDefaultAsync(a => a.Code == n)));
            finders.Add(new Tuple<string, Func<IQueryable<Product>, string, Task<Product>>>("کد محصول", async (q, n) => await q.FirstOrDefaultAsync(a => a.Code == n)));
            finders.Add(new Tuple<string, Func<IQueryable<Product>, string, Task<Product>>>("نام", async (q, n) => await q.FirstOrDefaultAsync(a => a.Title == n)));


            List<Tuple<string, Action<Product, object, int>>> setters = new List<Tuple<string, Action<Product, object, int>>>();
            setters.Add(new Tuple<string, Action<Product, object, int>>("قیمت", SetPrice));
            setters.Add(new Tuple<string, Action<Product, object, int>>("قیمت با تخفیف", SetDiscountPrice));
            setters.Add(new Tuple<string, Action<Product, object, int>>("وضعیت", SetStatus));
            setters.Add(new Tuple<string, Action<Product, object, int>>("موجودی", SetQuantity));

            HashSet<long> productIds = new HashSet<long>();

            int rowNum = 0;
            foreach (var row in GetExcelRows(excelStream))
            {
                rowNum++;
                //if (rowNum == 0)
                //{
                //    foreach (var item in row.Values)
                //    {
                //        if (!finders.Any(a => a.Item1 == item.ToString()) && !setters.Any(a => a.Item1 == item.ToString()) && !item.ToString().ToLower().StartsWith("ir"))
                //            throw new Exception($"ستون نامعتبر: {item}");
                //        columnNames.Add(item.ToString());
                //    }
                //    continue;
                //}

                var product = await GetProduct(row, row.columnNames, finders);
                if (product == null)
                    Throw(rowNum, ServiceMessages.Import.ProductCannotBeFound);

                if (productIds.Contains(product.Id))
                    Throw(rowNum, string.Format(ServiceMessages.Import.DuplicateProduct, product.Title));
                productIds.Add(product.Id);

                foreach (var setter in setters)
                {
                    if (row.columnNames.Contains(setter.Item1) && row[setter.Item1] != null)
                        setter.Item2(product, row[setter.Item1], rowNum);
                }

                foreach (var col in row.columnNames)
                {
                    if (col.ToLower().StartsWith("ir") && row[col] != null)
                    {
                        await SetShare(product, col, row[col], rowNum);
                    }
                }

            }
            await db.SaveChangesAsync();
            return rowNum;
        }

        async Task SetShare(Product p, string col, object val, int rowNum)
        {
            var paymentParties = await db.PaymentParties.Cacheable().ToListAsync();
            var pp = paymentParties.FirstOrDefault(a => a.ShabaId.ToLower() == col.ToLower());
            if (pp == null)
                Throw(rowNum, $"شماره ی شبا پیدا نشد: {col}");
            var ppp = p.PaymentParties.FirstOrDefault(a => a.PaymentPartyId == pp.Id);
            if (ppp == null)
            {
                ppp = new ProductPaymentParty();
                ppp.PaymentParty = pp;
                p.PaymentParties.Add(ppp);
            }
            if (!float.TryParse(val.ToString(), out var percent))
                Throw(rowNum, $"درصد نا معتبر است: {percent}");
            ppp.Percent = percent;
        }

        void SetStatus(Product p, object val, int rowNum)
        {
            var names = TranslateExtentions.GetTitles<ProductStatus>().ToDictionary(a => a.Value, a => a.Key);
            if (!names.TryGetValue(val.ToString(), out var st))
                Throw(rowNum, $"وضعیت نا معتبر: {val}");
            p.Status = st;
        }
        void SetPublished(Product p, object val, int rowNum)
        {
            bool pub = true;
            var s = val.ToString().ToLower();
            if (s == "بله" || s == "بلی" || s == "true" || s == "1")
                pub = true;
            else if (s == "false" || s == "0" || s == "خیر")
                pub = false;
            else
                Throw(rowNum, $"وضعیت انتشار اشتباه است: {s}");

            p.Published = pub;
        }
        void SetQuantity(Product p, object val, int rowNum)
        {
            if (!int.TryParse(val.ToString(), out var q))
                Throw(rowNum, ServiceMessages.Import.PriceIsEmpty);

            if (p.Quantities.Count == 0)
            {
                p.Quantities.Add(new ProductQuantity());
            }

            p.Quantities[0].Quantity = q;
            if (q == 0)
                p.Status = ProductStatus.NotAvailable;
        }

        void SetPrice(Product p, object val, int rowNum)
        {
            if (!decimal.TryParse(val.ToString(), out var price))
                Throw(rowNum, "قیمت نا معتبر است");

            if (p.Prices.Count == 0)
            {
                p.Prices.Add(new ProductPrice());
            }

            p.Prices[0].Price = price;
        }

        void SetDiscountPrice(Product p, object val, int rowNum)
        {
            if (!decimal.TryParse(val.ToString(), out var price))
                Throw(rowNum, "قیمت  با تخفبف نا معتبر است");

            if (p.Prices.Count == 0)
            {
                p.Prices.Add(new ProductPrice());
            }

            p.Prices[0].DiscountPrice = price;
        }


        private async Task<Product> GetProduct(ImportRow row, List<string> columnNames, List<Tuple<string, Func<IQueryable<Product>, string, Task<Product>>>> finders)
        {
            var q = db.Products.Include(a => a.Prices).Include(a => a.PaymentParties).Include(a => a.Quantities);
            foreach (var item in finders)
            {
                if (columnNames.Contains(item.Item1) && row[item.Item1] != null)
                {
                    return await item.Item2(q, row[item.Item1].ToString());
                }
            }
            return null;
        }

        public async Task<int> ImportProductCountAndPrices(Stream excelStream)
        {
            int i = 1;
            HashSet<long> productIds = new HashSet<long>();
            foreach (var row in GetExcelRows(excelStream))
            {
                try
                {
                    var name = row[1].ToString();
                    var product = await db.Products.Include(a => a.Prices).FirstOrDefaultAsync(a => a.Title == name);
                    if (product == null)
                        Throw(i, string.Format(ServiceMessages.Import.ProductCannotBeFound, name));
                    if (productIds.Contains(product.Id))
                        Throw(i, string.Format(ServiceMessages.Import.DuplicateProduct, name));
                    productIds.Add(product.Id);

                    var count = 0;
                    if (row[2] != null)
                        count = int.Parse(row[2].ToString());
                    decimal price = 0;
                    if (row[3] == null || !decimal.TryParse(row[3].ToString(), out price))
                        Throw(i, ServiceMessages.Import.PriceIsEmpty);
                    var discountedPrice = price;
                    if (row[4] != null)
                    {
                        discountedPrice = decimal.Parse(row[4].ToString());
                    }

                    if (product.Prices.Count == 0)
                    {
                        product.Prices.Add(new ProductPrice());
                    }

                    product.Prices[0].Price = price;
                    product.Prices[0].DiscountPrice = discountedPrice;
                }
                catch (Exception ex) when (!(ex is ImportException))
                {
                    Throw(i, ex);
                }
                i++;
            }
            await db.SaveChangesAsync();
            return i - 1;
        }
        void Throw(int row, string msg)
        {
            throw new ImportException(string.Format(ServiceMessages.Import.ImportingRow, row) + msg);
        }
        void Throw(int row, Exception ex)
        {
            throw new Exception(string.Format(ServiceMessages.Import.ImportingRow, row), ex);
        }


        #region Nested types
        public class ImportEnumerable : IEnumerable<ImportRow>
        {
            private readonly Stream excelStream;

            internal ImportEnumerable(Stream excelStream)
            {
                this.excelStream = excelStream;
            }
            public IEnumerator<ImportRow> GetEnumerator()
            {
                return new ImportEnumerator(excelStream);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        public class ImportEnumerator : IEnumerator<ImportRow>
        {
            internal ImportEnumerator(Stream excelStream)
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                book = new Aspose.Cells.Workbook(excelStream);
                sheet = book.Worksheets[0];
                foreach (Cell cell in sheet.Cells.Rows[0])
                    ColumnNames.Add(cell.StringValue);
                rowIndex = 0;
            }

            int rowIndex;
            Workbook book;
            Worksheet sheet;
            ImportRow current;
            public List<string> ColumnNames { get; } = new List<string>();
            public ImportRow Current => current;

            object IEnumerator.Current => current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                rowIndex++;
                if (sheet.Cells.Rows.Count <= rowIndex)
                    return false;
                current = new ImportRow((Row)sheet.Cells.Rows[rowIndex], ColumnNames);
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        public class ImportRow
        {
            internal ImportRow(Row row, List<string> names)
            {
                columnNames = names;
                List<object> l = new List<object>();
                for (var i = 0; i < names.Count; i++)
                {
                    l.Add(row[i].Value);
                }
                Values = l.ToArray();
            }
            public List<string> columnNames { get; private set; }
            public object[] Values { get; private set; }
            public object this[int i]
            {
                get
                {
                    return Values[i];
                }
            }

            public object this[string column]
            {
                get
                {
                    var i = columnNames.IndexOf(column);
                    if (i == -1)
                        throw new IndexOutOfRangeException($"Column can not be found: {column}");
                    return Values[i];
                }
            }
        }
        #endregion Nested types
    }
}
