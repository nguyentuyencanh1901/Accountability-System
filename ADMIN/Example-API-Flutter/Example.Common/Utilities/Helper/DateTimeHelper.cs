using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Example.Common.Utilities.Helper
{
#pragma warning disable S138,S3776,S1541,S1871 // Functions should not have too many lines of code
    public static class DateTimeHelper
    {
        public static DateTime GetEndOfDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        public static DateTime GetStartOfDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }

        public static bool DatesOnTheSameDay(DateTime d1, DateTime d2)
        {
            return d1.Year == d2.Year && d1.Month == d2.Month && d1.Day == d2.Day;
        }

        public static string FormatDate(long createDate, string format = "")
        {
            DateTime date = new DateTime(createDate);
            if (!string.IsNullOrWhiteSpace(format))
            {
                return date.ToString(format);
            }
            return date.ToString("MMM dd, yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        }

        public static string FormatDate(DateTime createDate, string format = "")
        {
            if (!string.IsNullOrWhiteSpace(format))
            {
                return createDate.ToString(format);
            }
            return createDate.ToString("MMM dd, yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        }



        public static string GetTimeAgoDisplay(long ticks)
        {
            if (ticks == 0)
            {
                return string.Empty;
            }

            var date = ConvertTicksToDateTime(ticks);
            if (!date.HasValue)
            {
                return string.Empty;
            }

            var now = DateTime.Now;
            if (DateTime.Compare(now, date.Value) >= 0)
            {
                var s = DateTime.Now.Subtract(date.Value);
                TimeSpan w = DateTime.Now.StartOfWeek(DayOfWeek.Monday).Subtract(date.Value);

                var numberDays = (now - date).Value.Days;

                var weekDiff = DateTime.Now.DayOfWeek == DayOfWeek.Monday ? (int)w.TotalDays + 7 : (int)w.TotalDays;

                var dayDiff = (int)s.TotalDays;

                var secDiff = (int)s.TotalSeconds;

                if (dayDiff == 0)
                {
                    if (secDiff < 60)
                    {
                        return "vừa xong";
                    }
                    if (secDiff < 120)
                    {
                        return "1 phút trước";
                    }
                    if (secDiff < 3600)
                    {
                        return string.Format("{0} phút trước", Math.Floor((double)secDiff / 60));
                    }
                    if (secDiff < 7200)
                    {
                        return "1 giờ trước";
                    }
                    if (secDiff < 86400)
                    {
                        return string.Format("{0} giờ trước", Math.Floor((double)secDiff / 3600));
                    }
                }
                if (numberDays == 1)
                {
                    return "Hôm qua lúc " + date.Value.ToString("HH:mm");
                }
                if (dayDiff > 1 && weekDiff < 7)
                {
                    var dateOfWeek = string.Empty;
                    switch (date.Value.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            dateOfWeek = "Thứ Hai";
                            break;

                        case DayOfWeek.Tuesday:
                            dateOfWeek = "Thứ Ba";
                            break;

                        case DayOfWeek.Wednesday:
                            dateOfWeek = "Thứ Tư";
                            break;

                        case DayOfWeek.Thursday:
                            dateOfWeek = "Thứ Năm";
                            break;

                        case DayOfWeek.Friday:
                            dateOfWeek = "Thứ Sáu";
                            break;

                        case DayOfWeek.Saturday:
                            dateOfWeek = "Thứ Bảy";
                            break;

                        case DayOfWeek.Sunday:
                            dateOfWeek = "Chủ Nhật";
                            break;
                        default:
                            break;
                    }
                    return string.Format("{0} lúc {1}", dateOfWeek, date.Value.ToString("HH:mm"));
                }

                if (dayDiff <= 7)
                {
                    return string.Format("{0} ngày trước", dayDiff);
                }

                return string.Format("{0} {1}:{2}", 
                    date.Value.ToString("dd/MM/yyyy"),
                    date.Value.Hour < 10 ? "0" + date.Value.Hour : date.Value.Hour + "", 
                    date.Value.Minute < 10 ? "0" + date.Value.Minute : date.Value.Minute + ""
                    );
            }
            else
                return string.Empty;
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = startOfWeek - dt.DayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(diff).Date;
        }

        public static string GetTimeDisplayByDay(long ticks)
        {
            if (ticks > 0)
            {
                var date = ConvertTicksToDateTime(ticks);
                if (date.HasValue)
                {
                    var now = DateTime.Now;
                    if (DateTime.Compare(now, date.Value) >= 0)
                    {
                        var s = DateTime.Now.Subtract(date.Value);

                        var dayDiff = (int)s.TotalDays;

                        var secDiff = (int)s.TotalSeconds;

                        if (dayDiff == 0)
                        {
                            if (secDiff < 60)
                            {
                                return "vừa xong";
                            }
                            if (secDiff < 120)
                            {
                                return "1 phút trước";
                            }
                            if (secDiff < 3600)
                            {
                                return string.Format("{0} phút trước", Math.Floor((double)secDiff / 60));
                            }
                            if (secDiff < 7200)
                            {
                                return "1 giờ trước";
                            }
                            if (secDiff < 86400)
                            {
                                return string.Format("{0} giờ trước", Math.Floor((double)secDiff / 3600));
                            }
                        }
                        else if (dayDiff == 1)
                        {
                            return date.Value.ToString("HH:mm") + " - Hôm qua";
                        }
                        else
                        {
                            return string.Format("{0}:{1} - {2}", date.Value.Hour < 10 ? "0" + date.Value.Hour : date.Value.Hour + "", date.Value.Minute < 10 ? "0" + date.Value.Minute : date.Value.Minute + "", date.Value.ToString("dd/MM/yyyy"));
                        }
                        return string.Empty;
                    }
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public static DateTime? ConvertTicksToDateTime(long lticks)
        {
            if (lticks == 0) return null;
            return new DateTime(lticks);
        }

        public static string ConvertTicksToStringFormat(long ticks)
        {
            if (ticks == 0) return string.Empty;
            var convertDate = ConvertTicksToDateTime(ticks);
            if (!convertDate.HasValue) return string.Empty;

            return string.Format("{0} {1}", convertDate.Value.ToString("dd/MM/yyyy"), convertDate.Value.ToString("HH:mm"));
        }

        public static string ConvertTicksToStringFormatWithDash(long ticks)
        {
            if (ticks == 0) return string.Empty;
            var convertDate = ConvertTicksToDateTime(ticks);
            if (!convertDate.HasValue) return string.Empty;

            return string.Format("{0} - {1}", convertDate.Value.ToString("dd/MM/yyyy"), convertDate.Value.ToString("HH:mm"));
        }

        public static string ConvertTicksToStringFormat(long ticks, string formatString)
        {
            if (ticks == 0) return string.Empty;
            var convertDate = ConvertTicksToDateTime(ticks);
            if (!convertDate.HasValue) return string.Empty;
            return string.Format("{0}", convertDate.Value.ToString(formatString));
        }

        public static string ConvertTicksToStringTimeFormat(long ticks)
        {
            if (ticks == 0) return string.Empty;
            var prefix = "";
            var convertDate = ConvertTicksToDateTime(ticks);
            if (!convertDate.HasValue) return string.Empty;

            var hour = convertDate.Value.Hour;
            if (hour >= 0 && hour <= 12) prefix = "sáng";
            if (hour >= 13 && hour <= 17) prefix = "chiều";
            if (hour >= 18 && hour <= 23) prefix = "tối";
            var strDateFormat = "lúc {0} " + prefix;
            return string.Format(strDateFormat, convertDate.Value.ToString("HH:mm"));
        }

        public static DateTime ConvertStringToDateTime(string value, string format)
        {
            return DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }

        public static DateTime ConvertToDateTime(object value)
        {
            if (null == value || !DateTime.TryParse(value.ToString(), out var returnValue))
            {
                returnValue = DateTime.MinValue;
            }

            return returnValue;
        }

        /// <summary>
        /// Return double, > 0 then dateTo > dateFrom.
        /// <pre> The instant: d = Date, h = Hour, m = Minute, s = Second</pre>
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="instant"></param>
        /// <returns></returns>
        public static double DateDiff(DateTime dateFrom, DateTime dateTo, string instant)
        {
            var span = dateTo - dateFrom;
            const double num = 0.0;
            var instantLower = instant.ToLower();
            if (instantLower == null)
            {
                return num;
            }
            switch (instantLower)
            {
                default:
                case "d":
                    return span.TotalDays;

                case "h":
                    return span.TotalHours;

                case "m":
                    return span.TotalMinutes;

                case "s":
                    return span.TotalSeconds;

                case "ms":
                    return span.TotalMilliseconds;
            }
        }

        public static long DateTimeToUnixTime(DateTime dateTime)
        {
            //DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //TimeSpan span = (TimeSpan)(dateTime - epoch.ToLocalTime());
            //return (long)(span.TotalSeconds * 1000.0);
            return (long)(new DateTimeOffset(dateTime).ToUnixTimeSeconds() * 1000.0);
        }

        public static long DateTimeToUnixTimeDaily(DateTime dateTime)
        {
            dateTime = DateTime.Parse(dateTime.ToString("MM/dd/yyyy 00:00:00"));
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var span = (TimeSpan)(dateTime.Date - epoch.ToLocalTime());
            return (long)(span.TotalSeconds * 1000.0);
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp / 1000.0).ToLocalTime();
            return dtDateTime;
        }

        public static long DateTimeToSpanHourly(DateTime dateTime)
        {
            dateTime = DateTime.Parse(dateTime.ToString("MM/dd/yyyy HH:00:00"));
            var time = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0);
            var span = (TimeSpan)(dateTime - time.ToLocalTime());
            return (long)(span.TotalSeconds * 1000.0);
        }

        public static Tuple<long, long> GetStartTineAndEndtimeFilter(int type)
        {
            Tuple<DateTime, DateTime> obj = GetStartTineAndEndtime(type);
            long startTimeStamp = DateTimeToUnixTime(obj.Item1);
            long endTimeStamp = DateTimeToUnixTime(obj.Item2);

            return new Tuple<long, long>(startTimeStamp, endTimeStamp);
        }

        public static Tuple<DateTime, DateTime> GetStartTineAndEndtime(int type)
        {
            DateTime startTime;
            DateTime endTime;

            DateTime baseDate = DateTime.Today;
            //var yesterday = baseDate.AddDays(-1);

            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddMilliseconds(-1);

            var lastWeekStart = thisWeekStart.AddDays(-7);
            var lastWeekEnd = thisWeekStart.AddMilliseconds(-1);

            var thisMonthStart = baseDate.AddDays(1 - baseDate.Day);
            var thisMonthEnd = thisMonthStart.AddMonths(1).AddMilliseconds(-1);

            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddMilliseconds(-1);

            int quarterNumber = (baseDate.Month - 1) / 3 + 1;
            DateTime firstDayOfQuarter = new DateTime(baseDate.Year, (quarterNumber - 1) * 3 + 1, 1);
            DateTime lastDayOfQuarter = firstDayOfQuarter.AddMonths(3).AddMilliseconds(-1);

            switch (type)
            {
                case 1:
                    startTime = baseDate;
                    endTime = baseDate.AddHours(24).AddMilliseconds(-1);
                    break;
                //Ngay hôm qua
                case 2:
                    startTime = baseDate.AddDays(-1);
                    endTime = baseDate.AddMilliseconds(-1);
                    break;

                //Tuần này
                case 3:
                    startTime = thisWeekStart;
                    endTime = thisWeekEnd;
                    break;

                //Tuần trước
                case 4:
                    startTime = lastWeekStart;
                    endTime = lastWeekEnd;
                    break;

                //Tháng này
                case 5:
                    startTime = thisMonthStart;
                    endTime = thisMonthEnd;
                    break;

                //Tháng trước
                case 6:
                    startTime = lastMonthStart;
                    endTime = lastMonthEnd;
                    break;

                //Quý này
                case 7:
                    //DateTime date = DateTime.Now.AddMonths(3);

                    startTime = firstDayOfQuarter;
                    endTime = lastDayOfQuarter;
                    break;

                //Quý trước
                case 8:
                    startTime = firstDayOfQuarter.AddMonths(-3);
                    endTime = lastDayOfQuarter.AddMonths(-3);
                    break;

                //Năm này
                case 9:
                    startTime = new DateTime(baseDate.Year, 1, 1); // 1st Feb this year
                    endTime = new DateTime(baseDate.Year + 1, 1, 1).AddMilliseconds(-1); // Last day in January next year
                    break;

                //Năm trước
                case 10:
                    startTime = new DateTime(baseDate.Year - 1, 1, 1); // 1st Feb last year
                    endTime = new DateTime(baseDate.Year, 1, 1).AddMilliseconds(-1); // 1st Feb this year
                    break;

                ////7 ngày trước
                //case 11:
                //    startTime = baseDate.AddDays(-7);
                //    endTime = baseDate.AddMilliseconds(-1);
                //    break;
                ////30 ngày trước
                //case 12:
                //    startTime = baseDate.AddDays(-30);
                //    endTime = baseDate.AddMilliseconds(-1);
                //    break;
                ////1 tháng
                //case 13:
                //    startTime = baseDate.AddMonths(-1);
                //    endTime = baseDate.AddHours(24).AddMilliseconds(-1);
                //    break;
                ////2 tháng
                //case 14:
                //    startTime = baseDate.AddMonths(-2);
                //    endTime = baseDate.AddHours(24).AddMilliseconds(-1);
                //    break;
                ////3 tháng
                //case 15:
                //    startTime = baseDate.AddMonths(-3);
                //    endTime = baseDate.AddHours(24).AddMilliseconds(-1);
                //    break;
                ////6 tháng
                //case 16:
                //    startTime = baseDate.AddMonths(-6);
                //    endTime = baseDate.AddHours(24).AddMilliseconds(-1);
                //    break;

                default:
                    startTime = baseDate;
                    endTime = baseDate.AddHours(24).AddMilliseconds(-1);
                    break;
            }

            return new Tuple<DateTime, DateTime>(startTime, endTime);
        }

        /// <summary>
        /// Get date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            while (startDate <= endDate)
            {
                yield return startDate;
                startDate = startDate.AddDays(1);
            }
        }
    }
}
