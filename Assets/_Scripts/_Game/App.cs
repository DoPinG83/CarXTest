namespace Core.Game
{
    using Core.Common;
    using UnityEngine;

    public class App : MonoBehaviour
    {
        public static GameObject Systems;

        [SerializeField] private GameObject _systems;
        
        public static readonly Container Common = new Container();  
        
        private void Awake()
        {
            Systems = _systems;

        }
    }
}