CREATE TABLE [dbo].[DeviceConnections] (
	[Id]             UNIQUEIDENTIFIER NOT NULL,
	[ExternalId]	 NVARCHAR(150) NOT NULL, 
	[ScenarioId]     UNIQUEIDENTIFIER NULL,
	[NetworkId]      UNIQUEIDENTIFIER NOT NULL,
	[DeviceSourceId] UNIQUEIDENTIFIER NOT NULL,
	[DeviceSourceAccountId] UNIQUEIDENTIFIER NULL,
	[SourceAddress]  NVARCHAR (100)   NULL,
	[DeviceTargetId] UNIQUEIDENTIFIER NOT NULL,
	[TargetAddress]  NVARCHAR (100)   NULL,
	[TargetRoleAuthorization] NVARCHAR(400) NULL,
	[DailyAvgTransactionIo] Float NULL,
	[DailyAvgStorageIo] Float NULL,
	[Description]    NVARCHAR (500)   NOT NULL,
	[DisplayName]    NVARCHAR (100)   NULL,
	[DateAdded]      SMALLDATETIME    NOT NULL DEFAULT getutcdate(),
	[DateCreated]    SMALLDATETIME    NOT NULL DEFAULT getutcdate(),
	[DateRemoved]    SMALLDATETIME    NULL,
	[ProtocolId]     UNIQUEIDENTIFIER NULL,
	[SensitivityLevel] INT NULL , 
	[UserName] VARCHAR(150) NOT NULL DEFAULT 'System', 
	[DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
	CONSTRAINT [PK_DeviceConnections] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_DeviceConnections_DevicesSource] FOREIGN KEY ([DeviceSourceId]) REFERENCES [dbo].[Devices] ([Id]),
	CONSTRAINT [FK_DeviceConnections_DevicesTarget] FOREIGN KEY ([DeviceTargetId]) REFERENCES [dbo].[Devices] ([Id]),
	CONSTRAINT [FK_DeviceConnections_Networks] FOREIGN KEY ([NetworkId]) REFERENCES [dbo].[Networks] ([Id]),
	CONSTRAINT [FK_DeviceConnections_Protocols] FOREIGN KEY ([ProtocolId]) REFERENCES [dbo].[Protocols] ([Id]),
	CONSTRAINT [FK_DeviceConnections_Scenarios] FOREIGN KEY ([ScenarioId]) REFERENCES [dbo].[Scenarios] ([Id]), 
	CONSTRAINT [CK_DeviceConnections_ExternalIdExists] CHECK (Len([ExternalId]) > 0), 
	CONSTRAINT [CK_DeviceConnections_SourceNotTarget] CHECK ([DeviceSourceId] <> [DeviceTargetId]),
);














GO

GO


GO

GO


GO

GO


EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'The device/app that is being connected to. This is the provider of data.',
	@level0type = N'SCHEMA',
	@level0name = N'dbo',
	@level1type = N'TABLE',
	@level1name = N'DeviceConnections',
	@level2type = N'COLUMN',
	@level2name = N'DeviceTargetId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'The login/account that is used to connect to the target device/app.',
	@level0type = N'SCHEMA',
	@level0name = N'dbo',
	@level1type = N'TABLE',
	@level1name = N'DeviceConnections',
	@level2type = N'COLUMN',
	@level2name = N'DeviceSourceAccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'The list of roles/logins authorized to access the connection.',
	@level0type = N'SCHEMA',
	@level0name = N'dbo',
	@level1type = N'TABLE',
	@level1name = N'DeviceConnections',
	@level2type = N'COLUMN',
	@level2name = N'TargetRoleAuthorization'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The address that is being connected to or the port.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DeviceConnections',
    @level2type = N'COLUMN',
    @level2name = N'TargetAddress'
GO