namespace Jero.Core;

/// <summary>
/// ES客户端类型（标识不同场景）
/// </summary>
public enum EsClientTypeEnum
{
    /// <summary>
    /// 日志专用
    /// </summary>
    Logging,

    /// <summary>
    /// 业务数据
    /// </summary>
    Business
}