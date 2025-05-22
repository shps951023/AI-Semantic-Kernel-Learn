using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using System.ComponentModel;


public class 异常单基本查询
{
    public async Task 执行()
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: "qwen3:14b",
            endpoint: new Uri("http://localhost:11434/v1"),
            apiKey: null);

        kernelBuilder.Plugins.AddFromType<QCPlugin>();

        var kernel = kernelBuilder.Build();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            Temperature = 0,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        Console.WriteLine("Loading...");
        var response = await kernel.InvokePromptAsync(
@"请帮忙列出 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况
需要特别注意的异常类型跟避免建议"
        , new(openAIPromptExecutionSettings));
        Console.WriteLine(response);

    }

    public class QCPlugin
    {
        [KernelFunction("GetNGReport")]
        [Description("获取指定时间异常单的数据")]
        [return: Description("异常单明细")]
        public string GetNGReport([Description("产品号")] string product_no
            , [Description("起始时间(可为空)")] DateTime? sDate
            , [Description("结束时间(可为空)")] DateTime? eDate)
        {
            if (sDate == null)
                sDate = DateTime.Now.AddMonths(-1);
            if (eDate == null)
                eDate = DateTime.Now;
            using SqliteConnection connection = Db.GetConnection();
            var results = connection.Query(@"SELECT title,product_no,location,type
FROM AbnormalReport 
WHERE product_no like @product_no and occur_time BETWEEN @sDate AND @eDate"
                , new { product_no, sDate, eDate });

            return JsonConvert.SerializeObject(results);
        }
    }
}

/*
 <think>
好的，用户让我列出AXA-200产品在2025年1月1日到2025年5月20日的异常单情况。首先，我需要确认用户的需求是什么。他们可能是在生产部门，需要了解这段时间内产品的异常情况，以便进行质量控制或设备维护。用户可能没有明确说明，但深层需求可能是找出主要问题点，或者统计异常类型和频率，以优化生产流程。

接下来，我需要检查提供的工具函数是否适用。工具函数QCPlugin-GetNGReport需要产品号、起始时间和结束时间。用户已经提供了产品号AXA-200，以及日期范围，所以参数都满足。不过，工具函数的响应示例显示返回的是一个异常单列表，包含标题、产品号、位置和类 型等信息。现在需要将这些数据整理成用户友好的格式。

然后，我需要处理返回的数据。注意到返回的数据中有重复的条目，比如“装配线停机”出现了两次，可能需要检查是否是数据重复还是不同的事件。不过用户可能只需要列表，所以暂时保留所有条目，但可以指出重复的情况。

接下来，将数据分类汇总，比如按异常类型统计，这样用户能更清楚问题分布。例如，设备故障出现次数较多，可能需要优先处理。同时，列出每个异常的具体位置和类型，帮助用户定位问题源头。

最后，确保回答清晰，分点列出，并给出总结，指出主要问题类型，方便用户快速理解。同时，询问是否需要进一步分析，以提供更深入的帮助。
</think>

以下是AXA-200产品在2025-01-01至2025-05-20期间的异常单汇总分析：

一、异常事件清单
1. 设备故障类（7项）
- 装配线停机（2次）：装配车间A1线
- 设备报警：注塑车间M7机
- 模具卡料：冲压车间M2线
- 传感器失灵：自动化线E5
- 电机过热：传送带T3
- 液压压力低：液压站H9
- PC烧板：电子组装线

2. 人为失误类（4项）
- 物料错投：配料区B3线
- 漏打螺丝：终检区X1产品
- 参数设置错误：注塑车间控制台
- 标签打印错误：包装区打印机

3. 品质问题类（2项）
- 焊接不良：焊接车间C2线
- 测试失败：质检中心F3批次

4. 其他技术问题
- 贴片偏移：电子组装线SMT（设备偏差）
- 电源中断：配电房主线路（电力故障）
- 程序错误：机器人作业区R3（软件故障）
- 包装破损：仓储物流区（运输问题）
- 涂层脱落：喷涂车间（工艺问题）
- 静电击穿：装配台ESD区域（环境问题）
- 缺料报警：SMT贴片机（物料短缺）

二、关键问题分析
1. 设备故障占比最高（35%），重点问题：
- 装配线停机重复发生
- 多处设备报警（注塑/自动化/传送带）

2. 人为失误占比20%，需加强：
- 参数设置规范培训
- 包装区打印流程管控

3. 工艺问题（涂层脱落）和环境问题（静电击穿）需特别关注。

是否需要进一步生成异常类型统计图表或建议改善方案？
 */