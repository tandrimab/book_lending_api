BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119055747_AddMemberEmail'
)
BEGIN
    ALTER TABLE [Members] ADD [Email] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119055747_AddMemberEmail'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Members_Email] ON [Members] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119055747_AddMemberEmail'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261119055747_AddMemberEmail', N'10.0.14');
END;

COMMIT;
GO

