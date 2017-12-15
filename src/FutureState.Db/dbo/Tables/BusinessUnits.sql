CREATE TABLE [dbo].[BusinessUnits](
	[Id] [uniqueidentifier] NOT NULL,
	[DisplayName] [nvarchar](100) NOT NULL,
	[ExternalId] [nvarchar](20) NOT NULL,
	[DateActive] [smalldatetime] NOT NULL,
	[DateRetired] [smalldatetime] NULL,
	[Currency] [char](3) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[ParentId] [uniqueidentifier] NULL,
	[Attributes] NVARCHAR(2000) NULL, 
    [UserName] VARCHAR(150) NOT NULL DEFAULT 'System', 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT (getutcdate()), 
    CONSTRAINT [PK_BusinessUnits] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


GO
ALTER TABLE [dbo].[BusinessUnits] ADD  CONSTRAINT [DF_BusinessUnits_Currency]  DEFAULT ('USD') FOR [Currency]
GO
ALTER TABLE [dbo].[BusinessUnits] ADD  CONSTRAINT [DF_BusinessUnits_DateActive]  DEFAULT (getutcdate()) FOR [DateActive]
GO
ALTER TABLE [dbo].[BusinessUnits]  WITH NOCHECK ADD  CONSTRAINT [FK_BusinessUnits_BusinessUnits] FOREIGN KEY([ParentId])
REFERENCES [dbo].[BusinessUnits] ([Id])
GO

ALTER TABLE [dbo].[BusinessUnits] CHECK CONSTRAINT [FK_BusinessUnits_BusinessUnits]
GO
ALTER TABLE [dbo].[BusinessUnits] ADD  CONSTRAINT [DF_BusinessUnits_Id]  DEFAULT (newid()) FOR [Id]
GO

CREATE UNIQUE INDEX [IX_BusinessUnits_Column] ON [dbo].[BusinessUnits] ([ExternalId])
