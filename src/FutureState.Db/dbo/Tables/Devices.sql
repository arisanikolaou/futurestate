CREATE TABLE [dbo].[Devices] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [ScenarioId]    UNIQUEIDENTIFIER NULL,
    [DeviceModelId] UNIQUEIDENTIFIER NOT NULL,
    [NetworkId]     UNIQUEIDENTIFIER NOT NULL,
    [DisplayName]   NVARCHAR (100)   NOT NULL,
    [Description]   NVARCHAR (500)   NOT NULL,
    [DateAdded]    SMALLDATETIME    NOT NULL DEFAULT getutcdate(),
	[DateRemoved] SMALLDATETIME NULL, 
    [FixedCost]     FLOAT (53)       NULL,
    [AnnualCost]    FLOAT (53)       NULL,
    [ExternalId]    NVARCHAR (50)    NOT NULL,
    [Attributes] NVARCHAR(2000) NULL, 
    [AvailabilityTier] NVARCHAR(100) NOT NULL DEFAULT 'Silver', 
    [UserName] NVARCHAR(150) NOT NULL DEFAULT 'System', 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    CONSTRAINT [PK_Devices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Devices_DeviceModels] FOREIGN KEY ([DeviceModelId]) REFERENCES [dbo].[DeviceModels] ([Id]),
    CONSTRAINT [FK_Devices_Networks] FOREIGN KEY ([NetworkId]) REFERENCES [dbo].[Networks] ([Id]),
    CONSTRAINT [FK_Devices_Scenarios] FOREIGN KEY ([ScenarioId]) REFERENCES [dbo].[Scenarios] ([Id])
);




GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The date the device was setup/acquired.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Devices', @level2type=N'COLUMN',@level2name='DateAdded'


GO

GO


GO

GO


GO

GO


GO

GO


EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The date the device would be removed.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Devices',
    @level2type = N'COLUMN',
    @level2name = N'DateRemoved'