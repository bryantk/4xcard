using UnityEngine;

/// <summary>
/// Clamp the object to be within a given X,Y range.
/// </summary>
public class LimitPosition : MonoBehaviour {

	[Tooltip("Maximum achievable X and Y positions.")]
	public Vector2 Max = new Vector2(4, 3);
	[Tooltip("Minimum achievable X and Y positions.")]
	public Vector2 Min = new Vector2(-4, -3);

	// Update is called once per frame
	void Update ()
	{
		var pos = transform.position;
		pos.x = Mathf.Clamp(pos.x, Min.x, Max.x);
		pos.y = Mathf.Clamp(pos.y, Min.y, Max.y);
		transform.position = pos;
	}
}
