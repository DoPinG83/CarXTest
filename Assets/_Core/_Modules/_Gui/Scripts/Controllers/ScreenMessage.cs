using System;
using System.Collections;
using Core.Gui.Controllers.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Gui.Controllers
{
    public class ScreenMessage : GuiController
    {
        [SerializeField] private int _maxMessages = 5;
        [SerializeField] private GameObject _messageElemPrefab = null;
        [SerializeField] private Transform _layoutRoot;
        [SerializeField] private float _showInfoDuration = 3f;
        [SerializeField] private float _slideUpDuration = 0.25f;
        [SerializeField] private Vector2 _defaultMessagesPosition = new Vector2(0, 440);
        [SerializeField] private ConnectionElement _connectionElement;

        private int _retainCount = 0;

        protected override void Start()
        {
            base.Start();

            //var rect = GetComponent<RectTransform>();
            //_defaultMessagesPosition = new Vector2(_defaultMessagesPosition.x, rect.rect.height / 2f);
            //((RectTransform)_layoutRoot).anchoredPosition = _defaultMessagesPosition;
        }

        public void ShowAlert(string text)
        {
            ShowText(text);
        }

        private IEnumerator ShowDelayed(string text, float delay, Action<string, float> showCllback)
        {
            yield return new WaitForSeconds(delay);
            if (showCllback != null)
                showCllback(text, -1);
        }

        public void ShowWarning(string text, float delay = -1)
        {
            if (delay > 0)
                StartCoroutine(ShowDelayed(text, delay, ShowWarning));
            else
                ShowText(text, Color.yellow);
        }

        public void ShowMessage(string text)
        {
            ShowText(text, Color.white);
        }

        private void ShowText(string text, Color? color = null)
        {
            MessageElement elem = _messageElemPrefab.InstantiateInParent<MessageElement>(_layoutRoot);
            elem.Init(text, _showInfoDuration, color);

            for (int i = 0; i < _layoutRoot.childCount - _maxMessages; i++)
            {
                var child = _layoutRoot.GetChild(i);
                var message = child.gameObject.GetComponent<MessageElement>();
                message.Hide();
            }

            if (QualitySettings.GetQualityLevel() != 0)
            {
                float slideHeight = ((RectTransform)_messageElemPrefab.transform).sizeDelta.y;

                if (_slideMessagesUp != null) StopCoroutine(_slideMessagesUp);
                _slideMessagesUp = SlideMessagesUp(
                    ((RectTransform)_layoutRoot).anchoredPosition - new Vector2(0, slideHeight),
                    _defaultMessagesPosition,
                    _slideUpDuration);
                StartCoroutine(_slideMessagesUp);
            }
        }

        private IEnumerator _slideMessagesUp;
        private IEnumerator SlideMessagesUp(Vector2 from, Vector2 to, float time)
        {
            float timePass = 0;
            float progress = 0;

            while (progress <= 1)
            {
                progress = timePass / time;

                ((RectTransform)_layoutRoot).anchoredPosition = Vector2.Lerp(from, to, progress);

                yield return null;
                timePass += Time.unscaledDeltaTime;
            }

            _slideMessagesUp = null;
        }

        public void ShowConnection(bool connecting, string text = "")
        {
            if (connecting)
            {
                _retainCount++;

                AnimateOpen();
                _connectionElement.Show(_retainCount > 0, text);
            }
            else
            {
                _retainCount--;

                OnClose = () =>
                {
                    _connectionElement.Show(_retainCount > 0, text);
                    CanvasGroup.alpha = 1;
                };
                AnimateClose();

            }
            //_connectionElement.Show(_retainCount > 0, text);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public override void Dispose()
        {
            StopAllCoroutines();
            base.Dispose();
        }
    }
}
