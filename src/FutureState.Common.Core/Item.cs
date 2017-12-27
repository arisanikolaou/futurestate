namespace FutureState
{
    /// <summary>
    ///     Name value tuple.
    /// </summary>
    public class Item
    {
        public Item()
        {
        }

        public Item(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}