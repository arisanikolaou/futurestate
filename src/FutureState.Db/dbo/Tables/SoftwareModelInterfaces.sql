CREATE TABLE [dbo].[SoftwareModelInterfaces] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [ExternalId] NVARCHAR(150) NOT NULL, 
    [ScenarioId]       UNIQUEIDENTIFIER NULL,
    [SoftwareModelId]  UNIQUEIDENTIFIER NOT NULL,
    [DisplayName]      NVARCHAR (150)    NOT NULL,
    [ProtocolId]       UNIQUEIDENTIFIER NULL,
    [Description]      NVARCHAR (500)   NOT NULL,
    [SensitivityLevel] INT              NULL,
    [DateCreated]      SMALLDATETIME    NOT NULL DEFAULT getutcdate(),
    [UserName] NVARCHAR(150) NULL DEFAULT N'System', 
    CONSTRAINT [PK_SoftwareModelInterfaces] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SoftwareModelInterfaces_Protocols] FOREIGN KEY ([ProtocolId]) REFERENCES [dbo].[Protocols] ([Id]),
    CONSTRAINT [FK_SoftwareModelInterfaces_Scenarios] FOREIGN KEY ([ScenarioId]) REFERENCES [dbo].[Scenarios] ([Id]),
    CONSTRAINT [FK_SoftwareModelInterfaces_SoftwareModels] FOREIGN KEY ([SoftwareModelId]) REFERENCES [dbo].[SoftwareModels] ([Id])
);






GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The software model exposing the interface.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModelInterfaces', @level2type=N'COLUMN',@level2name=N'SoftwareModelId'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The full display name of the interface' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModelInterfaces', @level2type=N'COLUMN',@level2name=N'DisplayName'


GO



GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The type of protocol exposed' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModelInterfaces', @level2type=N'COLUMN',@level2name=N'ProtocolId'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A description of the interface.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModelInterfaces', @level2type=N'COLUMN',@level2name=N'Description'


GO



GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The sensitivity level of the data exposed.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SoftwareModelInterfaces', @level2type=N'COLUMN',@level2name=N'SensitivityLevel'


GO

GO


GO

GO


GO

GO


GO

GO



CREATE INDEX [IX_SoftwareModelInterfaces_UniqueByModelId] ON [dbo].[SoftwareModelInterfaces] ([ScenarioId], [SoftwareModelId], [ProtocolId])

GO

CREATE INDEX [IX_SoftwareModelInterfaces_UniqueRow] ON [dbo].[SoftwareModelInterfaces] ([ScenarioId], [ExternalId])
