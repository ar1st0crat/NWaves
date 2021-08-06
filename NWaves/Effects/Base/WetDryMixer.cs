using System;

namespace NWaves.Effects.Base
{
    /// <summary>
    /// Class responsible for wet/dry mixing logic
    /// </summary>
    public class WetDryMixer : IMixable
    {
        /// <summary>
        /// Wet gain
        /// </summary>
        public float Wet { get; set; } = 1f;

        /// <summary>
        /// Dry gain
        /// </summary>
        public float Dry { get; set; } = 0f;

        /// <summary>
        /// Set wet/dry mix (in range [0.0, 1.0])
        /// </summary>
        /// <param name="mix">Wet/dry mix</param>
        /// <param name="mixingRule">Mixing rule</param>
        public void WetDryMix(float mix, MixingRule mixingRule = MixingRule.Linear)
        {
            if (mix < 0f)
            {
                mix = 0;
            }

            if (mix > 1f)
            {
                mix = 1;
            }

            switch (mixingRule)
            {
                case MixingRule.Balanced:
                    {
                        Dry = 2 * Math.Min(0.5f, 1 - mix);
                        Wet = 2 * Math.Min(0.5f, mix);
                        break;
                    }

                case MixingRule.Sin3Db:
                    {
                        Dry = (float)Math.Cos(0.5 * Math.PI * mix);
                        Wet = (float)Math.Sin(0.5 * Math.PI * mix);
                        break;
                    }

                case MixingRule.Sin4_5Db:
                    {
                        Dry = (float)Math.Pow(Math.Cos(0.5 * Math.PI * mix), 1.5);
                        Wet = (float)Math.Pow(Math.Sin(0.5 * Math.PI * mix), 1.5);
                        break;
                    }

                case MixingRule.Sin6Db:
                    {
                        Dry = (float)Math.Pow(Math.Cos(0.5 * Math.PI * mix), 2.0);
                        Wet = (float)Math.Pow(Math.Sin(0.5 * Math.PI * mix), 2.0);
                        break;
                    }

                case MixingRule.SqRoot3Db:
                    {
                        Dry = (float)Math.Sqrt(1 - mix);
                        Wet = (float)Math.Sqrt(mix);
                        break;
                    }

                case MixingRule.SqRoot4_5Db:
                    {
                        Dry = (float)Math.Pow(1 - mix, 1.5);
                        Wet = (float)Math.Pow(mix, 1.5);
                        break;
                    }

                case MixingRule.Linear:
                default:
                    {
                        Dry = 1 - mix;
                        Wet = mix;
                        break;
                    }
            }
        }

        /// <summary>
        /// Set wet/dry values in decibels (linear mix rule)
        /// </summary>
        /// <param name="wetDb"></param>
        /// <param name="dryDb"></param>
        public void WetDryDb(double wetDb, double dryDb)
        {
            var w = Math.Pow(10, wetDb / 20);
            var d = Math.Pow(10, dryDb / 20);

            var mix = w / (w + d);

            WetDryMix((float)mix, MixingRule.Linear);
        }
    }
}
