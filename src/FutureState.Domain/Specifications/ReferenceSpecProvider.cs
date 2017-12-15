using FutureState.Specifications;

namespace FutureState.Domain.Specifications
{
    public class ReferenceSpecProvider : SpecProvider<Reference>
    {
        public ReferenceSpecProvider()
        {
            this.Add((item) =>
            {
                if (item.Link == null || !(item.Link.Contains("://") || item.Link.StartsWith("\\")))
                        return new SpecResult($"Reference link {item.Link} is not a url or a directory.");

                return SpecResult.Success;
            },
            "Reference Link",
            "Reference Link must be a URL or a directory link.");
        }
    }
}
