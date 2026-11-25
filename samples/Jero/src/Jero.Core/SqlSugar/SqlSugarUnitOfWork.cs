namespace Jero.Core;

/// <summary>
/// SqlSugar 事务和工作单元
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="sqlSugarClient"></param>
public sealed class SqlSugarUnitOfWork(ISqlSugarClient sqlSugarClient) : IUnitOfWork
{
    /// <summary>
    /// SqlSugar 对象
    /// </summary>
    private readonly ISqlSugarClient _sqlSugarClient = sqlSugarClient;

	/// <summary>
	/// 开启工作单元处理
	/// </summary>
	/// <param name="context"></param>
	/// <param name="unitOfWork"></param>
	/// <exception cref="NotImplementedException"></exception>
	public void BeginTransaction(FilterContext context, UnitOfWorkAttribute unitOfWork)
    {
        _sqlSugarClient.AsTenant().BeginTran();
    }

    /// <summary>
    /// 提交工作单元处理
    /// </summary>
    /// <param name="resultContext"></param>
    /// <param name="unitOfWork"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void CommitTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
    {
        _sqlSugarClient.AsTenant().CommitTran();
    }

    /// <summary>
    /// 回滚工作单元处理
    /// </summary>
    /// <param name="resultContext"></param>
    /// <param name="unitOfWork"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void RollbackTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
    {
        _sqlSugarClient.AsTenant().RollbackTran();
    }

    /// <summary>
    /// 执行完毕（无论成功失败）
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resultContext"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void OnCompleted(FilterContext context, FilterContext resultContext)
    {
        _sqlSugarClient.Dispose();
    }

	public Task BeginTransactionAsync(FilterContext context, UnitOfWorkAttribute unitOfWork)
	{
		return _sqlSugarClient.AsTenant().BeginTranAsync();
	}

	public Task CommitTransactionAsync(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
	{
		return _sqlSugarClient.AsTenant().CommitTranAsync();
	}

	public Task RollbackTransactionAsync(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
	{
		return _sqlSugarClient.AsTenant().RollbackTranAsync();
	}

	public Task OnCompletedAsync(FilterContext context, FilterContext resultContext)
	{
		_sqlSugarClient.Dispose();
		return Task.CompletedTask;
	}
}