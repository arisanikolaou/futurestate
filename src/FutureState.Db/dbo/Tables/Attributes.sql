CREATE TABLE [dbo].[Attributes](
	[Id] [uniqueidentifier] NOT NULL,
	[ScenarioId] [uniqueidentifier] NOT NULL,
	[AssetId] [uniqueidentifier] NOT NULL,
	[AssetTypeId] [varchar](100) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Value] [varchar](200) NOT NULL,
 CONSTRAINT [PK_Attributes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Id of the enterprise asset' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Attributes', @level2type=N'COLUMN',@level2name=N'AssetId'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Its well known asset type' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Attributes', @level2type=N'COLUMN',@level2name=N'AssetTypeId'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The name of the attribute' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Attributes', @level2type=N'COLUMN',@level2name=N'Name'


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The attribute value.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Attributes', @level2type=N'COLUMN',@level2name=N'Value'


GO
ALTER TABLE [dbo].[Attributes]  WITH NOCHECK ADD  CONSTRAINT [FK_Attributes_AssetTypes] FOREIGN KEY([AssetTypeId])
REFERENCES [dbo].[AssetTypes] ([Id])
GO

ALTER TABLE [dbo].[Attributes] CHECK CONSTRAINT [FK_Attributes_AssetTypes]