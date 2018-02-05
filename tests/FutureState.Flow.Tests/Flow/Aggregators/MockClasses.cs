using System;

namespace FutureState.Flow.Tests.Aggregators
{
    public class WholeSource 
    {
        public string Key { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool Equals(Whole other)
        {
            return string.Equals(Key, other?.Key, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class Whole : IEquatable<Whole>
    {
        public string Key { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool Equals(Whole other)
        {
            return string.Equals(Key, other?.Key, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class Part : IEquatable<Whole>
    {
        public string Key { get; set; }

        public string FirstName { get; set; }

        protected bool Equals(Part other)
        {
            return string.Equals(Key, other.Key) && string.Equals(FirstName, other.FirstName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Part)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
            }
        }

        public bool Equals(Whole other)
        {
            return string.Equals(Key, other?.Key, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class OtherPart : IEquatable<Whole>, IEquatable<OtherPart>
    {
        public string Key { get; set; }

        public string LastName { get; set; }

        public bool Equals(Whole other)
        {
            return string.Equals(Key, other?.Key, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(OtherPart other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Key, other.Key) && string.Equals(LastName, other.LastName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OtherPart)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (LastName != null ? LastName.GetHashCode() : 0);
            }
        }
    }
}
