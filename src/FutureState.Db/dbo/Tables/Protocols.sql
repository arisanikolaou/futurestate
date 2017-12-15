CREATE TABLE [dbo].[Protocols] (
    [Id]             UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [ScenarioId]     UNIQUEIDENTIFIER NULL,
    [ProtocolTypeId] NVARCHAR (50)    NULL DEFAULT 'Http',
    [DisplayName]    NVARCHAR (100)   NOT NULL,
    [ExternalId]     NVARCHAR (150)    NOT NULL,
    [Description]    NVARCHAR (500)   NOT NULL,
    [ParentId]       UNIQUEIDENTIFIER NULL,
    [Version]        VARCHAR(20)   NULL,
    [UserName] NVARCHAR(100) NOT NULL DEFAULT 'System', 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    CONSTRAINT [PK_Protocols] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Protocols_Protocols] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Protocols] ([Id]),
    CONSTRAINT [FK_Protocols_ProtocolTypes] FOREIGN KEY ([ProtocolTypeId]) REFERENCES [dbo].[ProtocolTypes] ([Id])
);






GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Protocols and schemas ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Protocols'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Protocol stereotype' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Protocols', @level2type=N'COLUMN',@level2name=N'ProtocolTypeId'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Full display name of the protocol.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Protocols', @level2type=N'COLUMN',@level2name=N'DisplayName'


GO



GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A description of the protocol.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Protocols', @level2type=N'COLUMN',@level2name=N'Description'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The protocol that contains this instance.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Protocols', @level2type=N'COLUMN',@level2name=N'ParentId'


GO

GO


GO

GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Short code for the protocol.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Protocols', @level2type = N'COLUMN', @level2name = N'ExternalId';

