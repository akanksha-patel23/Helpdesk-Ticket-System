using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Core.Helpers;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo IstZone =
        TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

    public static DateTime ToIst(DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utc, IstZone);
    }

    public static DateTime? ToIst(DateTime? utc)
    {
        return utc.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(utc.Value, IstZone)
            : null;
    }
}
