namespace Core.Game.Components
{
    using UniRx;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class Clickable : MonoBehaviour, IPointerClickHandler
    {
        public ReactiveCommand OnClick = new ReactiveCommand();
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Execute();
        }
    }
}
