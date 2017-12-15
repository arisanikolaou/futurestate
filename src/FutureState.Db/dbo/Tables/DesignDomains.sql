CREATE TABLE [dbo].[DesignDomains](
	[Id] [uniqueidentifier] NOT NULL,
	[DisplayName] [nvarchar](100) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[ParentId] [uniqueidentifier] NULL,
	[Description] NVARCHAR(500) NOT NULL, 
    [DateCreated] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    [ExternalId] NVARCHAR(150) NOT NULL, 
    [UserName] VARCHAR(150) NOT NULL DEFAULT 'System', 
    [DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
    CONSTRAINT [PK_Domains] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


GO


ALTER TABLE [dbo].[DesignDomains] ADD  CONSTRAINT [DF_Domains_Id]  DEFAULT (newid()) FOR [Id]
GO

CREATE UNIQUE INDEX [IX_DesignDomains_UniqueRow] ON [dbo].[DesignDomains] ([ExternalId], [ParentId])

GO

CREATE UNIQUE INDEX [IX_DesignDomains_UniqueByExternalId] ON [dbo].[DesignDomains] ([ExternalId])
