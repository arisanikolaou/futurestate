CREATE TABLE [dbo].[Policies] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [BusinessUnitId] UNIQUEIDENTIFIER NULL,
    [DisplayName]    NVARCHAR (100)    NOT NULL,
    [Description]    NVARCHAR (500)   NOT NULL,
    [DateAdded]      SMALLDATETIME    NOT NULL,
    [DateRemoved]    SMALLDATETIME    NULL,
    [ContainerId]    UNIQUEIDENTIFIER NULL,
    [UserName] NVARCHAR(100) NOT NULL DEFAULT 'System', 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    [ExternalId] NVARCHAR(150) NOT NULL, 
    [Votes] INT NULL, 
    [DesignDomainId] UNIQUEIDENTIFIER NULL, 
    [LastReviewDate] DATE NULL, 
    [LastReviewNotes] VARCHAR(150) NULL, 
    CONSTRAINT [PK_Policies] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Policies_BusinessUnits] FOREIGN KEY ([BusinessUnitId]) REFERENCES [dbo].[BusinessUnits] ([Id]), 
    CONSTRAINT [FK_Policies_PolicyId] FOREIGN KEY ([ContainerId]) REFERENCES [Policies]([Id]), 
    CONSTRAINT [FK_Policies_DesignDomains] FOREIGN KEY ([DesignDomainId]) REFERENCES [DesignDomains]([Id])
);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The policy that owns or contains the current instance.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Policies',
    @level2type = N'COLUMN',
    @level2name = N'ContainerId'
GO

CREATE UNIQUE INDEX [IX_Policies_Column] ON [dbo].[Policies] ([ExternalId], [BusinessUnitId])

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The design domain the policy applies to e.g. hardware/software.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Policies',
    @level2type = N'COLUMN',
    @level2name = N'DesignDomainId'