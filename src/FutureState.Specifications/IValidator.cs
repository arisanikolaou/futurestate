namespace FutureState.Specifications
{
    /// <summary>
    ///     Generic validator for a given boxed object.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        ///     The error message generated after validating the last object passed to the Validate method.
        /// </summary>
        string ErrorMessage { get; }

        string Name { get; set; }

        bool Validate(object ob);
    }
}