namespace System
{
    public static class DateExtensions
    {
        public static bool IsNullOrEmpty(this DateTime obj)
        {
            return obj.IsNull() || obj == DateTime.MinValue;
        }

        public static bool IsNotNullOrEmpty(this DateTime obj)
        {
            return !obj.IsNullOrEmpty();
        }

        public static bool IsNullOrEmpty(this DateTime? obj)
        {
            return obj.IsNull() || obj == DateTime.MinValue;
        }

        public static bool IsNotNullOrEmpty(this DateTime? obj)
        {
            return !obj.IsNullOrEmpty();
        }

        public static string ToUserFriendlyString(this TimeSpan? time)
        {
            var timeSpanStr = "";
            if (time.HasValue)
            {
                var hours = time.Value.Hours;
                var minutes = time.Value.Minutes;
                var seconds = time.Value.Seconds;
                if (hours > 0)
                {
                    timeSpanStr += $"{hours} saat";
                }
                if (minutes > 0)
                {
                    timeSpanStr += $" {minutes} dakika";
                }
                if (seconds > 0)
                {
                    timeSpanStr += $" {seconds} saniye";
                }
            }

            return timeSpanStr.Trim();
        }
    }
}
