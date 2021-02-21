using System;
using UnityEngine;

namespace Core.Gui
{
    public class BackButtonListener : MonoBehaviour
    {
        public event Action OnBackButton;

        public bool DisableProcessing;

        void Update()
        {
            if (DisableProcessing) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (OnBackButton != null)
                    OnBackButton();
            }
        }

    }
}