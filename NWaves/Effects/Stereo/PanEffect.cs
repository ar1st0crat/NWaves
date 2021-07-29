using System;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Stereo panning effect
    /// </summary>
    public class PanEffect : StereoEffect
    {
        /// <summary>
        /// Pan value (must be in range [-1, 1])
        /// </summary>
        protected float _pan;

        /// <summary>
        /// Pan value for calculations (in range [0, 1])
        /// </summary>
        protected float _mappedPan;

        /// <summary>
        /// Pan value for calculations in constant-power mode (in range [0, 1])
        /// </summary>
        protected float _constantPowerPan;

        /// <summary>
        /// Pan value (must be in range [-1, 1])
        /// </summary>
        public float Pan
        { 
            get => _pan;
            set
            {
                _pan = value;
                if (_pan > 1) _pan = 1;
                if (_pan < -1) _pan = -1;

                _mappedPan = (_pan + 1) / 2;
                _constantPowerPan = (float)(Math.PI * (_pan + 1) / 4);
            }
        }

        /// <summary>
        /// Pan rule (pan law)
        /// </summary>
        public PanRule PanRule;

        /// <summary>
        /// Stereo pan constructor
        /// </summary>
        /// <param name="pan"></param>
        /// <param name="panRule"></param>
        public PanEffect(float pan, PanRule panRule)
        {
            Pan = pan;
            PanRule = panRule;
        }

        /// <summary>
        /// Process two channels : [ input left , input right ] -> [ output left , output right ]
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public override void Process(ref float left, ref float right)
        {
            switch (PanRule)
            {
                case PanRule.Balanced:
                    {
                        left *= 2 * Math.Min(0.5f, 1 - _mappedPan);
                        right *= 2 * Math.Min(0.5f, _mappedPan);
                        return;
                    }

                case PanRule.ConstantPower:
                    {
                        left *= (float)Math.Cos(_constantPowerPan);
                        right *= (float)Math.Sin(_constantPowerPan);
                        return;
                    }

                case PanRule.Sin3Db:
                    {
                        var gain = Math.Sqrt(2.0);
                        left *= (float)(gain * Math.Cos(0.5 * Math.PI * _mappedPan));
                        right *= (float)(gain * Math.Sin(0.5 * Math.PI * _mappedPan));
                        return;
                    }

                case PanRule.Sin4_5Db:
                    {
                        var gain = Math.Pow(2, 3 / 4.0);
                        left *= (float)(gain * Math.Pow(Math.Cos(0.5 * Math.PI * _mappedPan), 1.5));
                        right *= (float)(gain * Math.Pow(Math.Sin(0.5 * Math.PI * _mappedPan), 1.5));
                        return;
                    }

                case PanRule.Sin6Db:
                    {
                        var gain = 2.0;
                        left *= (float)(gain * Math.Pow(Math.Cos(0.5 * Math.PI * _mappedPan), 2.0));
                        right *= (float)(gain * Math.Pow(Math.Sin(0.5 * Math.PI * _mappedPan), 2.0));
                        return;
                    }

                case PanRule.SqRoot3Db:
                    {
                        var gain = Math.Sqrt(2.0);
                        left *= (float)(gain * Math.Sqrt(1 - _mappedPan));
                        right *= (float)(gain * Math.Sqrt(_mappedPan));
                        return;
                    }

                case PanRule.SqRoot4_5Db:
                    {
                        var gain = Math.Pow(2, 3 / 4.0);
                        left *= (float)(gain * Math.Pow(1 - _mappedPan, 1.5));
                        right *= (float)(gain * Math.Pow(_mappedPan, 1.5));
                        return;
                    }

                case PanRule.Linear:
                default:
                    {
                        left *= 2 * (1 - _mappedPan);
                        right *= 2 * _mappedPan;
                        return;
                    }
            }
        }
    }
}
