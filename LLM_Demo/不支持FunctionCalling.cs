﻿using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using System.ComponentModel;


public class 不支持FunctionCalling
{
    public async Task 执行()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: "deepseek-r1:1.5b",
            endpoint: new Uri("http://localhost:11434/v1"),
            apiKey: null,
            httpClient: DemoLogger.GetHttpClient(true));

        kernelBuilder.Plugins.AddFromType<QCPlugin>();

        var kernel = kernelBuilder.Build();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        var response = await kernel.InvokePromptAsync(
@"请帮忙汇总 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况
需要特别注意的前几种异常类型跟避免建议"
        , new(openAIPromptExecutionSettings));
        Console.WriteLine(response);

    }

    public class QCPlugin
    {
        [KernelFunction("GetNGReport")]
        [Description("获取异常单的数据")]
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
