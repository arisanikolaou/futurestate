namespace FutureState
{
    /// <summary>
    ///     Name value tuple.
    /// </summary>
    public class Item
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Item()
        {

        }

        public Item(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
