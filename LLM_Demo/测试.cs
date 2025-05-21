using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.ComponentModel;


public class 测试
{
    public async Task 执行()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: "qwen3:8b",
            endpoint: new Uri("http://localhost:11434/v1"),
            apiKey: null);

        kernelBuilder.Plugins.AddFromType<QCPlugin>();

        var kernel = kernelBuilder.Build();

        await ChatHelper.Chat(kernel, @"请帮忙汇总 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况
            需要特别注意的前几种异常类型跟避免建议");
    }

    public class QCPlugin
    {
        [KernelFunction("GetNGReport")]
        [Description("获取指定时间异常单的数据")]
        public string GetNGReport([Description("产品号")] string product_no
            , [Description("起始时间(可为空)")] DateTime? sDate
            , [Description("结束时间(可为空)")] DateTime? eDate)
        {
            if (sDate == null)
                sDate = DateTime.Now.AddMonths(-1);
            if (eDate == null)
                eDate = DateTime.Now;
            using SqliteConnection connection = Db.GetConnection();
            var results = connection.Query(@"SELECT * FROM AbnormalReport 
WHERE product_no like @product_no and occur_time BETWEEN @sDate AND @eDate"
                , new { product_no, sDate, eDate });

            return JsonConvert.SerializeObject(results);
        }
    }
}