using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLB
{
    [HelpURL(Consts.Help.UrlEffectFlicker)]
    [AddComponentMenu(Consts.Help.AddComponentMenuEffectFlicker)]
    public class EffectFlicker : EffectAbstractBase
    {
        public new const string ClassName = "EffectFlicker";

        [Range(1.0f, 60.0f)]
        public float frequency = Consts.Effects.FrequencyDefault;

        public bool performPauses = Consts.Effects.PerformPausesDefault;

        [MinMaxRange(0.0f, 10.0f)]
        public MinMaxRangeFloat flickeringDuration = Consts.Effects.FlickeringDurationDefault;

        [MinMaxRange(0.0f, 10.0f)]
        public MinMaxRangeFloat pauseDuration = Consts.Effects.PauseDurationDefault;

        public bool restoreIntensityOnPause = Consts.Effects.RestoreIntensityOnPauseDefault;

        [MinMaxRange(-5.0f, 5.0f)]
        public MinMaxRangeFloat intensityAmplitude = Consts.Effects.IntensityAmplitudeDefault;

        [Range(0.0f, 0.25f)]
        public float smoothing = Consts.Effects.SmoothingDefault;

        float m_CurrentAdditiveIntensity = 0.0f;

        public override void InitFrom(EffectAbstractBase source)
        {
            base.InitFrom(source);

            var sourceFlicker = source as EffectFlicker;
            if (sourceFlicker)
            {
                frequency = sourceFlicker.frequency;
                performPauses = sourceFlicker.performPauses;
                flickeringDuration = sourceFlicker.flickeringDuration;
                pauseDuration = sourceFlicker.pauseDuration;
                restoreIntensityOnPause = sourceFlicker.restoreIntensityOnPause;
                intensityAmplitude = sourceFlicker.intensityAmplitude;
                smoothing = sourceFlicker.smoothing;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(CoUpdate());
        }

        IEnumerator CoUpdate()
        {
            while(true)
            {
                yield return CoFlicker();

                if(performPauses)
                {
                    yield return CoChangeIntensity(pauseDuration.randomValue, restoreIntensityOnPause ? 0f : m_CurrentAdditiveIntensity);
                }
            }
        }

        IEnumerator CoFlicker()
        {
            float remainingDuration = flickeringDuration.randomValue;
            float lastTime = Time.deltaTime;

            while (!performPauses || remainingDuration > 0.0f)
            {
                Debug.Assert(frequency > 0.0f);
                float freqDuration = 1.0f / frequency;
                yield return CoChangeIntensity(freqDuration, intensityAmplitude.randomValue);
                remainingDuration -= freqDuration;
            }
        }

        IEnumerator CoChangeIntensity(float expectedDuration, float nextIntensity)
        {
            float velocity = 0.0f;
            float t = 0.0f;

            while (t < expectedDuration)
            {
                m_CurrentAdditiveIntensity = Mathf.SmoothDamp(m_CurrentAdditiveIntensity, nextIntensity, ref velocity, smoothing);
                SetAdditiveIntensity(m_CurrentAdditiveIntensity);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}