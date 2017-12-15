using FutureState.Data;
using System;
using System.Collections.Generic;

namespace FutureState.Domain
{
    /// <summary>
    ///     A generic organizational container for a set of users as well as
    ///     other groups. 
    /// </summary>
    public class UserGroup : FSEntity, IEntityMutableKey<Guid>, IDesignArtefact, IAuditable
    {
        /// <summary>
        ///     Gets the name of the user that last modifeid the record.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets/sets the scenario the user group is associated with.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets/sets the date the record was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public UserGroup()
        {
            // required by serializers
        }

        public UserGroup(string displayName, string description)
        {
            this.Id = SeqGuid.Create();

            this.DisplayName = displayName;
            this.Description = description ?? "";
            this.DateLastModified = DateTime.UtcNow;
            this.DateCreated = DateLastModified;
            this.UserName = "";
        }

        /// <summary>
        ///     Gets the services to navigate to the nested stakeholders
        ///     or enumerate its members.
        /// </summary>
        public DomainServices Services { get; private set; }

        /// <summary>
        ///     Sets the services for the instance.
        /// </summary>
        public void SetServices(DomainServices services) => Services = services;


        public class DomainServices
        {
            private readonly Func<IEnumerable<Stakeholder>> _membersGet;

            public DomainServices(
                Func<IEnumerable<Stakeholder>> membersGet)
            {
                _membersGet = membersGet;
            }

            public IEnumerable<Stakeholder> GetMembers() => _membersGet();
        }
    }
}
