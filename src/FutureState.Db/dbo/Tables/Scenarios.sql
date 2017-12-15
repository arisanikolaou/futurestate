CREATE TABLE [dbo].[Scenarios] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ExternalId]  NVARCHAR (100)   NOT NULL,
    [ProjectId]   UNIQUEIDENTIFIER NOT NULL,
    [DisplayName] NVARCHAR (100)   NOT NULL,
    [Description] NVARCHAR (500)   NOT NULL,
    [DateCreated] SMALLDATETIME    NOT NULL DEFAULT (getutcdate()),
    [IsInitialized] BIT NOT NULL DEFAULT 0, 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT (getutcdate()), 
    [UserName] VARCHAR(150) NOT NULL DEFAULT 'System', 
    CONSTRAINT [PK_Scenarios] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Scenarios_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]), 
    CONSTRAINT [CK_Scenarios_CheckExternalId] CHECK (Len([ExternalId]) > 1)
);

GO

CREATE UNIQUE INDEX [IX_Scenarios_UniqueScenarioKey] ON [dbo].[Scenarios] ([ProjectId], [ExternalId])
