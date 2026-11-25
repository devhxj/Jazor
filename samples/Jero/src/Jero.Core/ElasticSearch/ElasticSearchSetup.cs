


using Elastic.Clients.Elasticsearch;
namespace Jero.Core.ElasticSearch;

/// <summary>
/// ES服务注册
/// </summary>
public static class ElasticSearchSetup
{
    /// <summary>
    /// 注册所有ES客户端（日志+业务）
    /// </summary>
    public static void AddElasticSearchClients(this IServiceCollection services)
    {
        // 1. 创建客户端字典（枚举→客户端实例）
        var clients = new Dictionary<EsClientTypeEnum, ElasticsearchClient>();

        // 2. 注册日志客户端
        var loggingClient = ElasticSearchClientFactory.CreateClient<ElasticSearchOptions>(configPath: "ElasticSearch:Logging");
        if (loggingClient != null)
        {
            clients[EsClientTypeEnum.Logging] = loggingClient;
        }

        // 3. 注册业务客户端
        var businessClient = ElasticSearchClientFactory.CreateClient<ElasticSearchOptions>(configPath: "ElasticSearch:Business");
        if (businessClient != null)
        {
            clients[EsClientTypeEnum.Business] = businessClient;
        }

        // 4. 将客户端容器注册为单例（全局唯一）
        services.AddSingleton(new ElasticSearchClientContainer(clients));
    }
}