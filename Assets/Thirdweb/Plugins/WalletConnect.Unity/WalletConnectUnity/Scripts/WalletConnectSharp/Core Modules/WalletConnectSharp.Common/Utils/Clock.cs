using System;

namespace WalletConnectSharp.Common.Utils
{
    /// <summary>
    /// A helper class to help with clock related things
    /// </summary>
    public static class Clock
    {
        /// <summary>
        /// One second, in seconds
        /// </summary>
        public const long ONE_SECOND = 1;

        /// <summary>
        /// Five seconds, in seconds
        /// </summary>
        public const long FIVE_SECONDS = 5;

        /// <summary>
        /// Ten seconds, in seconds
        /// </summary>
        public const long TEN_SECONDS = 10;

        /// <summary>
        /// Thirty seconds, in seconds
        /// </summary>
        public const long THIRTY_SECONDS = 30;

        /// <summary>
        /// Sixty seconds, in seconds
        /// </summary>
        public const long SIXTY_SECONDS = 60;

        /// <summary>
        /// One minute, in seconds
        /// </summary>
        public const long ONE_MINUTE = SIXTY_SECONDS;

        /// <summary>
        /// Five minutes, in seconds
        /// </summary>
        public const long FIVE_MINUTES = ONE_MINUTE * 5;

        /// <summary>
        /// Ten minutes, in seconds
        /// </summary>
        public const long TEN_MINUTES = ONE_MINUTE * 10;

        /// <summary>
        /// Thirty minutes, in seconds
        /// </summary>
        public const long THIRTY_MINUTES = ONE_MINUTE * 30;

        /// <summary>
        /// Sixty minutes, in seconds
        /// </summary>
        public const long SIXTY_MINUTES = ONE_MINUTE * 60;

        /// <summary>
        /// One hour, in seconds
        /// </summary>
        public const long ONE_HOUR = SIXTY_MINUTES;

        /// <summary>
        /// Three hours, in seconds
        /// </summary>
        public const long THREE_HOURS = ONE_HOUR * 3;

        /// <summary>
        /// Six hours, in seconds
        /// </summary>
        public const long SIX_HOURS = ONE_HOUR * 6;

        /// <summary>
        /// Twelve hours, in seconds
        /// </summary>
        public const long TWELVE_HOURS = ONE_HOUR * 12;

        /// <summary>
        /// Twenty four hours, in seconds
        /// </summary>
        public const long TWENTY_FOUR_HOURS = ONE_HOUR * 24;

        /// <summary>
        /// One day, in seconds
        /// </summary>
        public const long ONE_DAY = TWENTY_FOUR_HOURS;

        /// <summary>
        /// Three days, in seconds
        /// </summary>
        public const long THREE_DAYS = ONE_DAY * 3;

        /// <summary>
        /// Five days, in seconds
        /// </summary>
        public const long FIVE_DAYS = ONE_DAY * 5;

        /// <summary>
        /// Seven days, in seconds
        /// </summary>
        public const long SEVEN_DAYS = ONE_DAY * 7;

        /// <summary>
        /// Thirty days, in seconds
        /// </summary>
        public const long THIRTY_DAYS = ONE_DAY * 30;

        /// <summary>
        /// One week, in seconds
        /// </summary>
        public const long ONE_WEEK = SEVEN_DAYS;

        /// <summary>
        /// Two weeks, in seconds
        /// </summary>
        public const long TWO_WEEKS = ONE_WEEK * 2;

        /// <summary>
        /// Three weeks, in seconds
        /// </summary>
        public const long THREE_WEEKS = ONE_WEEK * 3;

        /// <summary>
        /// Four weeks, in seconds
        /// </summary>
        public const long FOUR_WEEKS = ONE_WEEK * 4;

        /// <summary>
        /// One year, in seconds
        /// </summary>
        public const long ONE_YEAR = ONE_DAY * 365;

        /// <summary>
        /// Calculate an expiry date given a TTL parameter. You can optionally
        /// specify a start date (defaults to DateTime.Now)
        /// <param name="ttl">The TTL (time to live) in seconds</param>
        /// <param name="now">An (optional) start date, defaults to DateTime.Now</param>
        /// </summary>
        public static long CalculateExpiry(long ttl, DateTime now = default)
        {
            if (now == default)
                now = DateTime.Now;

            return ((DateTimeOffset) now).ToUnixTimeSeconds() + ttl;
        }

        /// <summary>
        /// Determines if an expiry date is currently expired
        /// </summary>
        /// <param name="expiry">The expiry date to check, in unix timestamp seconds</param>
        /// <returns>Returns true if DateTime.Now is equal to or past the given expiry date</returns>
        public static bool IsExpired(long expiry)
        {
            return Now() >= expiry;
        }

        /// <summary>
        /// Current DateTime.Now as unix timestamp seconds
        /// </summary>
        /// <returns></returns>
        public static long Now()
        {
            return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        }

        public static TimeSpan AsTimeSpan(long seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
