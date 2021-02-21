using System;
using UnityEngine;
using System.Collections;
using Core.Gui;
using Core.Gui.GuiPool;

namespace Core.Utils
{
    [RequireComponent(typeof(Animation))]
    public class GentleAnimation : MonoBehaviour, IDisposable, IPoolable
    {
        private Animation _animation;
        private AnimationState _clipState;
        private int _framesCount;
        private int _currentFrame;
        private float _changeFrameDuration;
        private Action<string> _onFinish;
        private Action<string> _onPercentPlayed;
        private float _onPercentPlayedValue;

        private const float _cFpsFrameRate = 0.03333333f;

        private void Awake()
        {
            _animation = GetComponent<Animation>();
        }

        public void Dispose()
        {
            _clipState = null;
            _currentFrame = 0;
            _changeFrameDuration = 0;
            _onPercentPlayedValue = 0.0f;
            _onFinish = null;
            _onPercentPlayed = null;
        }

        public void Play(string clipName, Action<string> onFinish = null)
        {
            Play(_animation[clipName], onFinish);
        }

        public void PlayClipByIndex(int index, Action<string> onFinish = null)
        {
            Play(_animation.GetClipByIndex(index), onFinish);
        }

        public void OnSpawn() {}

        public void OnUnspawn()
        {
            Dispose();
        }

        public void OnDestroy()
        {
            Dispose();
        }

        private void Play(AnimationState state, Action<string> onFinish = null)
        {
            if (state == null)
            {
                if (onFinish != null) onFinish(null);
                return;
            }

            _currentFrame = 0;
            _clipState = state;
            _framesCount = Mathf.FloorToInt(_clipState.length / _cFpsFrameRate);
            _changeFrameDuration = _cFpsFrameRate;

            _onFinish = onFinish;
            SampleAnimFrame();
        }

        public void Stop()
        {
            _clipState = null;
            _onFinish = null;
            _onPercentPlayed = null;
        }

        private void SampleAnimFrame()
        {
            // тут осмысленный ретурн вместо эксепшена, потому что иначе запорем весь апдейт
            if (_clipState == null || _animation == null)
            {
                CoreLog.LogWarning("Call SampleAnimFrame before initialization");
                return;
            }

            _clipState.normalizedTime = _currentFrame / (float)_framesCount;
            _clipState.enabled = true;
            _clipState.weight = 1;
            _animation.Sample();
            _clipState.enabled = false;
        }

        private void Update()
        {
            // а тут просто чтобы не спамило до инита
            if (_clipState == null || _animation == null)
                return;

            _changeFrameDuration -= Time.unscaledDeltaTime;
            if (_changeFrameDuration <= (Time.smoothDeltaTime / 2f))
            {
                _changeFrameDuration = _cFpsFrameRate;
                _currentFrame++;

                if (_currentFrame <= _framesCount)
                {
                    SampleAnimFrame();

                    if (_onPercentPlayed != null)
                    {
                        if (_currentFrame / (float)_framesCount >= _onPercentPlayedValue)
                        {
                            _onPercentPlayed(_clipState.name);
                            _onPercentPlayed = null;
                        }
                    }
                }
                else
                {
                    if (_onFinish != null)
                        _onFinish(_clipState.name);

                    _clipState = null;
                }
            }
        }

        public void SetOnPercentPlayed(Action<string> onPercentPlayed, float onPercentPlayedValue)
        {
            _onPercentPlayed = onPercentPlayed;
            _onPercentPlayedValue = onPercentPlayedValue;
        }
    }
}
