using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using System.ComponentModel;


public class 异常单基本查询加定义
{
    public async Task 执行()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: "qwen3:32b",
            endpoint: new Uri("http://localhost:11434/v1"),
            apiKey: null,
            httpClient: DemoLogger.GetHttpClient(true)
            );

        kernelBuilder.Plugins.AddFromType<QCPlugin>();

        var kernel = kernelBuilder.Build();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            Temperature=0,
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
        [Description("获取指定时间异常单的数据")]
        [return: Description("异常单数据")]
        public string GetNGReport([Description("产品号")] string product_no
            , [Description("起始时间(可为空)")] DateTime? sDate
            , [Description("结束时间(可为空)")] DateTime? eDate)
        {
            if (sDate == null)
                sDate = DateTime.Now.AddMonths(-1);
            if (eDate == null)
                eDate = DateTime.Now;
            using SqliteConnection connection = Db.GetConnection();
            var results = connection.Query(@"SELECT title,product_no,occur_time,location,type,solution
FROM AbnormalReport 
WHERE product_no like @product_no and occur_time BETWEEN @sDate AND @eDate"
                , new { product_no, sDate, eDate });

            return $@"---
栏位定义:
id INTEGER PRIMARY KEY AUTOINCREMENT,       -- 异常单ID（主键）
title TEXT NOT NULL,                        -- 异常标题
product_no TEXT NOT NULL,                   -- 产品编号
description TEXT,                           -- 异常描述
reporter TEXT NOT NULL,                     -- 报告人
occur_time DATETIME NOT NULL,               -- 异常发生时间
location TEXT,                              -- 发生地点/设备/模块
type TEXT,                                  -- 异常类型（如：系统故障、人为操作、网络中断等）
level TEXT CHECK(level IN ('Low', 'Medium', 'High', 'Critical')) NOT NULL DEFAULT 'Medium', -- 紧急程度
status TEXT CHECK(status IN ('Open', 'Processing', 'Closed', 'Pending')) DEFAULT 'Open', -- 处理状态
assignee TEXT,                              -- 分配处理人
resolve_time DATETIME,                      -- 解决时间
ai_solution TEXT ,                          -- AI生成的解决方案
solution TEXT                               -- 解决方案或备注

---
异常单数据
{JsonConvert.SerializeObject(results)}";
        }
    }
}

