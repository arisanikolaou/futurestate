CREATE TABLE [dbo].[AssetTypes](
	[Id] [varchar](100) NOT NULL,
 CONSTRAINT [PK_AssetTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A record of well known asset types.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AssetTypes'

