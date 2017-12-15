CREATE TABLE [dbo].[ApplicationAccesses]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [DeviceId] UNIQUEIDENTIFIER NOT NULL, 
    [DeviceModelDependencyId] UNIQUEIDENTIFIER NOT NULL, 
    [StakeholderLoginId] UNIQUEIDENTIFIER NOT NULL, 
    [UserGroupId] UNIQUEIDENTIFIER NULL, 
    [UserName] NVARCHAR(150) NOT NULL DEFAULT  'System', 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    [ScenarioId] UNIQUEIDENTIFIER NULL, 
    [ExternalId] NVARCHAR(150) NOT NULL, 
    [Description] NVARCHAR(200) NOT NULL, 
    [DisplayName] NVARCHAR(150) NOT NULL, 
    CONSTRAINT [FK_ApplicationAccesses_StakeholderLogins] FOREIGN KEY ([StakeholderLoginId]) REFERENCES [StakeholderLogins]([Id]),
	CONSTRAINT [FK_ApplicationAccesses_Devices] FOREIGN KEY ([DeviceId]) REFERENCES [Devices]([Id]),
	CONSTRAINT [FK_ApplicationAccesses_DevicesApps] FOREIGN KEY (DeviceModelDependencyId) REFERENCES [DeviceModelDependencies]([Id])
)

GO

CREATE UNIQUE INDEX [IX_ApplicationAccesses_UniqueDevice] ON [dbo].[ApplicationAccesses] ([StakeholderLoginId], [ScenarioId], [DeviceId])

GO

CREATE UNIQUE INDEX [IX_ApplicationAccesses_UniqueExternalKey] ON [dbo].[ApplicationAccesses] ([ScenarioId], [ExternalId])
