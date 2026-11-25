using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.ClientModel;
namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeAgentClientFactory
{
    Task<SaveLifeAgentOptions> GetOptionsAsync();

    Task<bool> CanUseChatAsync();

    Task<bool> CanUseEmbeddingsAsync();

    Task<ChatClient> CreateChatClientAsync();

    async Task<(ChatClient Client, SaveLifeAgentOptions Options)> CreateChatRuntimeAsync()
    {
        var options = await GetOptionsAsync();
        return (await CreateChatClientAsync(), options);
    }

    Task<EmbeddingClient> CreateEmbeddingClientAsync();
}

public sealed class SaveLifeAgentClientFactory(ISaveLifeAgentConfigProvider configProvider) : ISaveLifeAgentClientFactory, ITransient
{
    private readonly ISaveLifeAgentConfigProvider _configProvider = configProvider;

    public Task<SaveLifeAgentOptions> GetOptionsAsync()
    {
        return _configProvider.GetOptionsAsync();
    }

    public async Task<bool> CanUseChatAsync()
    {
        return (await GetOptionsAsync()).HasChatConfiguration;
    }

    public async Task<bool> CanUseEmbeddingsAsync()
    {
        return (await GetOptionsAsync()).HasEmbeddingConfiguration;
    }

    public async Task<ChatClient> CreateChatClientAsync()
    {
        var options = await GetOptionsAsync();
        return CreateChatClient(options);
    }

    public async Task<(ChatClient Client, SaveLifeAgentOptions Options)> CreateChatRuntimeAsync()
    {
        var options = await GetOptionsAsync();
        return (CreateChatClient(options), options);
    }

    private static ChatClient CreateChatClient(SaveLifeAgentOptions options)
    {
        if (!options.HasChatConfiguration)
            throw new InvalidOperationException("SaveLife:Agent chat configuration is incomplete.");

        return new ChatClient(
            options.ChatModel.Trim(),
            new ApiKeyCredential(options.GetChatApiKey()!),
            BuildClientOptions(options.GetChatEndpoint(), "SaveLife:Agent chat endpoint"));
    }

    public async Task<EmbeddingClient> CreateEmbeddingClientAsync()
    {
        var options = await GetOptionsAsync();
        if (!options.HasEmbeddingConfiguration)
            throw new InvalidOperationException("SaveLife:Agent embedding configuration is incomplete.");

        return new EmbeddingClient(
            options.EmbeddingModel.Trim(),
            new ApiKeyCredential(options.GetEmbeddingApiKey()!),
            BuildClientOptions(options.GetEmbeddingEndpoint(), "SaveLife:Agent embedding endpoint"));
    }

    private static OpenAIClientOptions BuildClientOptions(string? endpointValue, string settingName)
    {
        return new OpenAIClientOptions
        {
            Endpoint = SaveLifeAgentEndpointPolicy.RequireAllowed(endpointValue, settingName)
        };
    }
}
