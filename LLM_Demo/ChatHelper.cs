using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class ChatHelper
{
    public static async Task Chat(Kernel kernel, string chatMessage)
    {
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        Console.WriteLine("---- 开始查询 ----");
        await foreach (var content in kernel.InvokePromptStreamingAsync(chatMessage, new(openAIPromptExecutionSettings)))
        {
            Console.Write(content);
        }
        Console.WriteLine("---- 结束查询 ----");
    }
}

