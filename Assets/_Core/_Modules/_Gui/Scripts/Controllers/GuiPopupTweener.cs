namespace Core.Gui
{
    using System.Collections.Generic;
    using UnityEngine;

    public class GuiPopupTweener : MonoBehaviour
    {
        public float animationTime = 0.5f;
        public float delayBetweenElements = 0.2f;
        public float startScale = 0.3f;
        public List<Transform> elements;

        public event System.Action EffectEnded;

        private float _time;

        public void StartOpenTween()
        {
            elements.ForEach(e => { LeanTween.cancel(e.gameObject, true); });

            _time = animationTime;

            foreach (Transform element in elements)
            {
                if (element.gameObject.activeInHierarchy)
                {
                    var scaleOnFinish = element.localScale;
                    element.localScale = startScale * element.localScale;

                    LeanTween
                        .scale(element.gameObject, scaleOnFinish, _time)
                        .setEase(LeanTweenType.easeOutBack)
                        .setOnComplete(() =>
                        {
                            element.localScale = scaleOnFinish;
                            //if (element == elements.Last())
                            EffectEnded?.Invoke();
                        });

                    _time += delayBetweenElements;
                }
            }
        }

        public void CancelTween()
        {
            elements.ForEach(e => { LeanTween.cancel(e.gameObject, true); });
        }
    }
}