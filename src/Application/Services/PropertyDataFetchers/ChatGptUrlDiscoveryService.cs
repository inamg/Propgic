using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class ChatGptUrlDiscoveryService
{
    private readonly string _apiKey;
    private readonly AzureOpenAIClient? _azureClient;
    private readonly OpenAI.OpenAIClient? _openAIClient;
    private readonly bool _isAzure;

    public ChatGptUrlDiscoveryService(string apiKey, string? azureEndpoint = null, string? azureDeploymentName = null)
    {
        _apiKey = apiKey;

        if (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureDeploymentName))
        {
            _azureClient = new AzureOpenAIClient(new Uri(azureEndpoint), new AzureKeyCredential(apiKey));
            _isAzure = true;
        }
        else
        {
            _openAIClient = new OpenAI.OpenAIClient(apiKey);
            _isAzure = false;
        }
    }

    public async Task<string?> GetPropertyUrlAsync(string propertyAddress, string websiteName)
    {
        try
        {
            var prompt = $@"Given the property address: '{propertyAddress}'

Please construct or suggest the most likely property listing URL on {websiteName}.

Return ONLY the URL, nothing else. If you cannot determine a specific URL, return an empty string.";

            ChatCompletion completion;

            if (_isAzure && _azureClient != null)
            {
                var chatClient = _azureClient.GetChatClient("gpt-4");
                completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage("You are a helpful assistant that constructs property listing URLs for Australian real estate websites."),
                    new UserChatMessage(prompt)
                ]);
            }
            else if (_openAIClient != null)
            {
                var chatClient = _openAIClient.GetChatClient("gpt-4o-mini");
                completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage("You are a helpful assistant that constructs property listing URLs for Australian real estate websites."),
                    new UserChatMessage(prompt)
                ]);
            }
            else
            {
                return null;
            }

            var url = completion.Content[0].Text?.Trim();

            // Validate that it's a proper URL
            if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return url;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting URL from ChatGPT: {ex.Message}");
            return null;
        }
    }
}
