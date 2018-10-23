using System;
using Assets.IL.Scripts.Common;
using DG.Tweening;
using UnityEngine;

namespace IL.Input
{

	[RequireComponent(typeof(Clickable))]
	public class Dragable : MonoBehaviour
	{
        [UnityEngine.Tooltip("If not dropped on a Drop Zone, will return to starting position using a tween")]
		public bool ShouldReturnToStartPosition = true;
        [UnityEngine.Tooltip("What should the draggable do when dropped on a drop zone?")]
        public DropZoneBehavior  dropZoneBehavior = DropZoneBehavior.RETURN_TO_START_POSITION;
        [UnityEngine.Tooltip("Upon Click and/or Drag, move the center of the clickable onto mouse position.")]
        public bool _snapToCenter;
        [UnityEngine.Tooltip("Time it takes for draggable to return to starting position.")]
        public float ReturnTime = 0.25f;
		[UnityEngine.Tooltip("Offset the Z-Position of the draggable object by this amount during drag move.")]
		public float ZOffset;
		[UnityEngine.Tooltip("Offset the Drop Z-Position of the draggable object by this amount on drop.")]
		public float DropZOffset;
		[UnityEngine.Tooltip("If non-Zero, only permit drag movement along this vector.")]
		public Vector2 ConstraintVector = Vector2.zero;
        [UnityEngine.Tooltip("If non-Zero, only permit drag movement along this vector.")]
        public Vector2 DropOffsets = Vector2.zero;

		[UnityEngine.Tooltip("Make the collider smaller during drag. Can be used in conjuction with snaptocenter")]
		public float ColliderScaleDuringDrag = 1;

		public bool InReturnTween
	    {
	        get { return _returnTween != null && _returnTween.IsPlaying(); }
	    }

		private Vector3 _startPosition;
		private Vector3 _startClick;
		public static GameObject Selected;
	    private Clickable _clickable;
		private Tween _returnTween;
	    private Tween _centerTween;
		private BoxCollider2D _boxCollider2D;
		private CircleCollider2D _circleCollider2D;

		private Vector2 _boxColliderSize;
		private float _circleColliderRadius;

		public Clickable Clickable { get { return _clickable; } }

		public Vector3 StartPosition
		{
			get { return _startPosition; }
            set { _startPosition = value; }
		}

		public event EventHandler<GameObjectArgs> OnDroppedNotOnDropZone;

	    void Awake()
	    {
		    _boxCollider2D = GetComponent<BoxCollider2D>();
		    _circleCollider2D = GetComponent<CircleCollider2D>();
			_clickable = GetComponent<Clickable>();

			// Vector based movement does not work with center snap
		    _snapToCenter = _snapToCenter && ConstraintVector == Vector2.zero;
	    }

	    void OnEnable()
		{
            _clickable.DragStart += OnDragStartEvent;
            _clickable.Drag += OnDragMoveEvent;
            _clickable.DragEnd += OnDragEndEvent;

			if (_boxCollider2D != null)
			{
				_boxColliderSize = _boxCollider2D.size;
			}

			if (_circleCollider2D != null)
			{
				_circleColliderRadius = _circleCollider2D.radius;
			}
		}

		void OnDisable()
		{
            _clickable.DragStart -= OnDragStartEvent;
            _clickable.Drag -= OnDragMoveEvent;
            _clickable.DragEnd -= OnDragEndEvent;
		}

		public void OnDragStartEvent(object sender, PointerEventArgs e)
		{
            if (!enabled || InReturnTween)
                return;
            Selected = gameObject;
			_startClick = Camera.main.ScreenToWorldPoint(e.EventData.position);
			_startPosition = transform.position;

			ResizeCollider(true);

			if (_snapToCenter)
			{
				var target = _startClick;
				target.z = transform.position.z + ZOffset;
				transform.position = target;
			}
			e.EventData.Use();
		}

		public void ResizeCollider(bool setToDragScale)
		{
			// ResizeColliderOnDrag is to replace the use of 'ColliderScaleDuringDrag'.
			// Honor it first!
			if (GetComponent<ResizeColliderOnDrag>() != null) return;

			if (ColliderScaleDuringDrag == 0 || ColliderScaleDuringDrag == 1 ) return;

			var colliderScale = setToDragScale ? ColliderScaleDuringDrag : 1;
			if (_boxCollider2D != null)
			{
				_boxCollider2D.size = _boxColliderSize * colliderScale;
			}

			if (_circleCollider2D != null)
			{
				_circleCollider2D.radius = _circleColliderRadius * colliderScale;
			}
		}

		public void OnDragMoveEvent(object sender, PointerEventArgs e)
		{
			if (!enabled || Selected == null)
				return;
			var target = Camera.main.ScreenToWorldPoint(e.EventData.position);
			if (_snapToCenter)
			{
				var z = transform.position.z;
				target.z = z;
				transform.position = target;
			}
			else
			{
				var delta = target - _startClick;

				//z should be set by the provided ZOffset
				delta.z = 0;

				// If we want the dragable to be constrained to a direction,
				// project our movement diff over the desired vector direction
				// https://docs.unity3d.com/ScriptReference/Vector3.Project.html
				if (ConstraintVector != Vector2.zero)
				{
					delta = Vector3.Project(delta, ConstraintVector);
				}
				transform.position = _startPosition + new Vector3(0, 0, ZOffset) + delta;
			}
			e.EventData.Use();
		}

		public void OnDragEndEvent(object sender, PointerEventArgs e)
		{
			if (!enabled || Selected == null)
				return;
		    Selected = null;
			if (_snapToCenter)
			{
				var target = transform.position;
				target.z = transform.position.z - ZOffset;
				transform.position = target;
			}

			ResizeCollider(false);

			if (DropZone.Target != null)
		    {
		        DropZone.Target.RaiseOnDropOnZone(this, new GameObjectArgs {GameObject = gameObject, Triggered = DropZone.Target.gameObject });
		        switch (dropZoneBehavior)
		        {
		            case DropZoneBehavior.RETURN_TO_START_POSITION:
		                ReturnToStartPosition();
		                break;
                    case DropZoneBehavior.INSTANTLY_CENTER_ON_DROP_ZONE:
		                MoveToCenterOfDropZone(false, false);
		                break;
                    case DropZoneBehavior.STAY_WHERE_DROPPED:
		                break;
                    case DropZoneBehavior.TWEEN_TO_CENTER_OF_DROP_ZONE:
		                MoveToCenterOfDropZone(false, true);
		                break;
                    default:
		                ReturnToStartPosition();
		                break;
		        }
		    }
		    else
		    {
				OnDroppedNotOnDropZone.RaiseEvent(this, new GameObjectArgs {GameObject = gameObject, Triggered = null });
		        ReturnToStartPosition();
		    }
		    e.EventData.Use();
		}

		public void ReturnToStartPosition(bool forceReturn = false, bool withTween = true)
		{
			Selected = null;
			if (forceReturn || ShouldReturnToStartPosition) {
				if (_returnTween != null)
					_returnTween.Kill();
			    if (_centerTween != null)
			        _centerTween.Kill();
				_returnTween = transform.DOMove(_startPosition, ReturnTime);
				if (!withTween)
					_returnTween.Kill(true);
			    ResizeCollider(false);
			}
		}

		public void MoveToCenterOfDropZone(bool forceMove = false, bool useTween = true)
	    {
	        Selected = null;
	        if (forceMove || dropZoneBehavior == DropZoneBehavior.INSTANTLY_CENTER_ON_DROP_ZONE ||
	            dropZoneBehavior == DropZoneBehavior.TWEEN_TO_CENTER_OF_DROP_ZONE)
	        {
				if (_centerTween != null)
	                _centerTween.Kill();

                var targetPosition = DropZone._oldTarget.transform.position;
                targetPosition.z = _startPosition.z + DropZOffset;  // Set target z-position to drop z-position
                targetPosition.x += DropOffsets.x;
                targetPosition.y += DropOffsets.y;
				//move at the same rate no matter the distance to center.
				_centerTween = transform.DOMove(targetPosition, (Vector2.Distance(transform.position, targetPosition))/4f);
	            if (!useTween)
	                _centerTween.Kill(true);
	        }
	    }
	}
}
