using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using System.ComponentModel;


public class 自动生成SQL查询
{
    static Kernel? _kernel;
    static string? _chatMessage;
    static OpenAIPromptExecutionSettings _setting = new()
    {
        Temperature = 0,
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
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
        _chatMessage = @"请帮忙汇总 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况
需要特别注意的前几种异常类型跟避免建议";
        var response = await _kernel.InvokePromptAsync(
            _chatMessage
        , new(_setting));
        Console.WriteLine(response);

    }

    public class QCPlugin
    {
        [KernelFunction("GetNGReport")]
        [Description("获取指定时间异常单的数据")]
        [return: Description("异常单明细")]
        public async Task<string> GetNGReport([Description("产品号")] string product_no
            , [Description("起始时间(可为空)")] DateTime? sDate
            , [Description("结束时间(可为空)")] DateTime? eDate)
        {
            if (sDate == null)
                sDate = DateTime.Now.AddMonths(-1);
            if (eDate == null)
                eDate = DateTime.Now;

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
请按照查询内容生成只有查询 sqlite select sql 查询安全字串
请只输出 SQL 查询语句，不要任何解释。
---
用户查询内容: {_chatMessage}
";
                var response = await _kernel.InvokePromptAsync(
                    chatMessage);
                var sql = response.ToString().Split("</think>").LastOrDefault();
                Console.WriteLine("----生成的sql----");
                Console.WriteLine(sql);
                using SqliteConnection connection = Db.GetConnection();
                var results = connection.Query(sql);

                return JsonConvert.SerializeObject(results);
            }
        }
    }
}

/*
 
 ----请求----
{
  "temperature": 0,
  "tools": [
    {
      "function": {
        "description": "获取指定时间异常单的数据",
        "name": "QCPlugin-GetNGReport",
        "strict": false,
        "parameters": {
          "type": "object",
          "required": [
            "product_no",
            "sDate",
            "eDate"
          ],
          "properties": {
            "product_no": {
              "description": "产品号",
              "type": "string"
            },
            "sDate": {
              "description": "起始时间(可为空)",
              "type": "string"
            },
            "eDate": {
              "description": "结束时间(可为空)",
              "type": "string"
            }
          }
        }
      },
      "type": "function"
    }
  ],
  "messages": [
    {
      "role": "user",
      "content": "请帮忙汇总 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况\r\n需要特别注意的前几种异常类型跟避免建 议"
    }
  ],
  "model": "qwen3:14b",
  "tool_choice": "auto"
}

----响应----
{
  "id": "chatcmpl-76",
  "object": "chat.completion",
  "created": 1747826606,
  "model": "qwen3:14b",
  "system_fingerprint": "fp_ollama",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "",
        "tool_calls": [
          {
            "id": "call_pciyau93",
            "index": 0,
            "type": "function",
            "function": {
              "name": "QCPlugin-GetNGReport",
              "arguments": "{\"eDate\":\"2025-05-20\",\"product_no\":\"AXA-200\",\"sDate\":\"2025-01-01\"}"
            }
          }
        ]
      },
      "finish_reason": "tool_calls"
    }
  ],
  "usage": {
    "prompt_tokens": 237,
    "completion_tokens": 294,
    "total_tokens": 531
  }
}

----请求----
{
  "messages": [
    {
      "role": "user",
      "content": "---\r\nsqlite 数据结构如下:\r\nCREATE TABLE IF NOT EXISTS AbnormalReport (\r\n    id INTEGER PRIMARY KEY AUTOINCREMENT,       -- 异常单ID（主键）\r\n    title TEXT NOT NULL,                        -- 异常标题\r\n    product_no TEXT NOT NULL,                   -- 产品编号\r\n    description TEXT,                           -- 异常描述\r\n    reporter TEXT NOT NULL,                     -- 报告人\r\n    occur_time DATETIME NOT NULL,               -- 异常发生时间\r\n    location TEXT,                              -- 发生地点/设备/模块\r\n    type TEXT,                                  -- 异常类型（如：系统故障、人为操作、网络中断等）\r\n    level TEXT CHECK(level IN ('Low', 'Medium', 'High', 'Critical')) NOT NULL DEFAULT 'Medium', -- 紧急程度\r\n    status TEXT CHECK(status IN ('Open', 'Processing', 'Closed', 'Pending')) DEFAULT 'Open', -- 处理状态\r\n    assignee TEXT,                              -- 分配处理人\r\n    resolve_time DATETIME,                      -- 解决时间\r\n    solution TEXT                               -- 解决方案或备注\r\n);\r\n---\r\n请按照查询内容生成只有查询 sqlite select sql 查询安全字串\r\n请只输出 SQL 查询语句，不要任何解释。\r\n---\r\n用户查询内容: 请帮忙汇总 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况\r\n需要特别注意的前几种异常类型跟避免建议\r\n"
    }
  ],
  "model": "qwen3:14b"
}

----响应----
{
  "id": "chatcmpl-270",
  "object": "chat.completion",
  "created": 1747826635,
  "model": "qwen3:14b",
  "system_fingerprint": "fp_ollama",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "<think>\n好的，我现在需要处理用户的查询，生成对应的SQLite SELECT语句。用户的要求是汇总AXA-200产品在2025年1月1日到2025年5月20日之间的异常单情况，并且特别注意前几种异常类型和避免建议。首先，我需要理解用户的需求。\n\n首先，用户提到的产品编号是AXA-200，所以WHERE子句中product_no应该等于这个值。时间范围是occur_time在2025-01-01到2025-05-20之间，需要使用BETWEEN关键字或者>=和<=，不过BETWEEN更简洁。接下来，用户要汇总异常单的情况，可能需要统计不同类型的数量，或者列出所有记录？不过用户后面还提到要特别注意前几种异常类型和避免建议，可能需要按类型分组并统计数量，同时可能需要获取解决方案或建议的信息。\n\n不过，用户并没有明确说明是否需要统计数量，只是说“汇总异常单情况”。因此可能需要列出所有相关的记录，但同时需要特别注意异常类型。这时候可能需要查询所有符合条件的记录，并按照类型排序，或者统计每个类型的数量。但用户可能希望看到具体的记录，以及类型分布。不过用户最后提到“需要特别注意的前几种异常类型跟避免建议”，可能需要先按类型分组，统计数量，然后取前几种类型，同时获取解决方案或建议的信息。\n\n但根据用户给出的表结构，solution字段是解决方案或备注，可能包含避免建议。因此，可能需要查询所有符合条件的记录，并按照type分组，统计每个类型的数量，然后可能取前几个。不过用户可能需要同时看到具体的记录和统计信息，但根据问题描述，可能只需要统计类型分布，以及对应的建议。\n\n不过，用户可能只是希望查询出这些异常单的信息，然后在结果中注意类型及其对应的建议。因此，可能只需要查询所有符合条件的记录，并按类型排序，或者按发生时间排序。但用户提到“需要特别注意的前几种异常类型”，可能需要先统计每个类型的数量，再按数量从高到低排序，取前几种。不过，SQLite的语法中，如何实现这样的分组和排序？\n\n如果用户想先统计类型数量，然后排序，可能需要使用GROUP BY和ORDER BY，但用户可能希望同时获取具体的记录。这里可能需要进一步明确用户意图。但根据用户给出的查询内容，可能更倾向于列出所有符合条件的异常单，并按类型分类，可能同时统计数量，但用户没有明确说明是否要统计。\n\n考虑到用户可能需要的是具体的异常单数据，而“汇总异常单情况”可能是指列出所有记录，所以最初的思路是生成一个SELECT语句，筛选出product_no为AXA-200且occur_time在指定时间范围内的记录，并 可能按类型排序。但需要特别注意的是类型，可能需要使用ORDER BY type，或者按数量排序？不过，如果只是列出所有记录，可能不需 要分组统计。因此，可能用户需要的是查询所有符合条件的记录，而“特别注意前几种异常类型”可能是在分析结果时的建议，但查询语句本身可能不需要处理这部分。\n\n所以最终的查询可能只是SELECT * FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20'。但用户可能需要更详细的分析，比如统计每个类型的数量，这时候可能需要使用GROUP BY和COUNT，同时可能需要ORDER BY COUNT(*) DESC来得到最常出现的类型。不过用户是否需要这样的统计？问题中提到“需要特别注 意的前几种异常类型跟避免建议”，可能意味着用户需要统计每个类型的数量，并按数量排序，以便识别需要关注的类型，同时获取对应的解决方案（solution）作为建议。因此，可能需要两个部分：统计类型数量，以及获取每个类型对应的解决方案。但如何在一个查询中处理？\n\n不过，可能用户只是希望查询出所有符合条件的记录，并在这些记录中注意类型及其解决方案。因此，可能直接查询所有记录，并按类型排序，或者按解决时间排序等。不过用户没有明确排序条件，所以可能只需要查询所有符合条件的记录，并按类型分组，或者不排序。\n\n现在再仔细看用户的问题：“请帮忙汇总 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况”，这里的“汇总”可 能是指统计所有异常单的数量、类型分布、处理状态等，但用户可能只是希望列出所有相关的异常单记录。而“需要特别注意的前几种异常类型跟避免建议”可能意味着在查询结果中突出显示这些类型，或者在查询中筛选出这些类型。但用户没有说明具体的类型，所以可能需要先统计类型数量，然后在结果中显示前几个类型及对应的建议。\n\n由于用户要求生成的是SELECT语句，而不是聚合查询，可能需要先查询所有符合条件的记录，然后在应用层处理类型统计和排序。但用户可能希望直接在查询中进行统计，这时候可能需要使用GROUP BY，但用户可能需要的是所有记录，而不是统计结果。\n\n此时可能需要根据常见用法来推断，用户可能希望列出所有相关的异常单，并按异常类型排序，或者按发生时间排序。或者，用户可能需要将查询结果按异常类型分组，统计数量，并可能按数量降序排列。例如：\n\nSELECT type, COUNT(*) AS count FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20' GROUP BY type ORDER BY count DESC;\n\n但用户可能需要更详细的信息，如每个类型的具体案例，而不仅仅是统计数量 。因此，可能还是需要查询所有记录，并按类型排序，或者包括类型和solution字段，以便后续分析。\n\n但用户明确说“需要特别注意的前几种异常类型跟避免建议”，可能希望得到每个类型对应的解决方案，同时统计数量。这个时候可能需要同时进行分组统计和获取解决方案，但可能需要两个不同的查询。\n\n不过，根据用户的要求，可能只需要生成SELECT语句，而不需要处理统计。所以回到最初的结论，生成一个查询筛选出符合条件的记录，并包括所有字段，可能按类型排序：\n\nSELECT * FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20' ORDER BY type;\n\n但用户是否需要排序？用户没有明确说明 ，所以可能不需要。因此，最简单的查询可能是：\n\nSELECT * FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20';\n\n不过用户提到需要特别注意前几种异常类型，可能需要查询中包含类型信息，并且可能 按发生次数排序，但如果没有统计，可能无法做到这一点。因此，或许用户希望先统计类型数量，然后查询具体的记录。但用户要求的是生成查询语句，所以可能只能生成查询所有记录的语句，然后在应用程序中分析类型统计。\n\n因此，正确的SQL语句应该是：\n\nSELECT * FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20';\n\n但用户可能需要更具体的查询，如包括type和solution字段，并可能按type排序。或者，可能用户希望得到每个类型的汇总信息。不过，根据用户给出的示例中的“避免建议”，可能Solution字段中包含建议，所以查询中需要包括该字段。\n\n因此，正确的查询可能包含所有字段，或至少包括type和solution，所以：\n\nSELECT id, title, product_no, description, reporter, occur_time, location, type, level, status, assignee, resolve_time, solution FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20';\n\n但用户可能希望不使用SELECT *，而是明确列出字段。不过用户没有说明，所以使用*也可以。\n\n最终，根据用户要求，生成正确的SELECT语句，筛选条件正确，而无需额外处理。因此，正确的SQL是：\n\nSELECT * FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20';\n</think>\n\nSELECT * FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20';"
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 328,
    "completion_tokens": 1922,
    "total_tokens": 2250
  }
}

----生成的sql----


SELECT * FROM AbnormalReport WHERE product_no = 'AXA-200' AND occur_time BETWEEN '2025-01-01' AND '2025-05-20';
----请求----
{
  "temperature": 0,
  "tools": [
    {
      "function": {
        "description": "获取指定时间异常单的数据",
        "name": "QCPlugin-GetNGReport",
        "strict": false,
        "parameters": {
          "type": "object",
          "required": [
            "product_no",
            "sDate",
            "eDate"
          ],
          "properties": {
            "product_no": {
              "description": "产品号",
              "type": "string"
            },
            "sDate": {
              "description": "起始时间(可为空)",
              "type": "string"
            },
            "eDate": {
              "description": "结束时间(可为空)",
              "type": "string"
            }
          }
        }
      },
      "type": "function"
    }
  ],
  "messages": [
    {
      "role": "user",
      "content": "请帮忙汇总 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况\r\n需要特别注意的前几种异常类型跟避免建 议"
    },
    {
      "role": "assistant",
      "content": "",
      "tool_calls": [
        {
          "id": "call_pciyau93",
          "function": {
            "name": "QCPlugin-GetNGReport",
            "arguments": "{\"eDate\":\"2025-05-20\",\"product_no\":\"AXA-200\",\"sDate\":\"2025-01-01\"}"
          },
          "type": "function",
          "index": 0
        }
      ]
    },
    {
      "role": "tool",
      "tool_call_id": "call_pciyau93",
      "content": "[{\"id\":1,\"title\":\"装配线停机\",\"product_no\":\"AXA-200\",\"description\":\"A1线因气压不足导致自 动停机\",\"reporter\":\"张强\",\"occur_time\":\"2025-04-01 08:45:00\",\"location\":\"装配车间A1线\",\"type\":\"设备故障\",\"level\":\"High\",\"status\":\"Closed\",\"assignee\":\"李工\",\"resolve_time\":\"2025-04-01 10:30:00\",\"ai_solution\":null,\"solution\":\"更换气泵并恢复供气\"},{\"id\":2,\"title\":\"物料错投\",\"product_no\":\"AXA-200\",\"description\":\"B3线操作员误投型号为XYZ-2的物料\",\"reporter\":\"王芳\",\"occur_time\":\"2025-04-01 11:20:00\",\"location\":\"配料区B3线\",\"type\":\"人为失误\",\"level\":\"Medium\",\"status\":\"Closed\",\"assignee\":\"陈主管\",\"resolve_time\":\"2025-04-01 12:00:00\",\"ai_solution\":null,\"solution\":\"隔离批次并重新投料\"},{\"id\":3,\"title\":\"焊接不良\",\"product_no\":\"AXA-200\",\"description\":\"C2线第5号焊接点存在虚焊现象\",\"reporter\":\"赵磊\",\"occur_time\":\"2025-04-02 09:15:00\",\"location\":\"焊接车间C2线\",\"type\":\"品质问题\",\"level\":\"High\",\"status\":\"Processing\",\"assignee\":\"刘质检\",\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":4,\"title\":\"贴片偏移\",\"product_no\":\"AXA-200\",\"description\":\"SMT线贴片位置整体偏移0.3mm\",\"reporter\":\"周敏\",\"occur_time\":\"2025-04-02 14:10:00\",\"location\":\"电子组装线SMT\",\"type\":\"设备偏差\",\"level\":\"Critical\",\"status\":\"Open\",\"assignee\":null,\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":5,\"title\":\"设备报警\",\"product_no\":\"AXA-200\",\"description\":\"注塑机M7出现温度超限报警\",\"reporter\":\"吴勇\",\"occur_time\":\"2025-04-03 10:50:00\",\"location\":\"注塑车间M7机\",\"type\":\"设备故障\",\"level\":\"Medium\",\"status\":\"Closed\",\"assignee\":\"维修部黄工\",\"resolve_time\":\"2025-04-03 11:30:00\",\"ai_solution\":null,\"solution\":\"清理冷却系统并复位\"},{\"id\":6,\"title\":\"模具卡料\",\"product_no\":\"AXA-200\",\"description\":\"冲压线M2模具卡住未排出产品\",\"reporter\":\"郑浩\",\"occur_time\":\"2025-04-03 16:00:00\",\"location\":\"冲压车间M2线\",\"type\":\"设备故障\",\"level\":\"High\",\"status\":\"Processing\",\"assignee\":\"模 具组林工\",\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":7,\"title\":\"测试失败\",\"product_no\":\"AXA-200\",\"description\":\"成品测试F3批次电流超标\",\"reporter\":\"徐婷\",\"occur_time\":\"2025-04-04 09:40:00\",\"location\":\"质检中心F3批次\",\"type\":\"品质问题\",\"level\":\"Medium\",\"status\":\"Closed\",\"assignee\":\"品控杨工\",\"resolve_time\":\"2025-04-04 11:00:00\",\"ai_solution\":null,\"solution\":\"返修处理并重新测试\"},{\"id\":8,\"title\":\"传感器失灵\",\"product_no\":\"AXA-200\",\"description\":\"流水线E5光电传感器无响应\",\"reporter\":\"胡军\",\"occur_time\":\"2025-04-04 13:30:00\",\"location\":\"自动化线E5\",\"type\":\"设备故障\",\"level\":\"Medium\",\"status\":\"Processing\",\"assignee\":\"自控组唐工\",\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":9,\"title\":\"电源中断\",\"product_no\":\"AXA-200\",\"description\":\"总电闸跳闸导致整线停工10分钟\",\"reporter\":\"高伟\",\"occur_time\":\"2025-04-05 08:10:00\",\"location\":\"配电房主线路\",\"type\":\"电力故障\",\"level\":\"Critical\",\"status\":\"Open\",\"assignee\":null,\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":10,\"title\":\"程序错误\",\"product_no\":\"AXA-200\",\"description\":\"机器人R3执行路径出错撞模\",\"reporter\":\"许倩\",\"occur_time\":\"2025-04-05 15:20:00\",\"location\":\"机器人作业区R3\",\"type\":\"软件故障\",\"level\":\"High\",\"status\":\"Processing\",\"assignee\":\"自动化王工\",\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":11,\"title\":\"包装破损\",\"product_no\":\"AXA-200\",\"description\":\"发货前发现G7批次外箱压损\",\"reporter\":\"宋梅\",\"occur_time\":\"2025-04-06 10:00:00\",\"location\":\"仓储物流区\",\"type\":\"运输问题\",\"level\":\"Low\",\"status\":\"Closed\",\"assignee\":\"物流张经理\",\"resolve_time\":\"2025-04-06 10:40:00\",\"ai_solution\":null,\"solution\":\"更换外箱并加固打包\"},{\"id\":12,\"title\":\"漏打螺丝\",\"product_no\":\"AXA-200\",\"description\":\"终检发现X1产品缺少固定螺丝\",\"reporter\":\"冯涛\",\"occur_time\":\"2025-04-06 14:30:00\",\"location\":\"终检区X1产品\",\"type\":\"人为失误\",\"level\":\"Medium\",\"status\":\"Closed\",\"assignee\":\"班组长罗工\",\"resolve_time\":\"2025-04-06 15:00:00\",\"ai_solution\":null,\"solution\":\"补装螺丝并加强检查\"},{\"id\":13,\"title\":\"液压压力低\",\"product_no\":\"AXA-200\",\"description\":\"液压机H9输出压力不达标\",\"reporter\":\"邓杰\",\"occur_time\":\"2025-04-07 09:20:00\",\"location\":\"液压站H9\",\"type\":\"设备故障\",\"level\":\"High\",\"status\":\"Processing\",\"assignee\":\"设备组孙工\",\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":14,\"title\":\"电机过热\",\"product_no\":\"AXA-200\",\"description\":\"传送带T3电机温度过高报警\",\"reporter\":\"秦雪\",\"occur_time\":\"2025-04-07 16:15:00\",\"location\":\"传送带T3\",\"type\":\"设备故障\",\"level\":\"Medium\",\"status\":\"Closed\",\"assignee\":\"维修部王工\",\"resolve_time\":\"2025-04-07 17:00:00\",\"ai_solution\":null,\"solution\":\"清洗风扇并降温\"},{\"id\":15,\"title\":\"参数设置错误\",\"product_no\":\"AXA-200\",\"description\":\"注塑时间设置错误导致成型不良\",\"reporter\":\"韩峰\",\"occur_time\":\"2025-04-08 11:10:00\",\"location\":\"注塑车间控制台\",\"type\":\" 人为失误\",\"level\":\"Medium\",\"status\":\"Processing\",\"assignee\":\"工艺组赵工\",\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":16,\"title\":\"PCB烧板\",\"product_no\":\"AXA-200\",\"description\":\"电路板U5区域烧焦冒烟\",\"reporter\":\"白露\",\"occur_time\":\"2025-04-08 14:40:00\",\"location\":\"电子组装线\",\"type\":\"硬件故障\",\"level\":\"Critical\",\"status\":\"Open\",\"assignee\":null,\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":17,\"title\":\"缺料报警\",\"product_no\":\"AXA-200\",\"description\":\"贴片机持续报警提示元件空料\",\"reporter\":\"毛伟\",\"occur_time\":\"2025-04-09 09:05:00\",\"location\":\"SMT贴片机\",\"type\":\"物料短缺\",\"level\":\"Low\",\"status\":\"Closed\",\"assignee\":\"物料员李姐\",\"resolve_time\":\"2025-04-09 09:30:00\",\"ai_solution\":null,\"solution\":\"补充物料并恢复生产\"},{\"id\":18,\"title\":\"涂层脱落\",\"product_no\":\"AXA-200\",\"description\":\"喷漆线产品表 面涂层脱落\",\"reporter\":\"吕娜\",\"occur_time\":\"2025-04-09 13:20:00\",\"location\":\"喷涂车间\",\"type\":\"工艺问题\",\"level\":\"Medium\",\"status\":\"Closed\",\"assignee\":\"涂装主管\",\"resolve_time\":\"2025-04-09 14:00:00\",\"ai_solution\":null,\"solution\":\"调整烘烤时间并重喷\"},{\"id\":19,\"title\":\"标签打印错误\",\"product_no\":\"AXA-200\",\"description\":\"产品序列号与订单不符\",\"reporter\":\"曾丽\",\"occur_time\":\"2025-04-10 10:15:00\",\"location\":\"包装区打印机\",\"type\":\"人为失误\",\"level\":\"Low\",\"status\":\"Closed\",\"assignee\":\"包装组长\",\"resolve_time\":\"2025-04-10 10:45:00\",\"ai_solution\":null,\"solution\":\"重新打印并核对信息\"},{\"id\":20,\"title\":\"静电击穿\",\"product_no\":\"AXA-200\",\"description\":\"多个IC在装配过程中损坏\",\"reporter\":\"潘强\",\"occur_time\":\"2025-04-10 15:30:00\",\"location\":\"装配台ESD区域\",\"type\":\"环境问题\",\"level\":\"High\",\"status\":\"Processing\",\"assignee\":\"ESD工程 师\",\"resolve_time\":null,\"ai_solution\":null,\"solution\":null},{\"id\":31,\"title\":\"装配线停机\",\"product_no\":\"AXA-200\",\"description\":\"A1线因气压不足导致自动停机\",\"reporter\":\"张强\",\"occur_time\":\"2025-04-01 08:45:00\",\"location\":\"装配车间A1线\",\"type\":\"设备故障\",\"level\":\"High\",\"status\":\"Closed\",\"assignee\":\"李工\",\"resolve_time\":\"2025-04-01 10:30:00\",\"ai_solution\":null,\"solution\":\"更换气泵并恢复供气\"}]"
    }
  ],
  "model": "qwen3:14b",
  "tool_choice": "auto"
}






 */