using System;
using System.Collections;
using System.Collections.Generic;
using IL.Input;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Clickable))]
public class PropegateClickThrough : MonoBehaviour
{
	private Clickable _clickable;
	private const float HAIR_DISTANCE = 0.01f;

	void Awake()
	{
		_clickable = GetComponent<Clickable>();

		_clickable.PointerClick += OnPointerClick;
	}

	void Start()
	{
		// For enabled in inspector
	}

	private void OnPointerClick(object sender, PointerEventArgs args)
	{
		if (!enabled)
		{
			return;
		}
		var position = args.EventData.pointerPressRaycast.worldPosition;
		position.z = transform.position.z;
		var hits = Physics2D.RaycastAll(position, Vector2.zero, 100, Physics2D.DefaultRaycastLayers,
										position.z + HAIR_DISTANCE);
		for (int i = 0; i < hits.Length; i++)
		{
			var clickable = hits[i].transform.GetComponent<Clickable>();
			if (clickable != null)
			{
				clickable.OnPointerClick(args.EventData);
				return;
			}
		}
	}

}
