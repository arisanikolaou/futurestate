using FutureState.Data;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    public class ProtocolType : IEntityMutableKey<string>
    {
        [Key]
        [StringLength(100)]
        [Required]
        public string Id { get; set; }

        /// <summary>
        ///     Api call.
        /// </summary>
        public const string Api = "Api";

        /// <summary>
        ///     Schema.
        /// </summary>
        public const string Schema = "Schema";

        /// <summary>
        ///     A restful service.
        /// </summary>
        public const string RestService = "RestService";

        /// <summary>
        ///     Unspecified protocol.
        /// </summary>
        public const string Unspecified = "Unspecified";
    }
}