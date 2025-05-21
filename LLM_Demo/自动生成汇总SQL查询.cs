using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using System.ComponentModel;


public class 自动生成汇总SQL查询
{
    static Kernel? _kernel;
    static string? _chatMessage;
    static OpenAIPromptExecutionSettings _setting = new()
    {
        Temperature = 0,
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };
    public async Task 执行()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: "qwen3:14b",
            endpoint: new Uri("http://localhost:11434/v1"),
            apiKey: null,
            httpClient: DemoLogger.GetHttpClient(true));

        kernelBuilder.Plugins.AddFromType<QCPlugin>();

        _kernel = kernelBuilder.Build();
        _chatMessage = @"请帮忙汇总2025年异常单最多的前几名异常类型跟次数，列出数据";
        var response = await _kernel.InvokePromptAsync(
            _chatMessage
        , new(_setting));
        Console.WriteLine(response);

    }

    public class QCPlugin
    {
        [KernelFunction("GetNGReport")]
        [Description("异常单汇总")]
        public async Task<string> GetNGReport()
        {
            {
                var chatMessage = $@"---
sqlite 数据结构如下:
CREATE TABLE IF NOT EXISTS AbnormalReport (
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
    solution TEXT                               -- 解决方案或备注
);
---
请按照查询内容生成只有查询 select sql 查询安全字串
请只输出 SQL 查询语句，不要任何解释。
---
互用查询内容: {_chatMessage}
";
                var response = await _kernel.InvokePromptAsync(
                    chatMessage);
                var sql = response.ToString().Split("</think>").LastOrDefault();
                Console.WriteLine("----生成的sql----");
                Console.WriteLine(sql);
                Console.WriteLine("\n\n--------");
                using SqliteConnection connection = Db.GetConnection();
                var results = connection.Query(sql);

                return JsonConvert.SerializeObject(results);
            }
        }
    }
}
