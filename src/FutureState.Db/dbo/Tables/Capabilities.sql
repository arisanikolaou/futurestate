CREATE TABLE [dbo].[Capabilities] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL,
    [BusinessUnitId]      UNIQUEIDENTIFIER NULL,
    [SoftwareModelId]         UNIQUEIDENTIFIER NOT NULL,
    [ScenarioId]          UNIQUEIDENTIFIER NULL,
    [DisplayName]         NVARCHAR (100)   NOT NULL,
    [Description]         NVARCHAR (1500)  NOT NULL,
    [BusinessValue]       FLOAT (53)       NULL,
    [AnnualBusinessValue] FLOAT (53)       NULL,
    [DateCreated]         SMALLDATETIME    NOT NULL DEFAULT getutcdate(),
    [ExternalId] NVARCHAR(150) NOT NULL, 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    [UserName] NVARCHAR(150) NOT NULL DEFAULT 'System', 
    CONSTRAINT [PK_Capabilities] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Capabilities_BusinessUnits] FOREIGN KEY ([BusinessUnitId]) REFERENCES [dbo].[BusinessUnits] ([Id]),
    CONSTRAINT [FK_Capabilities_Scenarios] FOREIGN KEY (ScenarioId) REFERENCES [dbo].[Scenarios] ([Id]), 
    CONSTRAINT [FK_Capabilities_ToSoftwareModels] FOREIGN KEY (SoftwareModelId) REFERENCES [SoftwareModels]([Id])
);




GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The capabilities provided by an asset.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Capabilities'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The business unit defining the capability.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Capabilities', @level2type=N'COLUMN',@level2name=N'BusinessUnitId'


GO



GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Display name of the capability.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Capabilities', @level2type=N'COLUMN',@level2name=N'DisplayName'


GO

GO


GO

GO


GO

GO


EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The entity associated with the capability.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Capabilities',
    @level2type = N'COLUMN',
    @level2name = 'SoftwareModelId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The fixed business value provided by the capability.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Capabilities',
    @level2type = N'COLUMN',
    @level2name = N'BusinessValue'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The annual business value of the capability.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Capabilities',
    @level2type = N'COLUMN',
    @level2name = N'AnnualBusinessValue'
GO

CREATE UNIQUE INDEX [IX_Capabilities_UniqueByScenarioAndExternalId] ON [dbo].[Capabilities] ([ScenarioId], [ExternalId], [BusinessUnitId])
