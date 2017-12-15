CREATE TABLE [dbo].[UserGroupMemberships] (
	[Id]       UNIQUEIDENTIFIER NOT NULL,
	[GroupId] UNIQUEIDENTIFIER NOT NULL,
	[MemberId] UNIQUEIDENTIFIER NOT NULL,
	[ScenarioId] UNIQUEIDENTIFIER NULL, 
	[DateAdded] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
	[UserName] NVARCHAR(150) NOT NULL DEFAULT 'System', 
	[DateLastModified] SMALLDATETIME NOT NULL DEFAULT getutcdate(), 
	CONSTRAINT [PK_UserGroupMemberships] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [IX_UserGroupMemberships] UNIQUE NONCLUSTERED ([ScenarioId] ASC, [GroupId] ASC, [MemberId] ASC), 
	CONSTRAINT [FK_UserGroupMemberships_ToUserGroups] FOREIGN KEY ([GroupId]) REFERENCES [UserGroups]([Id]), 
	CONSTRAINT [FK_UserGroupMemberships_ToScenarios] FOREIGN KEY ([ScenarioId]) REFERENCES [Scenarios]([Id]),
	CONSTRAINT [FK_UserGroupMemberships_ToStakeholders] FOREIGN KEY ([MemberId]) REFERENCES [Stakeholders]([Id])
);




GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The row id', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserGroupMemberships', @level2type = N'COLUMN', @level2name = N'Id';


GO
EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'The date the user was added to the group in utc.',
	@level0type = N'SCHEMA',
	@level0name = N'dbo',
	@level1type = N'TABLE',
	@level1name = N'UserGroupMemberships',
	@level2type = N'COLUMN',
	@level2name = N'DateAdded'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'The design scenario being evaluated.',
	@level0type = N'SCHEMA',
	@level0name = N'dbo',
	@level1type = N'TABLE',
	@level1name = N'UserGroupMemberships',
	@level2type = N'COLUMN',
	@level2name = N'ScenarioId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'The stakeholder that belongs to the group.',
	@level0type = N'SCHEMA',
	@level0name = N'dbo',
	@level1type = N'TABLE',
	@level1name = N'UserGroupMemberships',
	@level2type = N'COLUMN',
	@level2name = N'MemberId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'The group (also a stakeholder).',
	@level0type = N'SCHEMA',
	@level0name = N'dbo',
	@level1type = N'TABLE',
	@level1name = N'UserGroupMemberships',
	@level2type = N'COLUMN',
	@level2name = N'GroupId'
GO

CREATE UNIQUE INDEX [IX_UserGroupMemberships_UniqueMembership] ON [dbo].[UserGroupMemberships] ([GroupId], [MemberId], [ScenarioId])
