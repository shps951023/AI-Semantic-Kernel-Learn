using Newtonsoft.Json;

/// <summary>
/// 修改自 https://github.com/andrew0928/AndrewDemo.DevAIAPPs/blob/master/UseMicrosoft_SemanticKernel/HttpLogger.cs
/// </summary>
public class DemoLogger : DelegatingHandler
{
    public static HttpClient GetHttpClient(bool log = false)
    {
        var hc = log
            ? new HttpClient(new DemoLogger(new HttpClientHandler()))
            : new HttpClient();

        hc.Timeout = TimeSpan.FromMinutes(5);

        return hc;
    }

    public DemoLogger(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("----请求----");
        if (request.Content != null)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var jsonObj = JsonConvert.DeserializeObject(body);
                if (jsonObj is Newtonsoft.Json.Linq.JObject jObject)
                {
                    foreach (var item in jObject)
                    {
                        if (item.Value.Type == Newtonsoft.Json.Linq.JTokenType.String)
                        {
                            var str = item.Value.ToString();
                            if (str.StartsWith(@"\u"))
                            {
                                var decodedStr = System.Text.RegularExpressions.Regex.Unescape(str);
                                jObject[item.Key] = decodedStr;
                            }
                        }
                    }
                }
                Console.WriteLine(JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
            }
            catch (Exception)
            {
                Console.WriteLine(JsonConvert.SerializeObject(body, Formatting.Indented));
            }

            
        }
        Console.WriteLine();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        Console.WriteLine("----响应----");
        if (response.Content != null)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var jsonObj = JsonConvert.DeserializeObject(body);
                if (jsonObj is Newtonsoft.Json.Linq.JObject jObject)
                {
                    foreach (var item in jObject)
                    {
                        if (item.Value.Type == Newtonsoft.Json.Linq.JTokenType.String)
                        {
                            var str = item.Value.ToString();
                            if (str.StartsWith(@"\u"))
                            {
                                var decodedStr = System.Text.RegularExpressions.Regex.Unescape(str);
                                jObject[item.Key] = decodedStr;
                            }
                        }
                    }
                }
                Console.WriteLine(JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
            }
            catch (Exception)
            {
                Console.WriteLine(body);
            }
        }
        Console.WriteLine();
        return response;
    }
}