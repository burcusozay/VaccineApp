namespace System
{
    public static class GuidExtensions
    {
        public static bool IsNullOrEmpty(this Guid obj)
        {
            return obj.IsNull()||obj==Guid.Empty;
        }

        public static bool IsNotNullOrEmpty(this Guid obj)
        {
            return !obj.IsNullOrEmpty();
        }

        public static bool IsNullOrEmpty(this Guid? obj)
        {
            return obj.IsNull() || obj == Guid.Empty;
        }

        public static bool IsNotNullOrEmpty(this Guid? obj)
        {
            return !obj.IsNullOrEmpty();
        }
    }
}
