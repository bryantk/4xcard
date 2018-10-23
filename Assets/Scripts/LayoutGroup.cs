using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Like the UI layout groups, but not just for UI land
/// </summary>
public class LayoutGroup : MonoBehaviour
{
	/// <summary>
	/// Helper component to position things.
	/// Will run as a component on start, or has public methods to call as needed.
	/// </summary>

	[Tooltip("List of game objects to manage. If none, manages immediate children.")]
	[SerializeField] private List<GameObject> _objects;
	[Tooltip("Direction to layout objects. (X,Y) -1 for left / down. 1 for right / up.")]
	public Vector2 LayoutDirection;
	[Tooltip("Unity units to pad between objects.")]
	public float Padding = 0;
    [Tooltip("Should this shuffle up the game objects locations (swap one with another)?")]
	[SerializeField] private bool _shuffleLocations;

	[Tooltip(("Center the layout group on the parent object"))]
	[SerializeField] private bool _centerOnParent; 
	
	[SerializeField] private Vector2 _minimumBounds;

	[SerializeField] private Vector2 _offset = Vector2.zero;

	// TODO - does this component even need a start method? or should it do it all via calls?
	void Start () {
		if (_objects != null && _objects.Count == 0)
		{
			_objects = gameObject.transform.GetChildren().Select(x => x.gameObject).ToList();
		}
		if (LayoutDirection != Vector2.zero)
			Layout(_objects, LayoutDirection, Padding, _centerOnParent);
		else if (_shuffleLocations)
		{
			ShuffleLocations(_objects);
		}
	}

	public void Layout()
	{
		Layout(_objects, LayoutDirection, Padding);
	}

	public void Layout(List<GameObject> objects)
	{
		Layout(objects, LayoutDirection, Padding);
	}

	/// <summary>
	/// Layout the given objects in given direction with the given padding.
	/// </summary>
	/// <param name="objects"></param>
	/// <param name="direction"></param>
	/// <param name="padding"></param>
	/// <param name="centerOnParent"></param>
	public void Layout(List<GameObject> objects, Vector2 direction, float padding, bool centerOnParent = false)
	{
		if (direction == Vector2.zero) return;
		Vector3 position = _offset;
		foreach (var obj in objects)
		{
			Vector2 size = Vector2.zero;
			Vector2 pivot = Vector2.zero;
			GetSize(obj, out size, out pivot);
			size = new Vector2(Mathf.Max(size.x, _minimumBounds.x), 
				Mathf.Max(size.y, _minimumBounds.y));
			if (size.x <= 0 || size.y <= 0)
			{
				continue;
			}
			// Get unity units to position the object in the given direction
			size = Vector3.Scale(size, direction);
			// Pivot is normalized 0 (bottom left) to 1(top right)...
			// which means when direction is negative, bad things happen.
			// We invert pivot points when direction is negative.
			pivot = new Vector2(
				direction.x < 0 ? Mathf.Abs(1 - pivot.x) : pivot.x,
				direction.y < 0 ? Mathf.Abs(1 - pivot.y) : pivot.y);
			// normalized (0 to 1) pivot to unity unity based.
			pivot = Vector3.Scale(pivot, size);
			obj.transform.localPosition = position + (Vector3)pivot;
			position += (Vector3)size + (Vector3)direction * padding;
		}
		if (centerOnParent)
		{
			//Use position as total size of layout (minus last padding)
			position -= (Vector3)direction * padding;
			var dif = position/2;
			foreach (var obj in objects)
			{
				obj.transform.localPosition -= dif;
			}
		}
	}

	/// <summary>
	/// Given a game object, attempt to find its size in unity units and center pivot (normalized 0 to 1) 
	/// </summary>
	private void GetSize(GameObject obj, out Vector2 size, out Vector2 pivot)
	{
		size = Vector2.zero;
		pivot = Vector2.one * 0.5f; 
		var sr = obj.GetComponent<SpriteRenderer>();
		if (sr != null)
		{
			var s = sr.sprite;
			size = new Vector2(s.texture.width / s.pixelsPerUnit, s.texture.height / s.pixelsPerUnit);
			size = Vector3.Scale(size, obj.transform.localScale);
			pivot = new Vector2(s.pivot.x / s.texture.width, s.pivot.y / s.texture.height);
			return;
		}
		var rect = obj.GetComponent<RectTransform>();
		if (rect != null)
		{
			size = rect.sizeDelta;
			pivot = rect.pivot;
			return;
		}
		Debug.LogWarning("Unknown object: " + obj.name);
	}

	/// <summary>
	/// Shuffle the locations of the given game objects.
	/// </summary>
	/// <returns>List of GameObjects in their new order</returns>
	public List<GameObject> ShuffleLocations (List<GameObject> objects) {
		var gameObjects = objects.Select(obj => new { obj.transform.position, obj }).ToList();
		gameObjects.Shuffle();

        var newOrder = new List<GameObject>();
		for (int i = 0; i < objects.Count; i++)
		{
			objects[i].transform.position = gameObjects[i].position;
            newOrder.Add(gameObjects[i].obj);
		}
        return newOrder;
	}
}
