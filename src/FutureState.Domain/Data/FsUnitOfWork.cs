using FutureState.Data;
using System;

namespace FutureState.Domain.Data
{
    /// <summary>
    ///     Future State database.
    /// </summary>
    public class FsUnitOfWork
        : UnitOfWorkLinq<ApplicationAccess, Guid>, IUnitOfWorkLinq<Device, Guid>, 
        IUnitOfWorkLinq<DeviceModelDependency, Guid>, IUnitOfWorkLinq<DeviceModel, Guid>, 
        IUnitOfWorkLinq<SoftwareModel, Guid>, IUnitOfWorkLinq<SoftwareModelDependency, Guid>, 
        IUnitOfWorkLinq<SoftwareModelInterface, Guid>, IUnitOfWorkLinq<Stakeholder, Guid>, IUnitOfWorkLinq<BusinessUnit, Guid>, 
        IUnitOfWorkLinq<StakeholderLogin, Guid>, IUnitOfWorkLinq<UserGroup, Guid>, IUnitOfWorkLinq<UserGroupMembership, Guid>,
        IUnitOfWorkLinq<Reference, Guid>, IUnitOfWorkLinq<Project, Guid>, IUnitOfWorkLinq<Scenario, Guid>, 
        IUnitOfWorkLinq<Capability, Guid>, IUnitOfWorkLinq<DesignDomain, Guid>, IUnitOfWorkLinq<LifeCycle, string>, 
        IUnitOfWorkLinq<Policy, Guid>, IUnitOfWorkLinq<ProtocolType, string>, IUnitOfWorkLinq<Protocol, Guid>,
        IUnitOfWorkLinq<DeviceConnection, Guid>, IUnitOfWorkLinq<Network, Guid>
    {
        public FsUnitOfWork(
            ISessionFactory sessionFactory,
            Func<ISession, IRepositoryLinq<BusinessUnit, Guid>> getBusinessUnits,
            Func<ISession, IRepositoryLinq<DesignDomain, Guid>> getDesignDomains,
            Func<ISession, IRepositoryLinq<LifeCycle, string>> getLifeCycles,
            Func<ISession, IRepositoryLinq<Policy, Guid>> getPolicies,
            Func<ISession, IRepositoryLinq<Project, Guid>> getProjects,
            Func<ISession, IRepositoryLinq<Scenario, Guid>> getScenarios,
            Func<ISession, IRepositoryLinq<ProtocolType, string>> getProtocolTypes,
            Func<ISession, IRepositoryLinq<Protocol, Guid>> getProtocols,
            Func<ISession, IRepositoryLinq<Stakeholder, Guid>> getStakeholders,
            Func<ISession, IRepositoryLinq<StakeholderLogin, Guid>> getStakeholderLogins,
            Func<ISession, IRepositoryLinq<UserGroup, Guid>> userGroupsGet,
            Func<ISession, IRepositoryLinq<UserGroupMembership, Guid>> getUserGroupMemberships,
            Func<ISession, IRepositoryLinq<SoftwareModel, Guid>> getSoftwareModels,
            Func<ISession, IRepositoryLinq<SoftwareModelDependency, Guid>> getSoftwareModelDependency,
            Func<ISession, IRepositoryLinq<SoftwareModelInterface, Guid>> getSoftwareModelInterfaces,
            Func<ISession, IRepositoryLinq<Device, Guid>> getDevices,
            Func<ISession, IRepositoryLinq<DeviceModelDependency, Guid>> getDeviceModelDependencies,
            Func<ISession, IRepositoryLinq<DeviceModel, Guid>> getDeviceModels,
            Func<ISession, IRepositoryLinq<DeviceConnection, Guid>> getDeviceConnections,
            Func<ISession, IRepositoryLinq<ApplicationAccess, Guid>> getApplicationAccess,
            Func<ISession, IRepositoryLinq<Reference, Guid>> getRefererences,
            Func<ISession, IRepositoryLinq<Capability, Guid>> getCapabilities,
            Func<ISession, IRepositoryLinq<Network, Guid>> getNetworks,
            ICommitPolicy commitPolicy) : base(getApplicationAccess, sessionFactory, commitPolicy)
        {
            this.Networks = new EntitySetLinq<Network, Guid>(this, getNetworks);
            this.Protocols = new EntitySetLinq<Protocol, Guid>(this, getProtocols);
            this.ProtocolTypes = new EntitySetLinq<ProtocolType, string>(this, getProtocolTypes);
            this.Policies = new EntitySetLinq<Policy, Guid>(this, getPolicies);
            this.Capabilities = new EntitySetLinq<Capability, Guid>(this, getCapabilities);
            this.LifeCycles = new EntitySetLinq<LifeCycle, string>(this, getLifeCycles);
            this.Projects = new EntitySetLinq<Project, Guid>(this, getProjects);
            this.Scenarios = new EntitySetLinq<Scenario, Guid>(this, getScenarios);
            this.DesignDomains = new EntitySetLinq<DesignDomain, Guid>(this, getDesignDomains);
            this.Devices = new EntitySetLinq<Device, Guid>(this, getDevices);
            this.DeviceModelDependencies = new EntitySetLinq<DeviceModelDependency, Guid>(this, getDeviceModelDependencies);
            this.DeviceModels = new EntitySetLinq<DeviceModel, Guid>(this, getDeviceModels);
            this.DeviceConnections = new EntitySetLinq<DeviceConnection, Guid>(this, getDeviceConnections);
            this.Stakeholders = new EntitySetLinq<Stakeholder, Guid>(this, getStakeholders);
            this.SoftwareModels = new EntitySetLinq<SoftwareModel, Guid>(this, getSoftwareModels);
            this.SoftwareModelInterfaces = new EntitySetLinq<SoftwareModelInterface, Guid>(this, getSoftwareModelInterfaces);
            this.SoftwareModelDependencies = new EntitySetLinq<SoftwareModelDependency, Guid>(this, getSoftwareModelDependency);
            this.StakeholderLogins = new EntitySetLinq<StakeholderLogin, Guid>(this, getStakeholderLogins);
            this.UserGroupMemberships = new EntitySetLinq<UserGroupMembership, Guid>(this, getUserGroupMemberships);
            this.UserGroups = new EntitySetLinq<UserGroup, Guid>(this, userGroupsGet);
            this.References = new EntitySetLinq<Reference, Guid>(this, getRefererences);
            this.ApplicationAccess = new EntitySetLinq<ApplicationAccess, Guid>(this, getApplicationAccess);
            this.BusinessUnits = new EntitySetLinq<BusinessUnit, Guid>(this, getBusinessUnits);
        }

        public EntitySetLinq<DeviceModel, Guid> DeviceModels { get;  }
        public EntitySetLinq<DeviceModelDependency, Guid> DeviceModelDependencies { get; }
        public EntitySetLinq<Device, Guid> Devices { get;}
        public EntitySetLinq<Stakeholder, Guid> Stakeholders { get;}
        public EntitySetLinq<SoftwareModelInterface, Guid> SoftwareModelInterfaces { get;}
        public EntitySetLinq<SoftwareModelDependency, Guid> SoftwareModelDependencies { get;}
        public EntitySetLinq<SoftwareModel, Guid> SoftwareModels { get;}
        public EntitySetLinq<StakeholderLogin, Guid> StakeholderLogins { get;}
        public EntitySetLinq<BusinessUnit, Guid> BusinessUnits { get;}
        public EntitySetLinq<UserGroupMembership, Guid> UserGroupMemberships { get;}
        public EntitySetLinq<UserGroup, Guid> UserGroups { get;}
        public EntitySetLinq<Reference, Guid> References { get; }
        public EntitySetLinq<Project, Guid> Projects { get; private set; }
        public EntitySetLinq<Scenario, Guid> Scenarios { get; private set; }
        public EntitySetLinq<Capability, Guid> Capabilities { get; private set; }
        public EntitySetLinq<DesignDomain, Guid> DesignDomains { get; private set; }
        public EntitySetLinq<LifeCycle, string> LifeCycles { get; private set; }
        public EntitySetLinq<Policy, Guid> Policies { get; private set; }
        public EntitySetLinq<Protocol, Guid> Protocols { get; private set; }
        public EntitySetLinq<ProtocolType, string> ProtocolTypes { get; private set; }
        public EntitySetLinq<DeviceConnection, Guid> DeviceConnections { get; private set; }
        public EntitySetLinq<Network, Guid> Networks { get; private set; }
        public EntitySetLinq<ApplicationAccess, Guid> ApplicationAccess { get; private set; }

        EntitySetLinq<Network, Guid> IUnitOfWorkLinq<Network, Guid>.EntitySet => Networks;

        EntitySetLinq<Device, Guid> IUnitOfWorkLinq<Device, Guid>.EntitySet => Devices;

        EntitySetLinq<DeviceModelDependency, Guid> IUnitOfWorkLinq<DeviceModelDependency, Guid>.EntitySet => DeviceModelDependencies;

        EntitySetLinq<DeviceModel, Guid> IUnitOfWorkLinq<DeviceModel, Guid>.EntitySet => DeviceModels;

        EntitySetLinq<SoftwareModelDependency, Guid> IUnitOfWorkLinq<SoftwareModelDependency, Guid>.EntitySet => SoftwareModelDependencies;

        EntitySetLinq<SoftwareModelInterface, Guid> IUnitOfWorkLinq<SoftwareModelInterface, Guid>.EntitySet => SoftwareModelInterfaces;

        EntitySetLinq<Stakeholder, Guid> IUnitOfWorkLinq<Stakeholder, Guid>.EntitySet => Stakeholders;

        EntitySetLinq<SoftwareModel, Guid> IUnitOfWorkLinq<SoftwareModel, Guid>.EntitySet => SoftwareModels;

        EntitySetLinq<BusinessUnit, Guid> IUnitOfWorkLinq<BusinessUnit, Guid>.EntitySet => BusinessUnits;

        EntitySetLinq<StakeholderLogin, Guid> IUnitOfWorkLinq<StakeholderLogin, Guid>.EntitySet => this.StakeholderLogins;

        EntitySetLinq<UserGroup, Guid> IUnitOfWorkLinq<UserGroup, Guid>.EntitySet => UserGroups;

        EntitySetLinq<UserGroupMembership, Guid> IUnitOfWorkLinq<UserGroupMembership, Guid>.EntitySet => UserGroupMemberships;

        EntitySetLinq<Reference, Guid> IUnitOfWorkLinq<Reference, Guid>.EntitySet => this.References;

        EntitySetLinq<Project, Guid> IUnitOfWorkLinq<Project, Guid>.EntitySet => this.Projects;

        EntitySetLinq<Scenario, Guid> IUnitOfWorkLinq<Scenario, Guid>.EntitySet => this.Scenarios;

        EntitySetLinq<Capability, Guid> IUnitOfWorkLinq<Capability, Guid>.EntitySet => this.Capabilities;

        EntitySetLinq<DesignDomain, Guid> IUnitOfWorkLinq<DesignDomain, Guid>.EntitySet => DesignDomains;

        EntitySetLinq<LifeCycle, string> IUnitOfWorkLinq<LifeCycle, string>.EntitySet => LifeCycles;

        EntitySetLinq<Policy, Guid> IUnitOfWorkLinq<Policy, Guid>.EntitySet => this.Policies;

        EntitySetLinq<ProtocolType, string> IUnitOfWorkLinq<ProtocolType, string>.EntitySet => ProtocolTypes;

        EntitySetLinq<Protocol, Guid> IUnitOfWorkLinq<Protocol, Guid>.EntitySet => Protocols;

        EntitySetLinq<DeviceConnection, Guid> IUnitOfWorkLinq<DeviceConnection, Guid>.EntitySet => DeviceConnections;
    }   
}