using MahtaKala.Entities;
using MahtaKala.Entities.Models;
using MahtaKala.SharedServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MahtaKala.GeneralServices
{
    public class ExportService
    {
        static ExportService()
        {
            AsposeLicenser.SetLicense();
        }

        public static Stream ExportQuery<T>(List<string> columns, IQueryable query)
        {
            MemoryStream ms = new MemoryStream();
            ExportQuery<T>(ms, columns, query);
            return ms;
        }
        public static void ExportQuery<T>(Stream output, List<string> columns, IQueryable query)
        {
            //var stream = output;
            //if (!output.CanSeek)
            //{
            //    MemoryStream ms = new MemoryStream();
            //    stream =ms;
            //}

            Aspose.Cells.Workbook book = new Aspose.Cells.Workbook();
            if (!book.Worksheets.Any())
                book.Worksheets.Add();
            var sheet = book.Worksheets[0];

            var columnNames = GetColumnNames<T>(columns);

            int col = 0;
            foreach (var n in columnNames)
            {
                sheet.Cells[0, col].Value = n;
                col++;
            }

            var t = typeof(T);
            int row = 1;
            var props = t.GetProperties().Where(p => columns.Contains(p.Name)).ToDictionary(p => p.Name);
            foreach (var item in query)
            {
                col = 0;
                foreach (var n in columns)
                {
                    sheet.Cells[row, col].Value = props[n].GetValue(item);
                    col++;
                }
                row++;
            }

            book.Save(output, Aspose.Cells.SaveFormat.Xlsx);
        }

        private static List<string> GetColumnNames<T>(List<string> columns)
        {
            var t = typeof(T);
            List<string> names = new List<string>();
            foreach (var c in columns)
            {
                var p = t.GetProperty(c);
                names.Add(TranslateExtentions.GetTitle(p));
            }
            return names;
        }
    }
}
