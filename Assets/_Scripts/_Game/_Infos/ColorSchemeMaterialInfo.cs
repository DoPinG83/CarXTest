using UniRx.Async;
using UnityEngine;

namespace Core.Game.Infos
{
    public class ColorSchemeMaterialInfo : ColorSchemeInfo
    {
        private MeshRenderer _mesh;
        [SerializeField] private Material[] materials;
        
        protected override async UniTask OnInfoEnable()
        {
            _mesh = GetComponent<MeshRenderer>();
        }
        
        public override void SetColorScheme(int schemeId)
        {
            _mesh.sharedMaterial = materials[schemeId];
        }
    }
}