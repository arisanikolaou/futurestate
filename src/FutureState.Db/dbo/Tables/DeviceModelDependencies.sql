CREATE TABLE [dbo].[DeviceModelDependencies] (
    [Id]                        UNIQUEIDENTIFIER NOT NULL,
    [DeviceModelId]             UNIQUEIDENTIFIER NOT NULL,
    [SoftwareModelDependencyId] UNIQUEIDENTIFIER NOT NULL,
    [Description]               NVARCHAR (500)    NULL,
    [DateCreated]               DATE             NULL,
    [DisplayName] NVARCHAR(100) NULL, 
    [ExternalId] NVARCHAR(150) NULL, 
    [ScenarioId] UNIQUEIDENTIFIER NULL, 
    CONSTRAINT [PK_DeviceModelDependencies] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DeviceModelDependencies_DeviceModels] FOREIGN KEY ([DeviceModelId]) REFERENCES [dbo].[DeviceModels] ([Id]),
    CONSTRAINT [FK_DeviceModelDependencies_SoftwareModels] FOREIGN KEY ([SoftwareModelDependencyId]) REFERENCES [dbo].[SoftwareModels] ([Id])
);




GO

GO


GO

GO

