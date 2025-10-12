using NPOI.HSSF.UserModel; // 用于 .xls
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel; // 用于 .xlsx
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class ExcelUtility
{
    private IWorkbook workbook;
    private FileStream fileStream;

    public ExcelUtility(string filePath)
    {
        try
        {
            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            string extension = Path.GetExtension(filePath);

            // 根据文件扩展名创建不同的 Workbook
            if (extension == ".xlsx")
            {
                workbook = new XSSFWorkbook(fileStream);
            }
            else if (extension == ".xls")
            {
                workbook = new HSSFWorkbook(fileStream);
            }
            else
            {
                throw new Exception("不支持的Excel文件格式。仅支持 .xls 和 .xlsx。");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载Excel文件失败: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 将Excel工作表转换为CSV格式
    /// </summary>
    public void ConvertToCSV(string outputPath, Encoding encoding)
    {
        if (workbook == null)
        {
            Debug.LogError("Workbook 未初始化，无法转换。");
            return;
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(outputPath, false, encoding))
            {
                ISheet sheet = workbook.GetSheetAt(0); // 默认获取第一个工作表

                for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (row == null) continue;

                    System.Text.StringBuilder csvLine = new System.Text.StringBuilder();

                    for (int colIndex = 0; colIndex < row.LastCellNum; colIndex++)
                    {
                        ICell cell = row.GetCell(colIndex);
                        string cellValue = GetCellValue(cell);

                        // 处理CSV特殊字符：如果值包含逗号、换行或引号，则用引号括起来
                        if (cellValue.Contains(",") || cellValue.Contains("\"") || cellValue.Contains("\n"))
                        {
                            cellValue = "\"" + cellValue.Replace("\"", "\"\"") + "\"";
                        }

                        csvLine.Append(cellValue);
                        if (colIndex < row.LastCellNum - 1)
                        {
                            csvLine.Append(",");
                        }
                    }
                    writer.WriteLine(csvLine.ToString());
                }
            }
            Debug.Log($"CSV文件已成功导出: {outputPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"转换CSV失败: {e.Message}");
            throw;
        }
        finally
        {
            // 确保资源被释放
            workbook?.Close();
            fileStream?.Close();
        }
    }

    /// <summary>
    /// 获取单元格值的字符串表示
    /// </summary>
    private string GetCellValue(ICell cell)
    {
        if (cell == null)
        {
            return "";
        }

        switch (cell.CellType)
        {
            case CellType.String:
                return cell.StringCellValue;
            case CellType.Numeric:
                // 判断是否是日期格式
                if (DateUtil.IsCellDateFormatted(cell))
                {
                    return cell.DateCellValue.ToString();
                }
                else
                {
                    return cell.NumericCellValue.ToString();
                }
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            case CellType.Formula:
                // 对于公式，可以尝试获取计算后的值，或直接返回公式字符串
                // 这里返回计算后的值
                try
                {
                    IFormulaEvaluator evaluator = WorkbookFactory.CreateFormulaEvaluator(workbook);
                    CellValue evaluatedValue = evaluator.Evaluate(cell);
                    return evaluatedValue.FormatAsString();
                }
                catch
                {
                    return cell.CellFormula;
                }
            default:
                return "";
        }
    }

    // 以下两个方法根据你的需要实现或留空
    public void ConvertToJson(string outputPath, Encoding encoding)
    {
        // JSON转换实现（可根据需要扩展）
        throw new System.NotImplementedException("JSON转换功能尚未实现");
    }

    public void ConvertToXml(string outputPath)
    {
        // XML转换实现（可根据需要扩展）
        throw new System.NotImplementedException("XML转换功能尚未实现");
    }
}