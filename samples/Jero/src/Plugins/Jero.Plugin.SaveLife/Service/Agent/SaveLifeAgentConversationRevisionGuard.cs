using System.Linq.Expressions;
using SqlSugar;

namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifeAgentConversationRevisionGuard
{
    public static async Task<long> AdvanceRevisionAsync(
        ISqlSugarClient db,
        long conversationId,
        string externalUserId,
        long expectedRevision,
        DateTime changedTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        var nextRevision = checked(expectedRevision + 1);
        var affected = await db.Updateable<SaveLifeAgentConversation>()
            .SetColumns(item => new SaveLifeAgentConversation
            {
                StateRevision = nextRevision,
                UpdateTime = changedTime
            })
            .Where(item =>
                item.Id == conversationId &&
                item.ExternalUserId == externalUserId &&
                item.StateRevision == expectedRevision &&
                item.Status == "active" &&
                !item.IsDelete)
            .ExecuteCommandAsync(cancellationToken);
        if (affected != 1)
            throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();

        return nextRevision;
    }

    public static async Task<long> AdvanceAsync(
        ISqlSugarClient db,
        SaveLifeAgentConversation conversation,
        Expression<Func<SaveLifeAgentConversation, SaveLifeAgentConversation>> changes,
        DateTime changedTime,
        Expression<Func<SaveLifeAgentConversation, bool>>? additionalGuard = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(changes);
        var expectedRevision = conversation.StateRevision;
        var nextRevision = checked(expectedRevision + 1);
        var update = db.Updateable<SaveLifeAgentConversation>()
            .SetColumns(changes)
            .SetColumns(item => new SaveLifeAgentConversation
            {
                StateRevision = nextRevision,
                UpdateTime = changedTime
            })
            .Where(item =>
                item.Id == conversation.Id &&
                item.ExternalUserId == conversation.ExternalUserId &&
                item.StateRevision == expectedRevision &&
                item.Status == "active" &&
                !item.IsDelete);
        if (additionalGuard != null)
            update = update.Where(additionalGuard);

        var affected = await update.ExecuteCommandAsync(cancellationToken);
        if (affected != 1)
            throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();

        conversation.StateRevision = nextRevision;
        conversation.UpdateTime = changedTime;
        return nextRevision;
    }
}
