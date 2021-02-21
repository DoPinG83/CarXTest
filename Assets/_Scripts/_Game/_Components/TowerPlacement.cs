using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] string _towerName;
    [HideInInspector] public string TowerName { get { return _towerName; } }

    [SerializeField] string _gunName;
    [HideInInspector] public string GunName { get { return _gunName; } }

    [SerializeField] Transform _gunPosition;
    [HideInInspector] public Transform GunPosition { get { return _gunPosition; } }
}
