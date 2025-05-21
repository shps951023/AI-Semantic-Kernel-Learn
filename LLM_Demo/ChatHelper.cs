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

    public static async Task Chat2(Kernel kernel, string chatMessage)
    {
        ChatCompletionAgent agent =
            new()
            {
                Name = "SK-Assistant",
                Instructions = "You are a helpful assistant.",
                Kernel = kernel,
                Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })

            };

        await foreach (AgentResponseItem<ChatMessageContent> response
            in agent.InvokeAsync("What is the price of the soup special?"))
        {
            Console.WriteLine(response.Message);
        }
        Console.WriteLine("---- 结束查询 ----");
    }
}

