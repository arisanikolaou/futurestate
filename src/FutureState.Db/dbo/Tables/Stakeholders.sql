CREATE TABLE [dbo].[Stakeholders] (
    [Id]                UNIQUEIDENTIFIER CONSTRAINT [DF_Stakeholders_Id] DEFAULT (newid()) NOT NULL,
	[ExternalId]        NVARCHAR (150)   NOT NULL,
    [DisplayName]       NVARCHAR (50)    NOT NULL,
    [FirstName]         NVARCHAR (50)    NOT NULL,
    [LastName]          NVARCHAR (50)    NOT NULL,
    [Description]       NVARCHAR (50)    NOT NULL,
    [StakeholderTypeId] NVARCHAR (50)    NOT NULL DEFAULT 'Windows',
    [BusinessUnitId]    UNIQUEIDENTIFIER NULL,
    [DateCreated]       SMALLDATETIME    CONSTRAINT [DF__Stakehold__DateC__251C81ED] DEFAULT (getutcdate()) NOT NULL,
    [Attributes] VARCHAR(2000) NULL, 
    [DateExpired] SMALLDATETIME NULL, 
    [ScenarioId] UNIQUEIDENTIFIER NULL, 
    [UserName] NVARCHAR(50) NOT NULL DEFAULT 'System', 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    CONSTRAINT [PK_Stakeholders] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Stakeholders_StakeholderTypes] FOREIGN KEY ([StakeholderTypeId]) REFERENCES [dbo].[StakeholderTypes] ([Id])
);








GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The business unique the stakeholder is associated with' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Stakeholders', @level2type=N'COLUMN',@level2name=N'BusinessUnitId'


GO

GO


GO


CREATE UNIQUE INDEX [IX_Stakeholders_UniqueKey] ON [dbo].[Stakeholders] ([ScenarioId], [ExternalId])
