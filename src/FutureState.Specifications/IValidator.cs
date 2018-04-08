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

        /// <summary>
        ///     The name of the rule to test.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Validates the subject and return true if valid and false if its not.
        /// </summary>
        bool Validate(object subject);
    }
}