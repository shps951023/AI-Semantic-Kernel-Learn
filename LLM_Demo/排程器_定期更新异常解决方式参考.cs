using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.Sqlite;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MiniExcelLibs;

public class 排程器_定期更新异常解决方式参考
{
    public async Task 执行()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: "deepseek-r1:1.5b",
            endpoint: new Uri("http://localhost:11434/v1"),
            apiKey: null,
            httpClient: DemoLogger.GetHttpClient(true)
            );
        var kernel = kernelBuilder.Build();
        var memory = new MemoryWebClient("http://127.0.0.1:9001/");
        var memoryPlugin = kernel.ImportPluginFromObject(new MemoryPlugin(memory), "kernel_memory");

        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0,
        };

        using SqliteConnection connection = Db.GetConnection();
        var list = connection.Query(@"SELECT * FROM AbnormalReport where ai_solution is null and solution is null");
        foreach (var e in list.Take(10))
        {
            var chatMessage = $@"请问如何解决 : 产品编号:{e.product_no} 异常描述:{e.description}";
            Console.WriteLine("----问题----");
            Console.WriteLine(chatMessage);
            var response = await kernel.InvokePromptAsync(
                $@"<message role=""system"">
请协助员工到 kernel_memory 查询相关资讯, 根据查询结果为基础回复使用者的问题
优先级 : 同产品编号 > 异常描述 > 其他
请简短 400 字以内回复，不要任何解释。
</message>
<message role=""user"">
{chatMessage}
</message>"

            , new(settings));
            Console.WriteLine(response);
            connection.Execute(
                "update AbnormalReport set ai_solution = @ai_solution where id = @id"
                , new { ai_solution= response.ToString().Split("</think>").LastOrDefault(), id =e.id });
        }
        var path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xlsx";
        MiniExcel.SaveAs(path, connection.Query("select * from AbnormalReport where ai_solution is not null and solution is null"));
        Console.WriteLine($"生成excel文件 : {path}");
    }
}

