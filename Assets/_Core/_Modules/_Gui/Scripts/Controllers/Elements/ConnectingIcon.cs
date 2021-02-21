using Core.Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable

namespace Core.Gui.Controllers.Elements
{
    public class ConnectingIcon : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _radioIcon;
        [SerializeField] private CanvasGroup[] _waveIcons;

        [SerializeField] private float _radioDelay;
        [SerializeField] private float _radioFadeInTime;
        [SerializeField] private float _waveFadeTime;
        [SerializeField] private float _waveStayTime;
        [SerializeField] private float _waveFullPeriod;
        [SerializeField] private float[] _waveDelays;

        private float? _startTime;

        public void Show(bool show)
        {
            if (show && gameObject.activeInHierarchy ||
                !show && !gameObject.activeInHierarchy)
                return;
            gameObject.SetActive(show);
            if (show)
            {
                _startTime = Time.time;
                _radioIcon.alpha = 0;
                _waveIcons.ForEach(cg => cg.alpha = 0);
            }
            else
                _startTime = null;
        }

        void Update()
        {
            if (_startTime == null)
                return;
            var time = Time.time;
            var timeElapsed = time - _startTime.Value;
            if (timeElapsed < _radioDelay + _radioFadeInTime)
            {
                if (timeElapsed >= _radioDelay)
                    _radioIcon.alpha = (timeElapsed - _radioDelay) / _radioFadeInTime;
            }
            else
            {
                _radioIcon.alpha = 1;
                var waveElapsedTime = timeElapsed - (_radioDelay + _radioFadeInTime);
                for (int i = 0; i < _waveIcons.Length; ++i)
                {
                    var curWaveElapsedTime = waveElapsedTime - _waveDelays[i];
                    if (curWaveElapsedTime <= 0)
                        continue;
                    var curWavePeriodTime = curWaveElapsedTime - ((int)(curWaveElapsedTime / _waveFullPeriod) * _waveFullPeriod);
                    var icon = _waveIcons[i];
                    if (curWavePeriodTime < _waveFadeTime)
                        icon.alpha = curWavePeriodTime / _waveFadeTime;
                    else if (curWavePeriodTime < _waveFadeTime + _waveStayTime)
                        icon.alpha = 1;
                    else if (curWavePeriodTime < 2 * _waveFadeTime + _waveStayTime)
                        icon.alpha = (2 * _waveFadeTime + _waveStayTime - curWavePeriodTime) / _waveFadeTime;
                    else
                        icon.alpha = 0;
                }
            }
        }
    }
}
