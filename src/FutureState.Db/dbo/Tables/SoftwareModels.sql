CREATE TABLE [dbo].[SoftwareModels] (
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [ScenarioId]           UNIQUEIDENTIFIER NULL,
    [ExternalId]         NVARCHAR (150)    NOT NULL,
    [DisplayName]          NVARCHAR (100)    NOT NULL,
    [Description]          NVARCHAR (500)  NOT NULL,
    [SoftwareModelDate]    DATE             NULL,
    [DomainId]             UNIQUEIDENTIFIER NULL,
    [Vendor]               NVARCHAR (100)   NOT NULL,
    [LicenseTypeId]        NVARCHAR (200)   NOT NULL,
    [LicenseReferenceCode] NVARCHAR (100)   NOT NULL,
    [FixedCost]            FLOAT (53)       NULL,
    [AnnualCost]           FLOAT (53)       NULL,
    [LifeCycleId]          NVARCHAR (50)    NULL,
    [LifeCycleStageDate]   SMALLDATETIME    NULL,
    [Version]              NVARCHAR (100)   NOT NULL,
    [Attributes] NVARCHAR(2000) NULL, 
    [UserName] NVARCHAR(150) NOT NULL DEFAULT 'System', 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    CONSTRAINT [PK_SoftwareModels] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SoftwareModels_Domains] FOREIGN KEY ([DomainId]) REFERENCES [dbo].[DesignDomains] ([Id]),
    CONSTRAINT [FK_SoftwareModels_LifeCycles] FOREIGN KEY ([LifeCycleId]) REFERENCES [dbo].[LifeCycles] ([Id]),
    CONSTRAINT [FK_SoftwareModels_Scenarios] FOREIGN KEY ([ScenarioId]) REFERENCES [dbo].[Scenarios] ([Id]), 
    CONSTRAINT [CK_SoftwareModels_CheckExternalId] CHECK (Len([ExternalId]) > 0)
);




GO



GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The release date of the software model.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModels', @level2type=N'COLUMN',@level2name=N'SoftwareModelDate'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The governance domain.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModels', @level2type=N'COLUMN',@level2name=N'DomainId'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The vendor of the software model' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModels', @level2type=N'COLUMN',@level2name=N'Vendor'


GO



GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The type of license' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModels', @level2type=N'COLUMN',@level2name=N'LicenseTypeId'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The license type id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModels', @level2type=N'COLUMN',@level2name=N'LicenseReferenceCode'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'One time cost to aquire the software.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModels', @level2type=N'COLUMN',@level2name=N'FixedCost'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Annual costs for the softare.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModels', @level2type=N'COLUMN',@level2name=N'AnnualCost'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date the lifecycle was changed.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModels', @level2type=N'COLUMN',@level2name=N'LifeCycleStageDate'


GO

GO


GO

GO


GO

GO


GO

GO


GO

GO



CREATE UNIQUE INDEX [IX_SoftwareModels_UniqueKey] ON [dbo].[SoftwareModels] ([ScenarioId], [ExternalId])
