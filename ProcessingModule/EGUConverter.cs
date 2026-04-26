using System;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for engineering unit conversion.
    /// </summary>
    public class EGUConverter
    {
        /// <summary>
        /// Converts the point value from raw to EGU form.
        /// Formula: EGU = scalingFactor * rawValue + deviation
        /// </summary>
        public double ConvertToEGU(double scalingFactor, double deviation, ushort rawValue)
        {
            return scalingFactor * rawValue + deviation;
        }

        /// <summary>
        /// Converts the point value from EGU to raw form.
        /// Formula: raw = round((EGU - deviation) / scalingFactor)
        /// </summary>
		public ushort ConvertToRaw(double scalingFactor, double deviation, double eguValue)
        {
            return (ushort)Math.Round((eguValue - deviation) / scalingFactor);
        }
    }
}