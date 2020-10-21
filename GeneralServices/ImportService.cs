using Aspose.Cells;
using MahtaKala.Entities;
using MahtaKala.GeneralServices.Exceptions;
using Microsoft.EntityFrameworkCore;
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
            string LData = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxMaWNlbnNlPg0KICAgIDxEYXRhPg0KICAgICAgICA8TGljZW5zZWRUbz5pckRldmVsb3BlcnMuY29tPC9MaWNlbnNlZFRvPg0KICAgICAgICA8RW1haWxUbz5pbmZvQGlyRGV2ZWxvcGVycy5jb208L0VtYWlsVG8+DQogICAgICAgIDxMaWNlbnNlVHlwZT5EZXZlbG9wZXIgT0VNPC9MaWNlbnNlVHlwZT4NCiAgICAgICAgPExpY2Vuc2VOb3RlPkxpbWl0ZWQgdG8gMTAwMCBkZXZlbG9wZXIsIHVubGltaXRlZCBwaHlzaWNhbCBsb2NhdGlvbnM8L0xpY2Vuc2VOb3RlPg0KICAgICAgICA8T3JkZXJJRD43ODQzMzY0Nzc4NTwvT3JkZXJJRD4NCiAgICAgICAgPFVzZXJJRD4xMTk0NDkyNDM3OTwvVXNlcklEPg0KICAgICAgICA8T0VNPlRoaXMgaXMgYSByZWRpc3RyaWJ1dGFibGUgbGljZW5zZTwvT0VNPg0KICAgICAgICA8UHJvZHVjdHM+DQogICAgICAgICAgICA8UHJvZHVjdD5Bc3Bvc2UuVG90YWwgUHJvZHVjdCBGYW1pbHk8L1Byb2R1Y3Q+DQogICAgICAgIDwvUHJvZHVjdHM+DQogICAgICAgIDxFZGl0aW9uVHlwZT5FbnRlcnByaXNlPC9FZGl0aW9uVHlwZT4NCiAgICAgICAgPFNlcmlhbE51bWJlcj57RjJCOTcwNDUtMUIyOS00QjNGLUJENTMtNjAxRUZGQTE1QUE5fTwvU2VyaWFsTnVtYmVyPg0KICAgICAgICA8U3Vic2NyaXB0aW9uRXhwaXJ5PjIwOTkxMjMxPC9TdWJzY3JpcHRpb25FeHBpcnk+DQogICAgICAgIDxMaWNlbnNlVmVyc2lvbj4zLjA8L0xpY2Vuc2VWZXJzaW9uPg0KICAgIDwvRGF0YT4NCiAgICA8U2lnbmF0dXJlPlFYTndiM05sTGxSdmRHRnNMb1B5YjJSMVkzUWdSbUZ0YVd4NTwvU2lnbmF0dXJlPg0KPC9MaWNlbnNlPg==";

            using (Stream stream = new MemoryStream(Convert.FromBase64String(LData)))
            {
                //Ex:
                stream.Seek(0, SeekOrigin.Begin);
                new Aspose.Cells.License().SetLicense(stream);
            }

        }

        public ImportEnumerable GetExcelRows(Stream excelStream)
        {
            return new ImportEnumerable(excelStream);
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
                    decimal priceCoef = 1;
                    bool useInputPriceCoef = false;
                    if (row[5] != null && decimal.TryParse(row[5].ToString(), out priceCoef) && priceCoef > 0)
                    {
                        useInputPriceCoef = true;
                    }                        


                    if(product.Prices.Count == 0)
                    {
                        product.Prices.Add(new ProductPrice());
                    }
                    
                    product.Prices[0].Price = price;
                    product.Prices[0].DiscountPrice = discountedPrice;
					product.Prices[0].PriceCoefficient = useInputPriceCoef ? priceCoef : 1;
					productIds.Add(product.Id);
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
                book = new Aspose.Cells.Workbook(excelStream);
                sheet = book.Worksheets[0];
                foreach (Cell cell in sheet.Cells.Rows[0])
                    columnNames.Add(cell.StringValue);
                rowIndex = 0;
            }

            int rowIndex;
            Workbook book;
            Worksheet sheet;
            ImportRow current;
            List<string> columnNames = new List<string>();
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
                current = new ImportRow((Row)sheet.Cells.Rows[rowIndex], columnNames);
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
                for(var i=0;i<names.Count;i++)
                {
                    l.Add(row[i].Value);
                }
                Values = l.ToArray();
            }
            List<string> columnNames;
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
