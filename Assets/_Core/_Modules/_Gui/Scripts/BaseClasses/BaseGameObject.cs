using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Core.Utils;

public class BaseGameObject : MonoBehaviour
{
    private static event Action<TriggerMark> _onTrigger = delegate { };

    private GameObject _gameObject;

    private Transform _transform;

    private CharacterController _chController;

    private Animator _animator;

    private Rigidbody _rigidbody;

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

    public RectTransform ThisRectTransform
    {
        get
        {
            return ThisTransform as RectTransform;;
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

    public Rigidbody ThisRigidbody
    {
        get
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
            
            return _rigidbody;
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

    protected virtual void Awake()
    {
        if(ThisAnimator != null)
        {
            _animator.logWarnings = false;
        }

        _onTrigger += OnTrigger;
    }

    protected Coroutine Call(float t, Action a)
    {
        if (!gameObject.activeInHierarchy)
        {
            a();
            return null;
        }
        return StartCoroutine(DelayedActionCoroutine(t, a));
    }

    protected Coroutine OnNextFrame(Action a)
    {
        if (!gameObject.activeInHierarchy)
        {
            a();
            return null;
        }
        return StartCoroutine(NextFrameActionCoroutine(a));
    }

    protected void WaitingForInitialize(object o, Action a)
    {
        StartCoroutine(WaitingForInit(o, a));
    }

    IEnumerator WaitingForInit(object o, Action a)
    {
        CoreLog.Log(o);
        yield return new WaitUntil(() => o != null);
        a();
    }

    IEnumerator DelayedActionCoroutine(float t, Action a)
    {
        yield return null;
        yield return new WaitForSeconds(t);
        a?.Invoke();
    }

    IEnumerator NextFrameActionCoroutine(Action a)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        a?.Invoke();
    }

    protected Coroutine OnEndOfFrame(Action a)
    {
        if(!gameObject.activeInHierarchy)
        {
            a();
            return null;
        }
        return StartCoroutine(WaitForEndOfFrame(a));
    }

    protected Coroutine OnFixedUpdate(Action a)
    {
        if (!gameObject.activeInHierarchy)
        {
            a?.Invoke();
            return null;
        }

        return StartCoroutine(WaitForFixedUpdate(a));
    }
    
    private IEnumerator WaitForFixedUpdate(Action a)
    {
        yield return new WaitForFixedUpdate();
        
        a?.Invoke();
    }

    IEnumerator WaitForEndOfFrame(Action a)
    {
        yield return new WaitForEndOfFrame();
        a?.Invoke();
    }

    protected virtual void OnDestroy()
    {
        _onTrigger -= OnTrigger;
    }


    protected void Trigger(TriggerMark mark)
    {
        _onTrigger.Invoke(mark);
    }

    protected virtual void OnTrigger(TriggerMark mark) { }


    protected Coroutine WaitForFrames(int count, Action a)
    {
        if (!gameObject.activeInHierarchy)
        {
            a();
            return null;
        }

        return StartCoroutine(WaitForFramesCoroutine(count, a));
    }

    IEnumerator WaitForFramesCoroutine(int count, Action a)
    {
        for (var i = 0; i < count; i++)
        {
            yield return null;
        }

        if (a != null)
            a();
    }
}

public enum TriggerMark
{
    NoArmor,
    StartRegeneration,
	StopRegeneration,
    FullHp,
    TinPickup,

    //Sounds
    HGExplosion,
	DashUse,
	TPSShooting,
    
    //HUDSounds
    DeathScreenSecondLeft,
    DeathScreenLastSecond,
    PlayerRespawned,
    AbilityRecharge,
    EndBattle,
    EndBattleTick
}