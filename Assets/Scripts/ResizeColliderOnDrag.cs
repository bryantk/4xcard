using IL.Input;
using UnityEngine;

/// <summary>
/// Scale the given 2d Collider by 'ResizeMagnitude' dimensions on drag start.
/// Revert to given collider bounds on drag end.
/// A more robust replacement of Dragable.ColliderScaleDuringDrag
/// </summary>
[RequireComponent(typeof(Dragable))]
public class ResizeColliderOnDrag : MonoBehaviour
{
	[Tooltip("Scale collider bounds. Circle radius will be scaled by the magnitude of this V2.")]
	public Vector2 ResizeMagnitude = Vector2.one;

	private Clickable _clickable;

	private BoxCollider2D _boxCollider2D;
	private CircleCollider2D _circleCollider2D;
	private CapsuleCollider2D _capsuleCollider2D;

	private Vector2 _colliderSize;
	private float _colliderRadius;

	void Awake()
	{
		// Look for all, but only resize the first found (in this order)
		_boxCollider2D = GetComponent<BoxCollider2D>();
		_circleCollider2D = GetComponent<CircleCollider2D>();
		_capsuleCollider2D = GetComponent<CapsuleCollider2D>();
		_clickable = GetComponent<Clickable>();
	}

	void OnEnable()
	{
		_clickable.DragStart += OnDragStart;
		_clickable.DragEnd += OnDragEnd;
		// Set a default start size
		if (_boxCollider2D != null)
		{
			_colliderSize = _boxCollider2D.size;
		}
		if (_circleCollider2D != null)
		{
			_colliderRadius = _circleCollider2D.radius;
		}
		if (_capsuleCollider2D != null)
		{
			_colliderSize = _capsuleCollider2D.size;
		}
	}

	void OnDisable()
	{
		_clickable.DragStart -= OnDragStart;
		_clickable.DragEnd -= OnDragEnd;
	}


	private void OnDragStart(object sender, PointerEventArgs pointerEventArgs)
	{
		// Do NOT permit a 0 dimension collider.
		if (_boxCollider2D != null)
		{
			_colliderSize = _boxCollider2D.size;
			_boxCollider2D.size = new Vector2(
				Mathf.Max(_colliderSize.x * ResizeMagnitude.x, 0.01f),
				Mathf.Max(_colliderSize.y * ResizeMagnitude.y, 0.01f));
			return;
		}

		if (_circleCollider2D != null)
		{
			_colliderRadius = _circleCollider2D.radius;
			_circleCollider2D.radius = Mathf.Max(_colliderRadius * ResizeMagnitude.magnitude, 0.01f);
			return;
		}

		if (_capsuleCollider2D != null)
		{
			_colliderSize = _capsuleCollider2D.size;
			_capsuleCollider2D.size = new Vector2(
				Mathf.Max(_colliderSize.x * ResizeMagnitude.x, 0.01f),
				Mathf.Max(_colliderSize.y * ResizeMagnitude.y, 0.01f));
			return;
		}
	}

	public void Revert()
	{
		OnDragEnd(null, null);
	}

	private void OnDragEnd(object sender, PointerEventArgs pointerEventArgs)
	{
		if (_boxCollider2D != null)
		{
			_boxCollider2D.size = _colliderSize;
			return;
		}

		if (_circleCollider2D != null)
		{
			_circleCollider2D.radius = _colliderRadius;
			return;
		}

		if (_capsuleCollider2D != null)
		{
			_capsuleCollider2D.size = _colliderSize;
			return;
		}
	}
}
