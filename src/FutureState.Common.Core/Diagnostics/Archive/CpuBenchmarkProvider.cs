#region

using System;
using System.Diagnostics;
using Microsoft.Win32;

#endregion

namespace FutureState.Diagnostics
{
    /// <summary>
    /// Utility class to query the host machine's clock speed in giga hertz.
    /// </summary>
    public sealed class CpuBenchmarkProvider
    {
        private const string _keyPath = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public CpuBenchmarkProvider()
        {
            try
            {
                // reg call should be faster than wmi
                // assume test harness has reg access rights
                Value = TryGetClockSpeedReg();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to read system clock speed: {0}", ex);
            }
        }

        /// <summary>
        /// Gets the current machine's cpu speed in Ghz. If the value cannot be
        /// extracted will yield null.
        /// </summary>
        public double? Value { get; }

        public static implicit operator double?(CpuBenchmarkProvider host)
        {
            return host?.Value;
        }

        /// <summary>
        /// Get the clock speed of the host from the registry.
        /// </summary>
        public double? TryGetClockSpeedReg()
        {
            try
            {
                using (var registrykeyHklm = Registry.LocalMachine)
                {
                    var registrykeyCpu = registrykeyHklm.OpenSubKey(_keyPath, false);
                    if (registrykeyCpu != null)
                    {
                        var megaHertzStr = registrykeyCpu.GetValue("~MHz").ToString();

                        double megaHertz;

                        if (double.TryParse(megaHertzStr, out megaHertz))
                        {
                            return megaHertz/1000.00;
                        }
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to read system clock speed from registry: {0}", ex);

                return null;
            }
        }
    }
}