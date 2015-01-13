using System;

namespace WalkCompanion.Utils
{
    public static class UnitsConverter
    {
        public static string TransformMetersToDesiredUnit(double meters, bool useMetricSystem)
        {
            double convertedUnit = meters;

            if (useMetricSystem == false)
                convertedUnit = MetersToYards(meters);

            string distanceCalculated = RoundDoubleAndConvertToString(convertedUnit);

            return useMetricSystem ? distanceCalculated + " (m)" : distanceCalculated + " (yd)";
        }

        public static string TransformSpeedToDesiredUnit(double kmPerHour, bool useMetricSystem)
        {
            double convertedUnit = kmPerHour;

            if (useMetricSystem == false)
                convertedUnit = KilometersToMiles(kmPerHour);

            string distanceCalculated = RoundDoubleAndConvertToString(convertedUnit);

            return useMetricSystem ? distanceCalculated + " (km/h)" : distanceCalculated + " (mph)";
        }

        public static double MetersToYards(double meters)
        {
            return meters * 1.0936133;
        }

        public static double KilometersToMiles(double meters)
        {
            return meters * 0.62137119;
        }

        public static string RoundDoubleAndConvertToString(double distance)
        {
            return distance.ToString("N0");
        }

        public static string DateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }

        public static string TimeSpanToString(TimeSpan timeSpan)
        {
            return timeSpan.ToString("mm':'ss");
        }
    }
}
