using Dapper;
using Microsoft.Data.Sqlite;
using MiniExcelLibs;


public class Test
{
    private static void 创建测试数据的Excel()
    {
        var path = Path.GetTempPath() + Guid.NewGuid() + ".xlsx";
        SqliteConnection connection = Db.GetConnection();
        var results = connection.Query(@"SELECT * FROM AbnormalReport");
        MiniExcel.SaveAs(path, results);
        Console.WriteLine(path);
    }
}

