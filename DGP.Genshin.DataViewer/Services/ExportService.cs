using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.IO;

namespace DGP.Genshin.DataViewer.Services
{
    public class ExportService
    {
        public static void SaveDataTableToExcel(DataTable table,string fileName,string sheetName)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet(sheetName);
            if (table != null)
            {
                //header
                IRow header = sheet.CreateRow(0);
                if (table.Columns.Count >= 1)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        DataColumn column = table.Columns[i];
                        ICell cell = header.CreateCell(i);
                        cell.SetCellValue(column.ColumnName);
                    }
                }
                //content
                if (table.Rows.Count >= 1)
                {
                    for (int j = 1; j < table.Rows.Count; j++)
                    {
                        DataRow dataRow = table.Rows[j];
                        IRow currentRow = sheet.CreateRow(j);
                        for (int k = 0; k < table.Columns.Count; k++)
                        {
                            ICell cell = currentRow.CreateCell(k);
                            cell.SetCellValue(dataRow[k].ToString());
                        }
                    }
                }
            }
            using (FileStream fileStream = File.Create(fileName))
            {
                workbook.Write(fileStream);
            }
        }
    }
}
