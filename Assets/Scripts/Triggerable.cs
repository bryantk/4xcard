using System;
using UnityEngine;

namespace IL.Input
{
	public class TriggerableEventArgs : EventArgs
	{
		public GameObject OtherGameObject;
	}

	[RequireComponent(typeof(Collider2D))]
	public class Triggerable : MonoBehaviour
	{
		public event EventHandler<TriggerableEventArgs> TriggerEnter2DEvent;
		public event EventHandler<TriggerableEventArgs> TriggerExit2DEvent;
		public event EventHandler<TriggerableEventArgs> TriggerStay2DEvent;

		void Start()
		{
			// here to draw 'enabled' button in editor
		}
		
		void OnDestroy()
		{
			TriggerEnter2DEvent = null;
			TriggerExit2DEvent = null;
			TriggerStay2DEvent = null;
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (enabled)
				TriggerEnter2DEvent.RaiseEvent(this, new TriggerableEventArgs { OtherGameObject = other.gameObject });
		}

		void OnTriggerExit2D(Collider2D other)
		{
			if (enabled)
				TriggerExit2DEvent.RaiseEvent(this, new TriggerableEventArgs { OtherGameObject = other.gameObject });
		}

		void OnTriggerStay2D(Collider2D other)
		{
			if (enabled)
				TriggerStay2DEvent.RaiseEvent(this, new TriggerableEventArgs { OtherGameObject = other.gameObject });
		}
	}

}

