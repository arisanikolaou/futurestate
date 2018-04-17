using Newtonsoft.Json;

namespace FutureState.Services.Web
{
    /// <summary>
    ///     A generic response item.
    /// </summary>
    public class JsonResponseDto
    {
        /// <summary>
        ///     Gets the error code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        ///     Gets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Gets the business or application rules that were violated.
        /// </summary>
        public string[] Rules { get; set; }

        // other fields

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
