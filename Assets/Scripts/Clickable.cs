using System;
using System.Collections.Generic;
using System.Linq;
using IL.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IL.Input
{
	public class PointerEventArgs : EventArgs
	{
		public PointerEventData EventData;
		public bool Force = false;
	}

	public class Clickable : MonoBehaviour, IPointerClickHandler, IPointerUpHandler,
		IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler,
		IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public static Vector2 LastClickedScreenLocation;
		public static GameObject LastClicked;

		public static GameObject LastPointerDown;

		public static GameObject LastPointerUp;


	    public static GameObject Selected;

        public static EventHandler<GenericEventArgs<bool>> InputStateChanged;

        public event EventHandler<PointerEventArgs> PointerClick;
		public event EventHandler<PointerEventArgs> PointerDown;
		public event EventHandler<PointerEventArgs> PointerUp;
		public event EventHandler<PointerEventArgs> PointerEnter;
		public event EventHandler<PointerEventArgs> PointerExit;

		public event EventHandler<PointerEventArgs> DragStart;
		public event EventHandler<PointerEventArgs> Drag;
		public event EventHandler<PointerEventArgs> DragEnd;

		public event EventHandler OnDisableEvent;
		public event EventHandler OnEnableEvent;

		[SerializeField]
		private bool _WorksWhenPaused = false;
		[Tooltip("0 or less is no repeat. Else, resend onDown event after X seconds.")]
		[SerializeField]
		private float _repeatDownSignalTime = 0f;

		private Image _uiSprite;
		//private SmartText _smrtText;

		private static List<RaycastResult> _hits;
		private static bool _inputDisabled;

		private bool _isDragging;
		private Timer _downRepeatTimer;

		private static int _currentFingerID = -1;

		// Unity does not respect out InputDisabled flag so we need to watch it for them.
		// Do not start a drag until DragBegin has been fired.
		private bool _dragBeginFired;

		/// <summary>
		/// Should player input be ignored?
		/// </summary>
		public static bool InputDisabled
		{
			set { SetInputStatus(value); }
			get { return _inputDisabled; }
		}

		public static void SetInputStatus(bool disable)
		{
			if (_inputDisabled != disable)
			{
				_inputDisabled = disable;
				ResetInstanceID();
				InputStateChanged.RaiseEvent(null, new GenericEventArgs<bool>(_inputDisabled));
			}
		}

		public bool WorksWhenPaused {
			get { return _WorksWhenPaused; }
		}

		public bool Enabled
		{
			set
			{
				enabled = value;
				if (enabled)
					OnEnableEvent.RaiseEvent(this, null);
				else
				{
					OnDisableEvent.RaiseEvent(this, null);
				}
			}
			get
			{
				return enabled && (_WorksWhenPaused || !_inputDisabled);
			}
		}

#region UnityMethods
        void OnDisable()
		{
			_downRepeatTimer = null;
		}

		void Awake()
		{
			_uiSprite = GetComponent<Image>();
			//_smrtText = GetComponent<SmartText>();
		}
	    // here to draw 'enabled' button in editor
        void Start() {}

		void Update()
		{
			if (_downRepeatTimer != null)
			{
				if (_downRepeatTimer.Tick(Time.unscaledDeltaTime))
				{
					_downRepeatTimer = null;
					OnPointerDown(null);
				}
			}
		}

		void OnDestroy()
		{
			Unwire();
		}
#endregion

        /// <summary>
        /// Remove all event references.
        /// </summary>
        public void Unwire()
		{
			PointerClick = null;
			PointerDown = null;
			PointerUp = null;
			PointerEnter = null;
			PointerExit = null;
			DragStart = null;
			Drag = null;
			DragEnd = null;
			// Do not unwire OnDisableEvent and OnEnableEvent or we lose our ILButton link
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_currentFingerID < 0 && eventData != null)
			{
				_currentFingerID = eventData.pointerId;
			}
			if (!MatchInstanceID(eventData)) return;

		    Selected = gameObject;
            //TimerManager.Instance.RestartTimeoutTimer();
            if (_repeatDownSignalTime > 0)
				_downRepeatTimer = new Timer(_repeatDownSignalTime);
			_isDragging = false;
			if (Enabled && PointerDown != null)
			{
				LastPointerDown = gameObject;
				PointerDown.RaiseEvent(this, new PointerEventArgs { EventData = eventData });
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		    _dragBeginFired = false;
			if (!MatchInstanceID(eventData)) return;
            //Selected = null;

            //TimerManager.Instance.RestartTimeoutTimer();
            _downRepeatTimer = null;
			if (Enabled && PointerUp != null)
			{
				LastPointerUp = gameObject;
				PointerUp.RaiseEvent(this, new PointerEventArgs { EventData = eventData });
			}
        }

		public void OnPointerEnter(PointerEventData eventData)
		{
		    if (_currentFingerID < 0 && eventData != null)
		    {
		        _currentFingerID = eventData.pointerId;
		    }
            if (!MatchInstanceID(eventData)) return;

			if (Enabled && PointerEnter != null)
				PointerEnter.RaiseEvent(this, new PointerEventArgs { EventData = eventData });
        }

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!MatchInstanceID(eventData)) return;

			_downRepeatTimer = null;
			if (Enabled && PointerExit != null)
				PointerExit.RaiseEvent(this, new PointerEventArgs { EventData = eventData });
        }

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!MatchInstanceID(eventData)) return;

			_downRepeatTimer = null;
			//TimerManager.Instance.RestartTimeoutTimer();

			if (_isDragging) return;    // Do not propagate clicks if the item was dragged because you didn't click you dragged which isn't a click

		    Debug.LogFormat("User clicked on {0} '{1}' at {2}",
		        enabled ? "" : "DISABLED", name, eventData.position);
            if (Enabled)
			{
				eventData.selectedObject = gameObject;
				eventData.Use();
				_hits = null;
				LastClicked = gameObject;
				LastClickedScreenLocation = eventData.pressPosition;
				if (PointerClick != null)
					PointerClick.RaiseEvent(this, new PointerEventArgs { EventData = eventData });
			}
			else
				PropegateClick(eventData);
        }


		private void PropegateClick(PointerEventData eventData)
		{
			if (_hits == null || _hits.Count == 0)
			{
				_hits = new List<RaycastResult>();
				EventSystem.current.RaycastAll(eventData, _hits);
				_hits = _hits.Where(x => x.gameObject.GetComponent<IPointerClickHandler>() != null).ToList();
			}
			if (_hits.Count == 0)
				return;
			if (_hits[0].gameObject == gameObject)
				_hits.RemoveAt(0);
			if (_hits.Count > 0)
			{
				EventSystem.current.SetSelectedGameObject(_hits[0].gameObject, eventData);
				_hits[0].gameObject.GetComponent<IPointerClickHandler>().OnPointerClick(eventData);
			}
			else
				_hits = null;
		}

		/// <summary>
		/// Implementation of IBeginDragHandler
		/// </summary>
		public void OnBeginDrag(PointerEventData eventData)
		{
		    if (!Enabled) return;

			_dragBeginFired = true;
			OnBeginDrag(eventData, false);
        }

		public void OnBeginDrag(PointerEventData eventData, bool force)
		{
			if (!MatchInstanceID(eventData)) return;

			_downRepeatTimer = null;
			//TimerManager.Instance.RestartTimeoutTimer();

			//early out if there is no dragable attached so pointerclick will still fire
			if (DragStart == null && Drag == null) return;

		    Debug.LogFormat("User began dragging {0} '{1}' at {2}",
		        enabled ? "" : "DISABLED", name, eventData.position);
            _isDragging = true;
			eventData.selectedObject = gameObject;
			if (Enabled || force)
				DragStart.RaiseEvent(this, new PointerEventArgs { EventData = eventData });
        }

		/// <summary>
		/// Implementation of IDragHandler
		/// </summary>
		public void OnDrag(PointerEventData eventData)
		{
            if (!Enabled || !_dragBeginFired) return;

            OnDrag(eventData, false);
		}

		public void OnDrag(PointerEventData eventData, bool force)
		{
			if (!MatchInstanceID(eventData)) return;

			//TimerManager.Instance.RestartTimeoutTimer();
			if (Enabled || force)
				Drag.RaiseEvent(this, new PointerEventArgs { EventData = eventData });
        }

		/// <summary>
		/// Implementation of IEndDragHandler
		/// </summary>
		public void OnEndDrag(PointerEventData eventData)
		{
		    _dragBeginFired = false;
			OnEndDrag(eventData, false);
		}

		public void OnEndDrag(PointerEventData eventData, bool force)
		{
			if (!MatchInstanceID(eventData)) return;

		    Debug.LogFormat("User end dragging {0} '{1}' at {2}",
		        enabled ? "" : "DISABLED", name, eventData.position);
            //TimerManager.Instance.RestartTimeoutTimer();
			if (Enabled || force)
				DragEnd.RaiseEvent(this, new PointerEventArgs { EventData = eventData });
        }


#region FingerIDStuff
        private static void ResetInstanceID()
		{
            _currentFingerID = -1;
		}

		/// <summary>
		/// Used to only allow one touch operation at a time.
		/// </summary>
		private bool MatchInstanceID(PointerEventData eventData)
		{
			//Cases to allow by default.
			if (eventData == null || UnityEngine.Input.touchCount <= 0)
			{
				return true;
			}

			var pointerId = eventData.pointerId;
			//A pointerID less than 0 is from a mouse button
			if (pointerId < 0)
			{
				return true;
			}

		    return _currentFingerID == pointerId;
		}

		private void LateUpdate()
		{
			//Reset finger ID if it currently isn't being detected
			if (_currentFingerID >= 0 && UnityEngine.Input.touches.All(t => t.fingerId != _currentFingerID))
			{
				ResetInstanceID();
			}
		}
#endregion
    }
}

