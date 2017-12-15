CREATE TABLE [dbo].[UserGroups]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT newid(), 
    [DisplayName] VARCHAR(150) NOT NULL, 
    [Description] NVARCHAR(250) NOT NULL, 
    [DateCreated] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    [UserName] NVARCHAR(150) NOT NULL DEFAULT 'System', 
    [ScenarioId] UNIQUEIDENTIFIER NULL, CONSTRAINT [FK_UserGroups_Scenarios] FOREIGN KEY ([ScenarioId]) REFERENCES [Scenarios]([Id])
)

GO

CREATE UNIQUE INDEX [IX_UserGroups_UniqueKey] ON [dbo].[UserGroups] ([ScenarioId], [DisplayName])
