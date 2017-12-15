CREATE TABLE [dbo].[SoftwareModelDependencies] (
    [Id]                        UNIQUEIDENTIFIER NOT NULL,
    [ScenarioId]                UNIQUEIDENTIFIER NULL,
    [SoftwareModelId]           UNIQUEIDENTIFIER NOT NULL,
    [SoftwareModelDependencyId] UNIQUEIDENTIFIER NOT NULL,
    [Description]               NVARCHAR (500)    NOT NULL,
    CONSTRAINT [PK_SoftwareModelDependencies] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SoftwareModelDependencies_Scenarios] FOREIGN KEY ([ScenarioId]) REFERENCES [dbo].[Scenarios] ([Id]),
    CONSTRAINT [FK_SoftwareModelDependencies_SoftwareModels] FOREIGN KEY ([SoftwareModelId]) REFERENCES [dbo].[SoftwareModels] ([Id]),
    CONSTRAINT [FK_SoftwareModelDependencies_SoftwareModels1] FOREIGN KEY ([SoftwareModelDependencyId]) REFERENCES [dbo].[SoftwareModels] ([Id]),
    CONSTRAINT [IX_SoftwareModelDependencies] UNIQUE NONCLUSTERED ([SoftwareModelId] ASC, [SoftwareModelDependencyId] ASC)
);




GO

GO


GO

GO


GO

GO



CREATE UNIQUE INDEX [IX_SoftwareModelDependencies_UniqueKey] ON [dbo].[SoftwareModelDependencies] ([ScenarioId], [SoftwareModelId], [SoftwareModelDependencyId])
