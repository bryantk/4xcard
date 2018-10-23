using System;
using Assets.IL.Scripts.Common;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IL.Input
{

	[RequireComponent(typeof(Clickable))]
	public class DragableCard : MonoBehaviour
	{
	    public CardType Card;

        public static float ReturnSpeed = 20f;

		[Tooltip("Offset the Z-Position of the draggable object by this amount during drag move.")]
		private const float ZOffset = -3f;
		[Tooltip("Offset the Drop Z-Position of the draggable object by this amount on drop.")]
		public float DropZOffset;

        [Tooltip("If non-Zero, only permit drag movement along this vector.")]
        public Vector2 DropOffsets = Vector2.zero;

		[Tooltip("Make the collider smaller during drag. Can be used in conjuction with snaptocenter")]
		public float ColliderScaleDuringDrag = 1;

		public bool InReturnTween
	    {
	        get { return returnTween != null && returnTween.IsPlaying(); }
	    }
        [HideInInspector]
		public Vector3 StartPosition;
		private Vector3 startClick;

	    private Clickable clickable;
		private Tween returnTween;
	    private Tween centerTween;
		private BoxCollider2D boxCollider2D;
		private Vector2 boxColliderSize;
	    private Vector2 boxColliderScaled;

        public event EventHandler<GameObjectArgs> OnDroppedNotOnDropZone;

	    void Awake()
	    {
		    boxCollider2D = GetComponent<BoxCollider2D>();
			clickable = GetComponent<Clickable>();
	    }

	    void OnEnable()
		{
            clickable.DragStart += OnDragStartEvent;
            clickable.Drag += OnDragMoveEvent;
            clickable.DragEnd += OnDragEndEvent;

			if (boxCollider2D != null)
			{
				boxColliderSize = boxCollider2D.size;
			    boxColliderScaled = boxColliderSize * ColliderScaleDuringDrag;

			}
		}

		void OnDisable()
		{
            clickable.DragStart -= OnDragStartEvent;
            clickable.Drag -= OnDragMoveEvent;
            clickable.DragEnd -= OnDragEndEvent;
		}

		public void OnDragStartEvent(object sender, PointerEventArgs e)
		{
            if (!enabled || InReturnTween)
                return;
			startClick = Camera.main.ScreenToWorldPoint(e.EventData.position);
			StartPosition = transform.position;

			ResizeCollider(true);

			e.EventData.Use();
		}

		public void ResizeCollider(bool scale)
		{
			// ResizeColliderOnDrag is to replace the use of 'ColliderScaleDuringDrag'.
			// Honor it first!
			if (GetComponent<ResizeColliderOnDrag>() != null) return;

			if (boxCollider2D != null)
			{
				boxCollider2D.size = scale ? boxColliderScaled : boxColliderSize;

			}
		}

		public void OnDragMoveEvent(object sender, PointerEventArgs e)
		{
			if (!enabled || Clickable.Selected == null)
				return;
			var target = Camera.main.ScreenToWorldPoint(e.EventData.position);

			var delta = target - startClick;
		    delta.z = 0;
			//z should be set by the provided ZOffset
            // TODO - move this to on pointer down.
		    target = StartPosition + delta;
		    target.z = ZOffset;
            transform.position = target;

			e.EventData.Use();
		}

		public void OnDragEndEvent(object sender, PointerEventArgs e)
		{
			if (!enabled || Clickable.Selected == null)
				return;

			ResizeCollider(false);

			if (DropZone.Target != null)
		    {

                // TODO - if not valid or in use
		        if ((Card & DropZone.Target.ExpectedCardType) == 0)
		        {
		            DropZone.Target = null;
                    ReturnToStartPosition();
                }
                else
		        {
		            DropZone.Target.RaiseOnDropOnZone(this, new GameObjectArgs { GameObject = gameObject, Triggered = DropZone.Target.gameObject });

                    MoveToCenterOfDropZone();
                }
		    }
		    else
		    {
				OnDroppedNotOnDropZone.RaiseEvent(this, new GameObjectArgs {GameObject = gameObject, Triggered = null });

                // TODO - box cast to find highest position.
		        if (returnTween != null)
		            returnTween.Kill();
		        returnTween = transform.DOMoveZ(StartPosition.z, ReturnSpeed).SetSpeedBased(true);
		        DropHere();

		    }
		    e.EventData.Use();
		}

	    private void DropHere()
	    {
            // TODO - box cast to find highest position.
	        var z = StartPosition.z;
	        StartPosition = transform.position;
	        StartPosition.z = z;
	        if (returnTween != null)
	            returnTween.Kill();
	        returnTween = transform.DOMoveZ(z, ReturnSpeed).SetSpeedBased(true);
        }

        public void ReturnToStartPosition(bool withTween = true)
        {
            if (Clickable.Selected == gameObject)
		        Clickable.Selected = null;
            Debug.LogWarning("go home");
            if (returnTween != null)
				returnTween.Kill();
			if (centerTween != null)
			    centerTween.Kill();
			returnTween = transform.DOMove(StartPosition, ReturnSpeed).SetSpeedBased(true);
			if (!withTween)
				returnTween.Kill(true);
			ResizeCollider(false);
		}

		public void MoveToCenterOfDropZone(bool useTween = true)
	    {
	        if (Clickable.Selected == gameObject)
	            Clickable.Selected = null;

            if (centerTween != null)
	            centerTween.Kill();
            Debug.LogWarning("set on dZ");
            var targetPosition = DropZone._oldTarget.transform.position;
            targetPosition.z = StartPosition.z + DropZOffset;  // Set target z-position to drop z-position
            targetPosition.x += DropOffsets.x;
            targetPosition.y += DropOffsets.y;
	        if (useTween)
	        {
	            //move at the same rate no matter the distance to center.
	            centerTween = transform.DOMove(targetPosition, (Vector2.Distance(transform.position, targetPosition)) / 4f);
	        }
	        else
	        {
	            transform.position = targetPosition;
	        }

	    }
	}
}
