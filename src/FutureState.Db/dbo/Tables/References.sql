CREATE TABLE [dbo].[References] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ReferenceId] UNIQUEIDENTIFIER NULL,
    [Link]        NVARCHAR (300)   NOT NULL,
    [Description] NVARCHAR (500)   NOT NULL,
    [ScenarioId] UNIQUEIDENTIFIER NULL, 
    CONSTRAINT [PK_References] PRIMARY KEY CLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_References_ScenarioId] FOREIGN KEY ([ScenarioId]) REFERENCES [Scenarios] ( [Id] ), 
    CONSTRAINT [CK_References_CheckLink] CHECK (Len(Link) > 0)
);

GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The url link' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'References', @level2type=N'COLUMN',@level2name=N'Link'


GO

CREATE UNIQUE INDEX [IX_References_UniqueLink] ON [dbo].[References] ([ReferenceId], [ScenarioId], [Link])
