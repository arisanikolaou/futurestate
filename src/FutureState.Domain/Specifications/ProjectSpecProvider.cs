using FutureState.Specifications;

namespace FutureState.Domain.Specifications
{
    public class ProjectSpecProvider : SpecProvider<Project>
    {
        public ProjectSpecProvider()
        {
            this.Add((item) =>
            {
                if (item.EndDate.HasValue)
                    if (item.StartDate > item.EndDate)
                        return new SpecResult($"Date Added {item.EndDate} cannot be a date in the future from Date Removed {item.StartDate}.");

                return SpecResult.Success;
            },
            "Project End Date",
            "A project cannot end before it starts.");

            this.Add((item) =>
            {
                if (item.Currency == null || item.Currency.Length != 3)
                        if (item.StartDate > item.EndDate)
                    return new SpecResult($"Project currency {item.Currency} must be a three letter ISO currency code.");


                return SpecResult.Success;
            },
            "Project Currency",
            "A project must have a base three letter currentcy.");
        }
    }
}
