#region

using FutureState.Data;

#endregion

namespace FutureState.Extensions
{
    public static class UpdateResultExtensions
    {
        public static bool IsNoAction(this UpdateResult value)
        {
            return value == UpdateResult.None;
        }

        public static bool IsRemoved(this UpdateResult value)
        {
            return value == UpdateResult.Deleted;
        }

        public static bool IsSaved(this UpdateResult value)
        {
            return value == UpdateResult.Added || value == UpdateResult.Updated;
        }
    }
}