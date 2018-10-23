using System;
using System.Collections.Generic;
using System.Linq;
using Assets.IL.Scripts.Common;
using IL.Input;
using UnityEngine;

public class GameObjectArgs : EventArgs
{
    public GameObject GameObject;
    public GameObject Triggered;
}

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DropZone : MonoBehaviour
{
    public event EventHandler<GameObjectArgs> OnEnterDropZone;
    public event EventHandler<GameObjectArgs> OnExitDropZone;

    public event EventHandler<GameObjectArgs> OnDropOnZone;
	public event EventHandler<GameObjectArgs> OnDropWrongObject;

	public static DropZone Target;
    public static DropZone _oldTarget;


    public CardType ExpectedCardType;


	private bool _entered;
	private Collider2D _collider;

	[SerializeField]
    private bool _requireObjectOverZone = false;

	void Awake()
    {
		_collider = GetComponent<Collider2D>();
	    _collider.isTrigger = true;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Start() {}

    /// <summary>
    /// Removes all events tied to this object.
    /// </summary>
    public void UnWire()
    {
        OnEnterDropZone = null;
        OnExitDropZone = null;
        OnDropOnZone = null;
	    OnDropWrongObject = null;

    }

    // Called via unity when a rigidbody with collider enters a trigger collider
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled)
            return;

        if (other.gameObject == Clickable.Selected)
        {
            var enteredDropZone = this;
            //Use this to require the whole image to be in the drop zone.
            if (_requireObjectOverZone && !_collider.ContainsAll(other))
                enteredDropZone = null;
            SetTarget(enteredDropZone, other.gameObject);
        }
    }

    private void SetTarget(DropZone nextTarget, GameObject triggeringDraggable)
    {
        // Exit the drop zone
        if(Target != null && Target != nextTarget )
        {
            Target.OnExitDropZone.RaiseEvent(Target, new GameObjectArgs { GameObject = triggeringDraggable, Triggered = Target.gameObject});
        }

        // Enter a drop zone
        if(nextTarget != null)
        {
            var args = new GameObjectArgs { GameObject = triggeringDraggable, Triggered = gameObject };
            _entered = true;
            _oldTarget = Target;
            Target = nextTarget;
            OnEnterDropZone.RaiseEvent(this, args);
        }
        _oldTarget = Target;
        Debug.LogWarning("Set target to: " + nextTarget);
        Target = nextTarget;
    }

    // Called via unity when a rigidbody with collider stays on a trigger collider
    void OnTriggerStay2D(Collider2D other)
    {
        // if draggable starts on the drop zone
        if (!_entered)
        {
            OnTriggerEnter2D(other);
        }
    }

    // Called via unity when a rigidbody with collider exits a trigger collider
    void OnTriggerExit2D(Collider2D other)
    {
	    _entered = false;
		if (!enabled || Target != this)
            return;

        if (other.gameObject == Clickable.Selected)
        {
            SetTarget(null, other.gameObject);
        }
    }



    /// <summary>
    /// Called by Automation to force entering into a drop zone.
    /// </summary>
    /// <param name="source">Source.</param>
    /// <param name="args">Arguments.</param>
    public void RaiseOnEnterDropZone(object source, GameObjectArgs args)
    {
        OnTriggerEnter2D(args.GameObject.GetComponent<Collider2D>());
    }

    /// <summary>
    /// Called by Dragable when dropped on a drop zone.
    /// </summary>
    /// <param name="source">Draggable that called</param>
    /// <param name="args">GameObject of dropped draggable</param>
    public void RaiseOnDropOnZone(object source, GameObjectArgs args)
    {
        OnDropOnZone.RaiseEvent(source, args);
        _oldTarget = Target;
        Target = null;
        _entered = false;
    }

    private void OnDestroy()
    {
        OnEnterDropZone = null;
        OnExitDropZone = null;
        OnDropOnZone = null;
    }
}
