using UnityEngine;
using UnityEngine.UI;

namespace Core.Gui.Controllers.Elements
{
    public class MessageElement : MonoBehaviour
    {
        [SerializeField] private Text _messageText = null;
        [SerializeField] private Animator _animator;

        private bool _isHiding = false;

        public void Init(string text, float time, Color? color)
        {
            _messageText.text = text;
            if (color.HasValue)
                _messageText.color = color.Value;

            if (QualitySettings.GetQualityLevel() != 0)
            {
                _animator.SetTrigger("show");
                this.StartBehaviourTimer(time, false, true, () => _animator.SetTrigger("hide"));
            }
            else
            {
                this.StartBehaviourTimer(time, false, true, Kill);
            }
        }

        public void Hide()
        {
            if (_isHiding)
                return;

            _isHiding = true;

            if (QualitySettings.GetQualityLevel() != 0)
                _animator.SetTrigger("quick_hide");
            else
                Kill();
        }

        private void Kill()
        {
            Destroy(gameObject);
        }
    }
}
