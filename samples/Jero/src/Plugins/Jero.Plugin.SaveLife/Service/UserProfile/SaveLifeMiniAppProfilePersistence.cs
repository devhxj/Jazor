namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeMiniAppProfilePersistence
{
    public static Task<int> UpdateAvatarAsync(
        ISqlSugarClient db,
        SaveLifeUserProfile profile)
    {
        profile.UpdateTime = DateTime.Now;
        return db.Updateable(profile)
            .UpdateColumns(u => new
            {
                u.AvatarFileId,
                u.AvatarUrl,
                u.UpdateTime
            })
            .ExecuteCommandAsync();
    }

    public static Task<int> UpdateAsync(
        ISqlSugarClient db,
        SaveLifeUserProfile profile)
    {
        profile.UpdateTime = DateTime.Now;
        return db.Updateable(profile)
            .UpdateColumns(u => new
            {
                u.RealName,
                u.Gender,
                u.BirthDate,
                u.Phone,
                u.Email,
                u.Address,
                u.Ethnicity,
                u.EmergencyContact,
                u.EmergencyPhone,
                u.MedicalHistory,
                u.Allergies,
                u.UpdateTime
            })
            .ExecuteCommandAsync();
    }
}
