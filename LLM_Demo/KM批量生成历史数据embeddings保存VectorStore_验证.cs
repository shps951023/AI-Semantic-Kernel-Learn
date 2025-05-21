using Dapper;
using Microsoft.KernelMemory;


public class KM批量生成历史数据embeddings保存VectorStore_验证
{
    public async Task 执行()
    {
        var memory = new MemoryWebClient("http://127.0.0.1:9001/");
        var answer = await memory.AskAsync("AXA-200 异常描述");
        Console.WriteLine("-------------------");
        Console.WriteLine(answer.Question);
        Console.WriteLine(answer.Result);
        Console.WriteLine("-------------------");
    }
}

/*
-------------------
AXA-200 异常描述
<think>
Okay, let's tackle this question. The user is asking about the abnormal descriptions for the product AXA-200. I need to go through the provided facts and extract all the relevant entries where the product is AXA-200.

Looking at the list, I'll scan each entry. The first one is AXA-200 with a current overflow issue in the F3 batch, solution is rework and retest, categorized as a quality problem. Then there's another AXA-200 entry about paint line coating peeling, solution involves adjusting baking time and repainting, which is a process issue.

Next, there's a mention of missing screws in the final inspection, which is a human error. Then there's an outer box damage during shipping, another transport issue. Another one is about a material mix-up by an operator, also human error. There's also a case where the product serial number didn't match the order, which is another human mistake.

Wait, I need to make sure I don't miss any entries. Let me check again. There's also an instance where the AXA-200 had a temperature sensor issue, but that's under P567890. No, wait, no, AXA-200 entries are the ones listed. Let me list them all out:

1. AXA-200, F3 batch current overflow, rework and retest, quality issue.
2. AXA-200, paint line coating peeling, adjust baking time and repaint, process issue.
3. AXA-200, missing screws in final inspection, add screws and check, human error.
4. AXA-200, outer box damage during shipping, replace box and reinforce packing, transport issue.
5. AXA-200, operator mistakenly used XYZ-2 material, isolate batch and refeed, human error.
6. AXA-200, product serial number mismatch, reprint and verify, human error.
7. AXA-200, temperature sensor anomaly leading to overheating alarm, replace sensor and calibrate, hardware issue. Wait, no, the P567890 entry is about temperature sensor. Wait, no, looking back, the entry for AXA-200 includes the temperature sensor? Let me check again. Wait, no, the entry for P567890 is temperature sensor. The AXA-200 entries are the ones listed. Let me confirm each line.

Looking again, the entries for AXA-200 are:

- AXA-200, F3批次电流超标, 返修处理并重新测试, 品质问题
- AXA-200, 喷漆线产品表面涂层脱落, 调整烘烤时间并重喷, 工艺问题
- AXA-200, 终检发现X1产品缺少固定螺丝, 补装螺丝并加强检查, 人为失误
- AXA-200, 发货前发现G7批次外箱压损, 更换外箱并加固打包, 运输问题
- AXA-200, B3线操作员误投型号为XYZ-2的物料, 隔离批次并重新投料, 人为失误
- AXA-200, 产品序列号与订单不符, 重新打印并核对信息, 人为失误
- AXA-200, 注塑机M7出现温度超限报警, 清理冷却系统并复位, 设备故障
- AXA-200, 贴片机持续报警提示元件空料, 补充物料并恢复生产, 物料短缺
- AXA-200, A1线因气压不足导致自动停机, 更换气泵并恢复供气, 设备故障

Wait, I think I missed some entries. Let me go through each line again. The user provided a list where each line is a separate entry. Let me check each line for AXA-200:

Looking at the list:

Line 2: AXA-200, F3批次电流超标, 返修处理并重新测试, 品质问题
Line 7: AXA-200, 喷漆线产品表面涂层脱落, 调整烘烤时间并重喷, 工艺问题
Line 9: AXA-200, 终检发现X1产品缺少固定螺丝, 补装螺丝并加强检查, 人为失误
Line 12: AXA-200, 发货前发现G7批次外箱压损, 更换外箱并加固打包, 运输问题
Line 13: AXA-200, B3线操作员误投型号为XYZ-2的物料, 隔离批次并重新投料, 人为失误
Line 19: AXA-200, 产品序列号与订单不符, 重新打印并核对信息, 人为失误
Line 21: AXA-200, 注塑机M7出现温度超限报警, 清理冷却系统并复位, 设备故障
Line 22: AXA-200, 贴片机持续报警提示元件空料, 补充物料并恢复生产, 物料短缺
Line 24: AXA-200, A1线因气压不足导致自动停机, 更换气泵并恢复供气, 设备故障

Wait, line 24 is AXA-200, A1线气压不足导致停机, solution is replace air pump and restore supply, equipment fault. So that's another entry. Also, line 19 is product sequence number mismatch, which is human error.

So total entries for AXA-200 are:

1. F3批次电流超标 (quality)
2. 喷漆线涂层脱落 (process)
3. X1缺少螺丝 (human error)
4. G7外箱压损 (transport)
5. B3误投物料 (human error)
6. 产品序列号不符 (human error)
7. 注塑机温度报警 (equipment)
8. 贴片机空料报警 (material shortage)
9. A1气压不足停机 (equipment)

Are there any more? Let me check again. Line 12 is G7外箱压损, line 13 is B3误投物料, line 19 is product serial number, line 21 is注塑机温度报警, line 22 is贴片机空料, line 24 is A1气压不足.

So that's nine entries. Now, I need to present all these in the answer. The user wants a comprehensive answer, so list each abnormal description with the solution and type. Make sure to structure it clearly.
</think>

AXA-200 的异常描述及解决方案如下：
1. **成品测试 F3 批次电流超标**
   - 解决方案：返修处理并重新测试
   - 异常类型：品质问题

2. **喷漆线产品表面涂层脱落**
   - 解决方案：调整烘烤时间并重喷
   - 异常类型：工艺问题

3. **终检发现 X1 产品缺少固定螺丝**
   - 解决方案：补装螺丝并加强检查
   - 异常类型：人为失误

4. **发货前发现 G7 批次外箱压损**
   - 解决方案：更换外箱并加固打包
   - 异常类型：运输问题

5. **B3 线操作员误投型号为 XYZ-2 的物料**
   - 解决方案：隔离批次并重新投料
   - 异常类型：人为失误

6. **产品序列号与订单不符**
   - 解决方案：重新打印并核对信息
   - 异常类型：人为失误

7. **注塑机 M7 出现温度超限报警**
   - 解决方案：清理冷却系统并复位
   - 异常类型：设备故障

8. **贴片机持续报警提示元件空料**
   - 解决方案：补充物料并恢复生产
   - 异常类型：物料短缺

9. **A1 线因气压不足导致自动停机**
   - 解决方案：更换气泵并恢复供气
   - 异常类型：设备故障

以上为 AXA-200 的所有异常描述及对应解决方案。
-------------------
 */