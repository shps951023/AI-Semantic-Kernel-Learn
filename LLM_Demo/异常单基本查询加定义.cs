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
            modelId: "qwen3:14b",
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
@"请帮忙列出 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况
需要特别注意的异常类型跟避免建议"
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
      "content": "请帮忙列出 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况\r\n需要特别注意的异常类型跟避免建议"
    }
  ],
  "model": "qwen3:14b",
  "tool_choice": "auto"
}

----响应----
{
  "id": "chatcmpl-630",
  "object": "chat.completion",
  "created": 1747826465,
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
            "id": "call_zz79b4bm",
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
    "prompt_tokens": 235,
    "completion_tokens": 244,
    "total_tokens": 479
  }
}

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
      "content": "请帮忙列出 AXA-200 产品在 2025-01-01 到 2025-05-20 异常单情况\r\n需要特别注意的异常类型跟避免建议"
    },
    {
      "role": "assistant",
      "content": "",
      "tool_calls": [
        {
          "id": "call_zz79b4bm",
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
      "tool_call_id": "call_zz79b4bm",
      "content": "---\r\n栏位定义:\r\nid INTEGER PRIMARY KEY AUTOINCREMENT,       -- 异常单ID（主键）\r\ntitle TEXT NOT NULL,                        -- 异常标题\r\nproduct_no TEXT NOT NULL,                   -- 产品编号\r\ndescription TEXT,                           -- 异常描述\r\nreporter TEXT NOT NULL,                     -- 报告人\r\noccur_time DATETIME NOT NULL,               -- 异常发生时间\r\nlocation TEXT,                              -- 发生地点/设备/模块\r\ntype TEXT,                                  -- 异常类型（如：系统故障、人为操作、网络中断等）\r\nlevel TEXT CHECK(level IN ('Low', 'Medium', 'High', 'Critical')) NOT NULL DEFAULT 'Medium', -- 紧急程度\r\nstatus TEXT CHECK(status IN ('Open', 'Processing', 'Closed', 'Pending')) DEFAULT 'Open', -- 处理状态\r\nassignee TEXT,                              -- 分配处理人\r\nresolve_time DATETIME,                      -- 解决时间\r\nai_solution TEXT ,                          -- AI生成的解决方案\r\nsolution TEXT                               -- 解决方案或备注\r\n\r\n---\r\n异常单数据\r\n[{\"title\":\"装配线停机\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-01 08:45:00\",\"location\":\"装配车间A1线\",\"type\":\"设备故障\",\"solution\":\"更换气泵并恢复供气\"},{\"title\":\"物料错投\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-01 11:20:00\",\"location\":\"配料区B3线\",\"type\":\"人为失误\",\"solution\":\"隔离批次并重新投料\"},{\"title\":\"焊接不良\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-02 09:15:00\",\"location\":\"焊接车间C2线\",\"type\":\"品质问题\",\"solution\":null},{\"title\":\"贴片偏移\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-02 14:10:00\",\"location\":\"电子组装线SMT\",\"type\":\"设备偏差\",\"solution\":null},{\"title\":\"设备报警\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-03 10:50:00\",\"location\":\"注塑车间M7机\",\"type\":\"设备故障\",\"solution\":\"清理冷却系统并复位\"},{\"title\":\"模具卡料\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-03 16:00:00\",\"location\":\"冲压车间M2线\",\"type\":\"设备故障\",\"solution\":null},{\"title\":\"测试失败\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-04 09:40:00\",\"location\":\"质检中心F3批次\",\"type\":\"品质问题\",\"solution\":\"返修处理并重新测试\"},{\"title\":\"传感器失灵\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-04 13:30:00\",\"location\":\"自动化线E5\",\"type\":\"设备故障\",\"solution\":null},{\"title\":\"电源中断\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-05 08:10:00\",\"location\":\"配电房主线路\",\"type\":\"电力故障\",\"solution\":null},{\"title\":\"程序错误\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-05 15:20:00\",\"location\":\"机器人作业区R3\",\"type\":\"软件故障\",\"solution\":null},{\"title\":\"包 装破损\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-06 10:00:00\",\"location\":\"仓储物流区\",\"type\":\"运输问题\",\"solution\":\"更换外箱并加固打包\"},{\"title\":\"漏打螺丝\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-06 14:30:00\",\"location\":\"终检区X1产品\",\"type\":\"人为失误\",\"solution\":\"补装螺丝并加强检查\"},{\"title\":\"液压压力低\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-07 09:20:00\",\"location\":\"液压站H9\",\"type\":\"设备故障\",\"solution\":null},{\"title\":\"电机过热\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-07 16:15:00\",\"location\":\"传送带T3\",\"type\":\"设备故障\",\"solution\":\"清洗风扇并降温\"},{\"title\":\"参数设置错误\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-08 11:10:00\",\"location\":\"注塑车间控制台\",\"type\":\"人为失误\",\"solution\":null},{\"title\":\"PCB烧板\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-08 14:40:00\",\"location\":\"电子组装线\",\"type\":\"硬件故障\",\"solution\":null},{\"title\":\"缺料报警\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-09 09:05:00\",\"location\":\"SMT贴片机\",\"type\":\"物料短缺\",\"solution\":\"补充物料并恢复生产\"},{\"title\":\"涂层脱落\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-09 13:20:00\",\"location\":\"喷涂车间\",\"type\":\"工艺问题\",\"solution\":\"调整烘烤时间并重喷\"},{\"title\":\"标签打印错误\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-10 10:15:00\",\"location\":\"包装区打印机\",\"type\":\"人为失误\",\"solution\":\"重新打印并核对信息\"},{\"title\":\"静电击穿\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-10 15:30:00\",\"location\":\"装配台ESD区域\",\"type\":\"环境问题\",\"solution\":null},{\"title\":\"装配线停机\",\"product_no\":\"AXA-200\",\"occur_time\":\"2025-04-01 08:45:00\",\"location\":\" 装配车间A1线\",\"type\":\"设备故障\",\"solution\":\"更换气泵并恢复供气\"}]"
    }
  ],
  "model": "qwen3:14b",
  "tool_choice": "auto"
}

----响应----
{
  "id": "chatcmpl-485",
  "object": "chat.completion",
  "created": 1747826489,
  "model": "qwen3:14b",
  "system_fingerprint": "fp_ollama",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "<think>\n好的，我现在需要处理用户关于AXA-200产品在2025年1月1日到2025年5月20日之间的异常单情况的查询 。用户还特别提到需要关注异常类型和避免建议。首先，我需要确认用户提供的数据是否符合时间范围，然后分析异常类型，最后给出相应的建议。\n\n首先，检查数据中的每个异常单的occur_time是否在指定的时间范围内。用户给出的时间是2025-01-01到2025-05-20。查看数据中的日期，发现所有记录的occur_time都在这个范围内，从2025-04-01到2025-04-10，都在用户指定的时间段内，所以没问题。\n\n接下来，统计各个异常类型出现的次数。类型包括设备故障、人为失误、品质问题、设备偏差、电力故障、软件故障、运输问题、物料短缺、工艺问题、环境问题。例如，设备故障出现次数较多，有5次，人为失误有3次，品质问题有2次，其他类型各一次。这可能表明设 备维护和人为操作是主要问题点。\n\n然后，需要针对每个类型提出避免建议。比如，设备故障可能需要定期维护和检查；人为失误可能需要加强培训和流程规范；品质问题可能需要加强质检流程等。同时，注意有些解决方案是null，可能需要进一步分析或建议。\n\n最后，整理这些信息，用清晰的结构呈现给用户，确保涵盖所有异常类型，并给出具体的避免建议，帮助用户改善生产流程。\n</think>\n\n### AXA-200 产品异常单分析 (2025-01-01 至 2025-05-20)\n\n---\n\n#### **异常类型统计**\n| 异常类型   | 出现次数 | 说明 |\n|------------|----------|------|\n| 设备故障   | 5        | 涉及气泵、冷却系统、液压站、电机、传感器等设备问题 |\n| 人 为失误   | 3        | 包括物料错投、参数设置错误、标签打印错误 |\n| 品质问题   | 2        | 焊接不良、测试失败 |\n| 设备偏差   | 1        | 贴片偏移（SMT设备精度问题） |\n| 电力故障   | 1        | 电源中断（配电房主线路） |\n| 软件故障   | 1        | 程序错误（机器人作业区） |\n| 运输问题   | 1        | 包装破损（物流环节） |\n| 物料短缺   | 1        | 缺料 报警（SMT贴片机） |\n| 工艺问题   | 1        | 涂层脱落（喷涂车间烘烤工艺） |\n| 环境问题   | 1        | 静电击穿（ESD区域防护不足） |\n\n---\n\n#### **重点异常类型分析与避免建议**\n\n1. **设备故障（高频问题）**\n   - **问题表现**：气泵故障、冷却系统堵塞、液压压力低、电机过热、传感器失灵。\n   - **避免建议**：\n     - 建立设备预防性维护计划（如每日点检、定期更换易损件）。\n     - 安装实时监控系统（如温度、压力、振动传感器）。\n     - 对关键设备（如气泵、液压站）配置备用单元。\n\n2. **人为失误（可优化流程）**\n   - **问题表现**：物料错投、参数设置错误、标签打印错误。\n   - **避免建议**：\n     - 引入条码/RFID物料追溯系统，减少人工投料错误。\n     - 参数设置增加双人确认机制。\n     - 打印机与MES系统联动，自动校验标签信息。\n\n3. **品质问题（需加强过程控制）**\n   - **问题表现**：焊接不良、测试失败。\n   - **避免建议**：\n     - 增加在线检测设备（如AOI、X-Ray）。\n     - 对焊接工艺进行SPC（统计过程控制）。\n     - 测试环节增加自动复检流程。\n\n4. **环境问题（ESD防护）**\n   - **问题表现**：静电击穿导致电路板损坏。\n   - **避免建议**：\n     - 在ESD区域铺设防静电地胶 ，强制佩戴防静电手环。\n     - 定期检测环境湿度（保持40-60% RH）。\n\n---\n\n#### **未解决异常项（需重点关注）**\n以下异常单未提供有效解决方案，建议优先处理：\n- **焊接不良**（2025-04-02）：需分析焊接参数（温度/时间）是否偏离标准。\n- **贴 片偏移**（2025-04-02）：检查SMT设备校准状态及钢网精度。\n- **模具卡料**（2025-04-03）：优化模具设计或增加自动润滑系统。\n- **PCB烧板**（2025-04-08）：排查电源模块过载保护机制。\n\n---\n\n#### **总结**\n建议优先解决 **设备故障** 和 **人为失 误** 类问题，同时加强 **过程监控** 和 **员工培训**。对于未解决项，需组织跨部门（生产/质量/工程）进行根因分析（RCA）。"
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 1716,
    "completion_tokens": 1190,
    "total_tokens": 2906
  }
}

<think>
好的，我现在需要处理用户关于AXA-200产品在2025年1月1日到2025年5月20日之间的异常单情况的查询。用户还特别提到需要关注异常类型和避免建议。首先，我需要确认用户提供的数据是否符合时间范围，然后分析异常类型，最后给出相应的建议。

首先，检查数据中的每个异常单的occur_time是否在指定的时间范围内。用户给出的时间是2025-01-01到2025-05-20。查看数据中的日期，发现所有记录的occur_time都在这个范围内，从2025-04-01到2025-04-10，都在用户指定的时间段内，所以没问题。

接下来，统计各个异常类型出现的次数。类型包括设备故障、人为失误、品质问题、设备偏差、电力故障、软件故障、运输问题、物料短缺、工艺问题、环境问题。例如，设备故障出现次数较多，有5次，人为失误有3次，品质问题有2次，其他类型各一次。这可能表明设备 维护和人为操作是主要问题点。

然后，需要针对每个类型提出避免建议。比如，设备故障可能需要定期维护和检查；人为失误可能需要加强培训和流程规范；品质问题可能需要加强质检流程等。同时，注意有些解决方案是null，可能需要进一步分析或建议。

最后，整理这些信息，用清晰的结构呈现给用户，确保涵盖所有异常类型，并给出具体的避免建议，帮助用户改善生产流程。
</think>

### AXA-200 产品异常单分析 (2025-01-01 至 2025-05-20)

---

#### **异常类型统计**
| 异常类型   | 出现次数 | 说明 |
|------------|----------|------|
| 设备故障   | 5        | 涉及气泵、冷却系统、液压站、电机、传感器等设备问题 |
| 人为失误   | 3        | 包括物料错投、参数设置错误、标签打印错误 |
| 品质问题   | 2        | 焊接不良、测试失败 |
| 设备偏差   | 1        | 贴片偏移（SMT设备精度问题） |
| 电力故障   | 1        | 电源中断（配电房主线路） |
| 软件故障   | 1        | 程序错误（机器人作业区） |
| 运输问题   | 1        | 包装破损（物流环节） |
| 物料短缺   | 1        | 缺料报警（SMT贴片机） |
| 工艺问题   | 1        | 涂层脱落（喷涂车间烘烤工艺） |
| 环境问题   | 1        | 静电击穿（ESD区域防护不足） |

---

#### **重点异常类型分析与避免建议**

1. **设备故障（高频问题）**
   - **问题表现**：气泵故障、冷却系统堵塞、液压压力低、电机过热、传感器失灵。
   - **避免建议**：
     - 建立设备预防性维护计划（如每日点检、定期更换易损件）。
     - 安装实时监控系统（如温度、压力、振动传感器）。
     - 对关键设备（如气泵、液压站）配置备用单元。

2. **人为失误（可优化流程）**
   - **问题表现**：物料错投、参数设置错误、标签打印错误。
   - **避免建议**：
     - 引入条码/RFID物料追溯系统，减少人工投料错误。
     - 参数设置增加双人确认机制。
     - 打印机与MES系统联动，自动校验标签信息。

3. **品质问题（需加强过程控制）**
   - **问题表现**：焊接不良、测试失败。
   - **避免建议**：
     - 增加在线检测设备（如AOI、X-Ray）。
     - 对焊接工艺进行SPC（统计过程控制）。
     - 测试环节增加自动复检流程。

4. **环境问题（ESD防护）**
   - **问题表现**：静电击穿导致电路板损坏。
   - **避免建议**：
     - 在ESD区域铺设防静电地胶，强制佩戴防静电手环。
     - 定期检测环境湿度（保持40-60% RH）。

---

#### **未解决异常项（需重点关注）**
以下异常单未提供有效解决方案，建议优先处理：
- **焊接不良**（2025-04-02）：需分析焊接参数（温度/时间）是否偏离标准。
- **贴片偏移**（2025-04-02）：检查SMT设备校准状态及钢网精度。
- **模具卡料**（2025-04-03）：优化模具设计或增加自动润滑系统。
- **PCB烧板**（2025-04-08）：排查电源模块过载保护机制。

---

#### **总结**
建议优先解决 **设备故障** 和 **人为失误** 类问题，同时加强 **过程监控** 和 **员工培训**。对于未解决项，需组织跨部门（生 产/质量/工程）进行根因分析（RCA）。

C:\Users\vipuser\Desktop\LLM_Demo\LLM_Demo\bin\Debug\net8.0\LLM_Demo.exe (进程 13080)已退出，代码为 0 (0x0)。
要在调试停止时自动关闭控制台，请启用“工具”->“选项”->“调试”->“调试停止时自动关闭控制台”。
按任意键关闭此窗口. . .


 
 */