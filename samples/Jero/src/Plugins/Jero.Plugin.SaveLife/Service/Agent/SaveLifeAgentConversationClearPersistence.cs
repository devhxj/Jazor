using System.Runtime.ExceptionServices;
using SqlSugar;

namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifeAgentConversationClearPersistence
{
    public static async Task<SaveLifeAgentConversationClearResult> ClearAsync(
        ISqlSugarClient db,
        string externalUserId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        var owner = externalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("咨询记录所属账号不能为空", nameof(externalUserId));

        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            var messageCount = await db.Updateable<SaveLifeAgentMessage>()
                .SetColumns(item => new SaveLifeAgentMessage
                {
                    IsDelete = true,
                    DeleteTime = now,
                    UpdateTime = now
                })
                .Where(item =>
                    item.ExternalUserId == owner &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            var profileProposalCount = await db.Updateable<SaveLifePatientProfileProposal>()
                .SetColumns(item => new SaveLifePatientProfileProposal
                {
                    IsDelete = true,
                    DeleteTime = now,
                    UpdateTime = now
                })
                .Where(item =>
                    item.OwnerExternalUserId == owner &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            var businessCommandCount = await db.Updateable<SaveLifeAgentBusinessCommand>()
                .SetColumns(item => new SaveLifeAgentBusinessCommand
                {
                    IsDelete = true,
                    DeleteTime = now,
                    UpdateTime = now
                })
                .Where(item =>
                    item.ExternalUserId == owner &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            var conversationCount = await db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(item => new SaveLifeAgentConversation
                {
                    IsDelete = true,
                    DeleteTime = now,
                    UpdateTime = now,
                    GenerationLeaseOwner = null,
                    GenerationLeaseTurnSequence = null,
                    GenerationLeaseExpiresTime = null
                })
                .Where(item =>
                    item.ExternalUserId == owner &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);

            return new SaveLifeAgentConversationClearResult(
                conversationCount,
                messageCount,
                profileProposalCount,
                businessCommandCount);
        }, _ => { });

        if (!transaction.IsSuccess)
        {
            ExceptionDispatchInfo.Capture(
                transaction.ErrorException ?? new InvalidOperationException("咨询记录清空事务失败"))
                .Throw();
        }

        return transaction.Data ?? SaveLifeAgentConversationClearResult.Empty;
    }
}

internal sealed record SaveLifeAgentConversationClearResult(
    int ConversationCount,
    int MessageCount,
    int ProfileProposalCount,
    int BusinessCommandCount)
{
    public static SaveLifeAgentConversationClearResult Empty { get; } = new(0, 0, 0, 0);
}
