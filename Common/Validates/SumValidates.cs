using System;

namespace Common.Validates
{
    public static class DateValidator
    {
        public static string ValidateStartEnd(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && startDate.Value.Date < DateTime.UtcNow.Date)
            {
                return "Start date must be today or later.";
            }

            if (endDate.HasValue)
            {
                if (!startDate.HasValue)
                {
                    return "Start date must be provided before end date.";
                }

                if (endDate.Value.Date <= startDate.Value.Date)
                {
                    return "End date must be after start date.";
                }

                if (endDate.Value.Date <= DateTime.UtcNow.Date)
                {
                    return "End date must be after today.";
                }
            }

            return null; // No errors
        }
    }
}
