using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TriggerGameObject : EventTrigger
{
    private GameObject _gameObject;

    private Transform _transform;

    private CharacterController _chController;

    private Animator _animator;

    public GameObject ThisGameObject
    {
        get
        {
            if (_gameObject == null)
                _gameObject = gameObject;

            return _gameObject;
        }
    }

    public Transform ThisTransform
    {
        get
        {
            if (_transform == null)
                _transform = transform;

            return _transform;
        }
    }

    public CharacterController ChController
    {
        get
        {
            if (_chController == null)
                _chController = GetComponent<CharacterController>();

            return _chController;
        }
    }

    public Animator ThisAnimator
    {
        get
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();

            return _animator;
        }
    }
}
