using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaturatedSprite : MonoBehaviour
{
    [SerializeField] Image saturatedImage;
    void Start()
    {
        saturatedImage.material = Instantiate<Material>(saturatedImage.material);
        saturatedImage.material.SetFloat("_Desaturate", 1f);
    }

    private void OnDestroy()
    {
        Destroy(saturatedImage.material);
    }
}
