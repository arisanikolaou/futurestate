CREATE TABLE [dbo].[DeviceModels] (
    [Id]                 UNIQUEIDENTIFIER NOT NULL,
    [ScenarioId]         UNIQUEIDENTIFIER NULL,
    [DisplayName]        NVARCHAR (50)    NOT NULL,
    [ModelVersion]       VARCHAR(10)    NOT NULL,
    [Description]        NVARCHAR (500)   NOT NULL,
    [ExternalId]         NVARCHAR (150)    NOT NULL,
    [DomainId]           UNIQUEIDENTIFIER NULL,
    [AnnualCost]         FLOAT (53)       NULL,
    [FixedCost]          FLOAT (53)       NULL,
    [LifeCycleId]        NVARCHAR (50)    NULL,
    [LifeCycleStageDate] SMALLDATETIME    NULL,
    [Attributes] NVARCHAR(2000) NULL, 
    [DateCreated] SMALLDATETIME NULL DEFAULT getutcdate(), 
    [UserName] VARCHAR(150) NULL DEFAULT 'System', 
    [DateLastModified] SMALLDATETIME NULL DEFAULT getutcdate(), 
    CONSTRAINT [PK_DeviceModels] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DeviceModels_Domains] FOREIGN KEY ([DomainId]) REFERENCES [dbo].[DesignDomains] ([Id]),
    CONSTRAINT [FK_DeviceModels_LifeCycles] FOREIGN KEY ([LifeCycleId]) REFERENCES [dbo].[LifeCycles] ([Id]),
    CONSTRAINT [FK_DeviceModels_Scenarios] FOREIGN KEY ([ScenarioId]) REFERENCES [dbo].[Scenarios] ([Id])
);




GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Model asset stage.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'DeviceModels', @level2type=N'COLUMN',@level2name=N'LifeCycleId'


GO

GO


GO

GO


GO

GO

