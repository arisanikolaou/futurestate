/*
Post-Deployment Script Template
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.
 Use SQLCMD syntax to include a file in the post-deployment script.
 Example:      :r .\myfile.sql
 Use SQLCMD syntax to reference a variable in the post-deployment script.
 Example:      :setvar TableName MyTable
			   SELECT * FROM [$(TableName)]
--------------------------------------------------------------------------------------
*/

INSERT INTO [dbo].[DeviceTypes] ([Id]) VALUES (N'Firewall'),
	(N'Other'),
	(N'Server'),
	(N'Workstation')

INSERT INTO [dbo].[StakeholderTypes] ([Id]) VALUES (N'System Account'),
	(N'User'),
	(N'User Group')

INSERT INTO [dbo].[AssetTypes] ([Id]) VALUES (N'DeviceModel'),
	(N'Network'),
	(N'Software'),
	(N'SoftwareModel'),
	(N'Stakeholder')

INSERT INTO [dbo].[LifeCycles] ([Id]) VALUES (N'Exit'),
	(N'Invest'),
	(N'Maintain'),
	(N'Research')

INSERT INTO [dbo].[ProtocolTypes] ([Id]) VALUES (N'Other'),
	(N'File'),
	(N'Http'),
	(N'Https'),
	(N'REST'),
	(N'SMB'),
	(N'Tcp')

INSERT INTO [dbo].[_Version] 
([Version], [DateModified])
VALUES (N'1.0.0.0', N'2010-01-01 00:00:00')

INSERT INTO [dbo].[BusinessUnits] 
([Id], [DisplayName], [ExternalId], [DateActive], [DateRetired], [Currency], [Description], [ParentId])
VALUES (N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66e', N'Main', N'MN', N'2000-01-01 00:00:00', NULL, N'USD', N'', NULL)

INSERT INTO [dbo].[Stakeholders] 
([Id], [DisplayName], [FirstName], [LastName], [Description], [StakeholderTypeId], [BusinessUnitId],[ExternalId],[DateCreated])
VALUES (N'a838689d-8f86-4038-9327-a6a5f6deb736', N'Admin', N'', N'', N'', N'System Account',  N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66e','SH-000','2010-1-1')

INSERT INTO [dbo].[Projects] ([Id],[ExternalId], [DisplayName], [StartDate], [DateCreated], [Description])
VALUES (N'a838689d-8f86-4038-9327-a6a5f6deb738','PR-00', N'Main', getutcdate(), N'2000-01-01 00:00:00', N'Main project')

INSERT INTO [dbo].[Scenarios] ([Id], [ProjectId], [DisplayName], [ExternalId], [Description], [DateCreated])
VALUES (N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66f', N'a838689d-8f86-4038-9327-a6a5f6deb738', N'MN-S', N'MN-S',  N'Default scenario', N'2000-01-01 00:00:00')

INSERT INTO [dbo].[DesignDomains] ([Id], [ExternalId], [DisplayName], [Type], [ParentId], [Description])
VALUES	(N'37585467-b8a1-4754-b637-2a7e1838d131', 'DD-01', N'Software', N'Technical', NULL, N''),
		(N'37585467-b8a1-4754-b637-2a7e1838d141', 'DD-02', N'Hardware', N'Technical', NULL, N'')

INSERT INTO [dbo].[Protocols] 
([Id], [ScenarioId], [ProtocolTypeId], [DisplayName], [ExternalId], [Description], [ParentId], [Version]) 
VALUES (N'3e369066-30f5-4e0f-8a8a-5be48b8ca66f', N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66f', N'Http', N'HttpProtocol', N'HTTP-2', N'Sample http protocol', NULL, N'2.0.0')

INSERT INTO [dbo].[SoftwareModels] 
([Id], [ScenarioId], [ExternalId], [DisplayName], [Description], [SoftwareModelDate], [DomainId], [Vendor], [LicenseTypeId], [LicenseReferenceCode], [FixedCost], [AnnualCost], [LifeCycleId], [LifeCycleStageDate], [Version], [Attributes]) 
VALUES (N'90f74beb-3920-476b-8f8f-737927eec921', N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66f',  N'SM-1', N'Software 1', N'Some description', N'2010-01-01', N'37585467-b8a1-4754-b637-2a7e1838d131', N'Microsoft', N'Leased', N'Reference Code', 0, 0, N'Invest', N'2010-01-01 00:00:00', N'1.0.0', NULL)

-- insert software model interfaces
INSERT INTO [dbo].[SoftwareModelInterfaces] 
([Id], [ScenarioId], [SoftwareModelId], [DisplayName], [ProtocolId], [Description], [SensitivityLevel], [DateCreated],[ExternalId]) 
VALUES (N'38585467-b8a1-4754-b637-2a7e1878d141', N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66f', N'90f74beb-3920-476b-8f8f-737927eec921', N'Interface 1', NULL, N'Some Description', 1, N'2010-01-01 00:00:00','SMI-00')

INSERT INTO [dbo].[DeviceModels] 
([Id], [ScenarioId], [DisplayName], [ModelVersion], [Description], [ExternalId], [DomainId], [AnnualCost], [FixedCost], [LifeCycleId], [LifeCycleStageDate], [Attributes], [DateCreated]) 
VALUES (N'90f74beb-3920-476b-8f8f-737927efc921', N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66f', N'Device Model 1', N'1.0.0', N'Reference Model', N'001', N'37585467-b8a1-4754-b637-2a7e1838d141', 0, 0, N'Invest', N'2010-01-01 00:00:00', NULL, N'2010-01-01 00:00:00')

INSERT INTO [dbo].[Networks] 
([Id], [ScenarioId], [DisplayName], [ExternalId], [Type], [Description], [DateActive], [FixedCost], [AnnualCost], [BusinessUnitId], [ParentId], [Attributes]) 
VALUES (N'3e369066-30f5-4e0f-8a8a-9ae48b8ca66f', N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66f', N'Network 1', N'NET01', N'Network', N'Network Description', N'2010-01-01 00:00:00',0, 0, N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66e', NULL, NULL)

INSERT INTO [dbo].[Devices] ([Id], [ScenarioId], [DeviceModelId], [NetworkId], [DisplayName], [Description], [DateAdded], [FixedCost], [AnnualCost], [ExternalId], [Attributes], [AvailabilityTier], [DateRemoved]) 
VALUES	(N'37585467-b8a1-4754-b637-2a7e1878d141', N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66f', N'90f74beb-3920-476b-8f8f-737927efc921', N'3e369066-30f5-4e0f-8a8a-9ae48b8ca66f', N'Device', N'Device Description', N'2017-02-24 22:40:00',  0, 0, N'DEV1', NULL, N'Silver', N'2010-01-01 00:00:00'),
		(N'37585467-b8a1-4754-b637-2a7e1878d142', N'3e369066-30f5-4e0f-8a8a-5ae48b8ca66f', N'90f74beb-3920-476b-8f8f-737927efc921', N'3e369066-30f5-4e0f-8a8a-9ae48b8ca66f', N'Device', N'Device Description', N'2017-02-24 22:40:00',  0, 0, N'DEV1', NULL, N'Silver', N'2010-01-01 00:00:00')

-- insert software model interfaces