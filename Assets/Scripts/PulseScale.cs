using System;
using DG.Tweening;
using IL.Input;
using UnityEngine;

namespace Assets.IL.Scripts.Effects
{
	public class PulseScale : MonoBehaviour
	{
		[SerializeField]
		private float _interval = 0.3f; //300 ms taken from old flash manager

		private Clickable _clickable;

		private Sequence _sequence;
		private Vector3 _originalScale;

		[Tooltip("If true, then highlighting won't happen without an enabled clickable object.")]
		public bool RequireClickableEnabled;

		private bool CanBePulsed
		{
			get { return (RequireClickableEnabled && _clickable != null && _clickable.enabled) || !RequireClickableEnabled; }
		}

		private void Awake()
		{
			_clickable = gameObject.GetComponent<Clickable>();

			if (RequireClickableEnabled)
		    {
		        _clickable.OnEnableEvent += OnClickableEnableEvent;
		    }
		}

		private void OnClickableEnableEvent(object sender, EventArgs eventArgs)
		{
            OnEnable();
		}

		private void OnEnable()
		{
			if (!CanBePulsed || _sequence != null) return;

			_sequence = DOTween.Sequence();
			_originalScale = gameObject.transform.localScale;
			_sequence.Append(gameObject.transform.DOScale(Vector3.Scale(_originalScale, new Vector3(1.1f, 1.1f, 1.1f)), _interval));
			_sequence.Append(gameObject.transform.DOScale(_originalScale, _interval));
			_sequence.SetLoops(int.MaxValue, LoopType.Restart);
			_sequence.Play();
		}

		private void OnDisable()
		{
			if (_sequence == null) return;

			_sequence.Complete();
			_sequence = null;
			//gameObject.transform.localScale = _originalScale;
			transform.DOScale(_originalScale, 0.2f);
		}

		private void OnDestroy()
		{
            if (_clickable != null)
			    _clickable.OnEnableEvent -= OnClickableEnableEvent;
		}
	}
}
