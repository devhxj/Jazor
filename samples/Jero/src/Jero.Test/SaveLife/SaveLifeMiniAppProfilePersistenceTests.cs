using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMiniAppProfilePersistenceTests
{
    [Fact]
    public async Task UpdateAvatarAsync_WithNewFile_PersistsFileReferenceAndUrl()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.Ado.ExecuteCommand("""
                CREATE TABLE sl_user_profile (
                    Id INTEGER PRIMARY KEY,
                    AvatarFileId INTEGER NULL,
                    AvatarUrl TEXT NULL,
                    UpdateTime TEXT NULL
                );
                INSERT INTO sl_user_profile (Id, AvatarFileId, AvatarUrl)
                VALUES (1, 101, 'upload/save-life/avatar/101.jpg');
                """);

            var profile = new SaveLifeUserProfile
            {
                Id = 1,
                AvatarFileId = 202,
                AvatarUrl = "upload/save-life/avatar/202.png",
            };

            var affected = await SaveLifeMiniAppProfilePersistence.UpdateAvatarAsync(db, profile);
            var row = db.Ado.SqlQuery<ProfileAvatarRow>("""
                SELECT AvatarFileId, AvatarUrl
                FROM sl_user_profile
                WHERE Id = 1
                """).Single();

            Assert.Equal(1, affected);
            Assert.Equal(202, row.AvatarFileId);
            Assert.Equal("upload/save-life/avatar/202.png", row.AvatarUrl);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task UpdateAsync_WithClearedOptionalFields_PersistsNullValues()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.Ado.ExecuteCommand("""
                CREATE TABLE sl_user_profile (
                    Id INTEGER PRIMARY KEY,
                    RealName TEXT NULL,
                    Gender TEXT NULL,
                    BirthDate TEXT NULL,
                    Phone TEXT NULL,
                    Email TEXT NULL,
                    Address TEXT NULL,
                    Ethnicity TEXT NULL,
                    EmergencyContact TEXT NULL,
                    EmergencyPhone TEXT NULL,
                    MedicalHistory TEXT NULL,
                    Allergies TEXT NULL,
                    UpdateTime TEXT NULL
                );
                INSERT INTO sl_user_profile (
                    Id, RealName, Gender, BirthDate, Phone, Email, Address,
                    Ethnicity, EmergencyContact, EmergencyPhone, MedicalHistory, Allergies
                ) VALUES (
                    1, '张三', 'male', '1988-01-01', '13800138000', 'user@example.com',
                    '上海市', '汉族', '李四', '13900139000', '既往病史', '青霉素'
                );
                """);

            var profile = new SaveLifeUserProfile
            {
                Id = 1,
                UpdateTime = new DateTime(2026, 7, 11, 18, 0, 0),
            };

            var affected = await SaveLifeMiniAppProfilePersistence.UpdateAsync(db, profile);
            var row = db.Ado.SqlQuery<ProfileOptionalFieldsRow>("""
                SELECT RealName, Gender, BirthDate, Phone, Email, Address, Ethnicity,
                       EmergencyContact, EmergencyPhone, MedicalHistory, Allergies
                FROM sl_user_profile
                WHERE Id = 1
                """).Single();

            Assert.Equal(1, affected);
            Assert.Null(row.RealName);
            Assert.Null(row.Gender);
            Assert.Null(row.BirthDate);
            Assert.Null(row.Phone);
            Assert.Null(row.Email);
            Assert.Null(row.Address);
            Assert.Null(row.Ethnicity);
            Assert.Null(row.EmergencyContact);
            Assert.Null(row.EmergencyPhone);
            Assert.Null(row.MedicalHistory);
            Assert.Null(row.Allergies);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"Data Source={dbPath};Pooling=False",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });
    }

    private sealed class ProfileOptionalFieldsRow
    {
        public string? RealName { get; set; }

        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? Ethnicity { get; set; }

        public string? EmergencyContact { get; set; }

        public string? EmergencyPhone { get; set; }

        public string? MedicalHistory { get; set; }

        public string? Allergies { get; set; }
    }

    private sealed class ProfileAvatarRow
    {
        public long? AvatarFileId { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
