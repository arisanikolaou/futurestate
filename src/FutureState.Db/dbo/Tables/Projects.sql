CREATE TABLE [dbo].[Projects] (
	[Id]            UNIQUEIDENTIFIER NOT NULL,
	[DisplayName]   NVARCHAR (100)   NOT NULL,
	[DateCreated]   SMALLDATETIME    NOT NULL,
	[StartDate]     SMALLDATETIME    NOT NULL,
	[EndDate]       SMALLDATETIME    NULL,
	[Description]   NVARCHAR (500)   NOT NULL,
	[ExternalId]    NVARCHAR (100)   NOT NULL,
	[Currency] CHAR(3) NOT NULL DEFAULT ('USD'), 
	[BusinessUnitId] UNIQUEIDENTIFIER NULL, 
	[DateLastModified] SMALLDATETIME NOT NULL DEFAULT (getutcdate()), 
	[UserName] NVARCHAR(150) NOT NULL DEFAULT 'System', 
	CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([Id] ASC), 
	CONSTRAINT [CK_Projects_CheckCurrency] CHECK (Len(Currency) > 0),
	CONSTRAINT [CK_Projects_CheckExternalId] CHECK (Len(ExternalId) > 0)
);




GO

GO



CREATE UNIQUE INDEX [IX_Projects_Column] ON [dbo].[Projects] ([BusinessUnitId], [ExternalId])
