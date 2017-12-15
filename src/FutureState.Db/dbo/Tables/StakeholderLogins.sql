CREATE TABLE [dbo].[StakeholderLogins](
	[Id] [uniqueidentifier] NOT NULL DEFAULT newid(),
    [ExternalId] NVARCHAR(150) NOT NULL, 
	[StakeholderId] [uniqueidentifier] NOT NULL,
	[LoginType] [nvarchar](50) NOT NULL DEFAULT 'Windows',
	[UserName] [nvarchar](50) NOT NULL,
	[DateAdded] [smalldatetime] NOT NULL DEFAULT getutcdate(),
	[Description] [nvarchar](500) NOT NULL,
	[DateExpired] SMALLDATETIME NULL, 
    [ScenarioId] UNIQUEIDENTIFIER NULL, 
    CONSTRAINT [PK_StakeholderLogins] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
    CONSTRAINT [FK_StakeholderLogins_ToTable] FOREIGN KEY ([ScenarioId]) REFERENCES [Scenarios]([Id]), 
    CONSTRAINT [CK_StakeholderLogins_CheckUserName] CHECK (Len([UserName]) > 0)
) ON [PRIMARY]


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The type of login' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'StakeholderLogins', @level2type=N'COLUMN',@level2name=N'LoginType'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The account name used to log in.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'StakeholderLogins', @level2type=N'COLUMN',@level2name=N'UserName'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The date the record was added.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'StakeholderLogins', @level2type=N'COLUMN',@level2name=N'DateAdded'


GO
ALTER TABLE [dbo].[StakeholderLogins]  WITH CHECK ADD  CONSTRAINT [FK_StakeholderLogins_Stakeholders] FOREIGN KEY([StakeholderId])
REFERENCES [dbo].[Stakeholders] ([Id])
GO

ALTER TABLE [dbo].[StakeholderLogins] CHECK CONSTRAINT [FK_StakeholderLogins_Stakeholders]
GO

CREATE UNIQUE INDEX [IX_StakeholderLogins_UniqueUserName] ON [dbo].[StakeholderLogins] ([ScenarioId], [StakeholderId], [LoginType], [UserName])
