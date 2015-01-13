using System;

namespace WalkCompanion.Utils
{
    public static class Calculators
    {
        /// <summary>
        /// http://developer.nokia.com/community/wiki/Jogging_-_Calorie_Calculator:_Demonstrating_Real_World_usecase_of_WP8_features
        /// </summary>
        /// <param name="distance">meters</param>
        /// <param name="timeM">minutes</param>
        /// <param name="weight">kg</param>
        /// <returns></returns>
        public static double CaloriesBurned(double distance, TimeSpan timeElapsed, double weight = 85)
        {
            double timeInMinutes = timeElapsed.TotalMinutes;
            double speed = timeInMinutes > 0 ? distance / timeInMinutes : 0;
            double temp = speed <= 99.16 ? (0.1 * speed) + (1.8 * speed * 0.01) + 3.5 : (0.2 * speed) + (0.9 * speed * 0.01) + 3.5;

            double calExpPerMin = (temp * weight) / 200;
            double totalCalExp = calExpPerMin * timeInMinutes;

            return totalCalExp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance">meters</param>
        /// <param name="timeElapsed">time elapsed</param>
        /// <returns>km/h</returns>
        public static double AverageSpeed(double distance, TimeSpan timeElapsed)
        {
            return timeElapsed.TotalHours > 0 ? (distance / 1000) / timeElapsed.TotalHours : 0;
        }
    }
}
