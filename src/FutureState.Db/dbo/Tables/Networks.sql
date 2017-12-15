CREATE TABLE [dbo].[Networks](
	[Id] [uniqueidentifier] NOT NULL,
	[ScenarioId] [uniqueidentifier] NULL,
	[DisplayName] [nvarchar](100) NOT NULL,
	[ExternalId] [nvarchar](100) NOT NULL,
	[Type] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[DateActive] [smalldatetime] NULL,
	[FixedCost] [float] NULL,
	[AnnualCost] [float] NULL,
	[DateAdded] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    [DateRemoved] SMALLDATETIME NULL, 
	[BusinessUnitId] [uniqueidentifier] NULL,
	[ParentId] UNIQUEIDENTIFIER NULL, 
	[Attributes] NVARCHAR(2000) NULL, 
	[UserName] VARCHAR(150) NOT NULL DEFAULT 'System', 
	[DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    CONSTRAINT [PK_Networks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
		CONSTRAINT [FK_Networks_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Networks]([Id])
) ON [PRIMARY]


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Expected date of activation.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Networks', @level2type=N'COLUMN',@level2name=N'DateActive'
GO

ALTER TABLE [dbo].[Networks]  WITH NOCHECK ADD  CONSTRAINT [FK_Networks_Scenarios] FOREIGN KEY([ScenarioId])
REFERENCES [dbo].[Scenarios] ([Id])
GO

ALTER TABLE [dbo].[Networks] CHECK CONSTRAINT [FK_Networks_Scenarios]
GO
ALTER TABLE [dbo].[Networks]  WITH NOCHECK ADD  CONSTRAINT [FK_Networks_BusinessUnits] FOREIGN KEY([BusinessUnitId])
REFERENCES [dbo].[BusinessUnits] ([Id])
GO

ALTER TABLE [dbo].[Networks] CHECK CONSTRAINT [FK_Networks_BusinessUnits]
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The date the network was wound up.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Networks',
    @level2type = N'COLUMN',
    @level2name = N'DateAdded'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The date the network was removed. If null the network is still active.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Networks',
    @level2type = N'COLUMN',
    @level2name = N'DateRemoved'