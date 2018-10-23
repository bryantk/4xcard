using System;
using Assets.IL.Scripts.Common;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace IL.Input
{
	public class ColorArgs : EventArgs
	{
		public Color color;
	}

	[RequireComponent(typeof(Clickable))]
	public class ILButton : MonoBehaviour
	{
		public event EventHandler<ColorArgs> OnColorChange;

		[Header("Normal")]
		[SerializeField]
		private Sprite _normalSprite;
		public Color NormalColor = Color.white;
		[SerializeField]
		private Vector3 _normalScale = Vector3.one;
		[Header("Hover")]
		[SerializeField]
		private Sprite _hoverSprite;
		[SerializeField]
		private Color _hoverColor = new Color(0.455f, 0.969f, 1);
		[FormerlySerializedAs("_hoverScale")]
		public Vector3 HoverScale = Vector3.one;
		[Header("Press")]
		[SerializeField]
		private Sprite _pressSprite;
		[SerializeField]
		private Color _pressColor = new Color(0.369f, 0.769f, 0.792f);
		[SerializeField]
		private Vector3 _pressScale = Vector3.one;
		[Header("Selected")]
		[SerializeField]
		private Sprite _selectedSprite;
		[SerializeField]
		private Color _selectedColor = new Color(0.369f, 0.769f, 0.792f);
		[SerializeField]
		private Vector3 _selectedScale = Vector3.one;
		[Header("Disabled")]
		[SerializeField]
		private Sprite _disabledSprite;
		[SerializeField]
		private Color _disabledColor = Color.grey;
		[SerializeField]
		private Vector3 _disabledScale = Vector3.one;

		[Tooltip("Use the Unity defined sprite and color for IL Button normal?")]
		[SerializeField]
		private bool _useOriginalAsNormal = true;
		[Tooltip("Fallback to NormalSprite/renderer sprite if none given in 'Hover', 'Pressed', and/or 'Disable'?")]
		[SerializeField]
		private bool _useNormalSpriteIfNull = true;
		[Tooltip("Multiply colors by source rather than replace?")]
		[SerializeField]
		private bool _multiplicitive = false;

		[Tooltip("Multiply each button state scale by the original transform scale")]
		[SerializeField]
		private bool _multiplyScale = true;

		[Tooltip("Use the selection sprite/color/scale when picking a button. Only works if this object is a child of another.")]
		[SerializeField]
		private bool _useSelectionState = false;
		[Tooltip("Forces 'Selectable' to true. Toggles between selected and not.")]
		[SerializeField]
		private bool _toggle = false;
		[Tooltip("Isolate the button, exclude it from a hierarchical group.")]
		[SerializeField]
		private bool _excludeFromGroup = false;

	    [Header("3D Options")]
        [Tooltip("Treat all child Mesh Renderers as a part of this button.")]
        [SerializeField]
        private bool _includeChildMeshRenderers = false;

	    [Tooltip("Material index to affect. (-1 = ALL).")]
        [SerializeField]
        private int _materialIndex = -1;

	    private int _shaderColorId;

        private Clickable _click;
		private static bool _mouseDown;
		private bool _mouseOver;
		private bool _hasDragable;
		// possible renderers
		private SpriteRenderer _sr;
		private Image _image;
		private TMP_Text _tmp;
	    private List<Renderer> _meshRenderers = new List<Renderer>();
		private Vector3 _startingNormalScale;
		private Vector3 _startingHoverScale;
		private Vector3 _startingPressScale;
	    private Vector3 _startingSelectedScale;
        private Vector3 _startingDisabledScale;

		private static Dictionary<Transform, ILButton> _lastSelectedDictionary = new Dictionary<Transform, ILButton>();

	    private ILButtonState _state;
		private bool _isDragging;

		public ILButtonState ButtonState
	    {
	        get { return _state; }
	    }

		[ContextMenu("Set all to Sprite")]
		public void SetSprite()
		{
			var sr = GetComponent<SpriteRenderer>();
			if (sr == null) return;

			_normalSprite = sr.sprite;
			_hoverSprite = sr.sprite;
			_disabledSprite = sr.sprite;
			name = sr.sprite.name;
		}

        public Color HoverColor { get { return _hoverColor; } }

		// Use this for initialization
		void Awake ()
		{
		    _shaderColorId = Shader.PropertyToID("_Color");
            _useSelectionState = _useSelectionState || _toggle;
			_sr = GetComponent<SpriteRenderer>();
		    if (_includeChildMeshRenderers)
		    {
                _meshRenderers = GetComponentsInChildren<MeshRenderer>().Cast<Renderer>().ToList();
		        _meshRenderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>().Cast<Renderer>().ToArray());
            }
		    else
		    {
                _meshRenderers = GetComponents<MeshRenderer>().Cast<Renderer>().ToList();
                _meshRenderers.AddRange(GetComponents<SkinnedMeshRenderer>().Cast<Renderer>().ToArray());
		    }
		        
			_image = GetComponent<Image>();
			_click = GetComponent<Clickable>();

			_tmp = GetComponent<TMP_Text>();

			_hasDragable = GetComponent<Dragable>() != null;

			_click.PointerDown += OnPressDown;
			_click.PointerUp += OnPressUp;
			_click.PointerClick += OnPointerClick;
			_click.PointerEnter += OnEnter;
			_click.PointerExit += OnExit;
			_click.OnDisableEvent += OnClickOnDisable;
			_click.OnEnableEvent += OnClickEnabled;


			if (_useOriginalAsNormal)
			{
				NormalColor = MyColor;
				if (_sr != null)
				{
					_normalSprite = _sr.sprite;
				}
				else if (_image != null)
				{
					_normalSprite = _image.sprite;
				}
				else
				{
					_normalSprite = null;
				}
			}
			if (_useNormalSpriteIfNull)
			{
				_hoverSprite = _hoverSprite == null ? _normalSprite : _hoverSprite;
				_pressSprite = _pressSprite == null ? _normalSprite : _pressSprite;
				_selectedSprite = _pressSprite == null ? _normalSprite : _pressSprite;
				_disabledSprite = _disabledSprite == null ? _normalSprite : _disabledSprite;
			}
			if (_multiplicitive)
			{
				NormalColor = MyColor;
			}

			_startingNormalScale = _normalScale;
			_startingHoverScale = HoverScale;
			_startingPressScale = _pressScale;
			_startingSelectedScale = _selectedScale;
			_startingDisabledScale = _disabledScale;

			if (_multiplyScale)
			{
				UpdateScale();
			}
		}

		public bool MultiplyScale { set { _multiplyScale = value; } }

		public void UpdateScale()
		{
			_normalScale = Vector3.Scale(gameObject.transform.localScale, _startingNormalScale);
			HoverScale = Vector3.Scale(gameObject.transform.localScale, _startingHoverScale);
			_pressScale = Vector3.Scale(gameObject.transform.localScale, _startingPressScale);
			_disabledScale = Vector3.Scale(gameObject.transform.localScale, _startingDisabledScale);
			_selectedScale = Vector3.Scale(gameObject.transform.localScale, _startingSelectedScale);

		}

		public Color MyColor
		{
			get
			{
				if (_sr != null)
					return _sr.color;
				if (_image != null)
					return _image.color;
				if (_tmp != null)
					return _tmp.color;
			    if (_meshRenderers.Count > 0 && _meshRenderers[0].material.HasProperty(_shaderColorId))
			        return _meshRenderers[0].material.color;
				return NormalColor;
			}
			set
			{
				if (_sr != null)
					_sr.color = value;
				else if (_image != null)
					_image.color = value;
				else if (_tmp != null)
					_tmp.color = value;
                else if (_meshRenderers.Count > 0)
                    SetMeshColors(value);
			}
		}

	    public void SetMeshColors(Color color)
	    {
            if (_meshRenderers.Count <= 0)
                return;
	        foreach (Renderer rend in _meshRenderers)
	        {
	            if (_materialIndex != -1 && rend.materials.Length > _materialIndex)
	            {
                    if (rend.materials[_materialIndex].HasProperty(_shaderColorId))
	                    rend.materials[_materialIndex].color = color;
	            }
	            else
	            {
	                foreach (Material mat in rend.materials)
                        if (mat.HasProperty(_shaderColorId))
	                        mat.color = color;
	            }
	        } 
	    }

		/// <summary>
		/// Update the default color for the button
		/// </summary>
		/// <param name="color"></param>
		public void ChangeNormalColor(Color color)
		{
			MyColor = color;
			NormalColor = color;
			NormalState();
		}

		/// <summary>
		/// Update the disabled color for the button
		/// </summary>
		/// <param name="color"></param>
		public void ChangeDisabledColor(Color color)
		{
			_disabledColor = color;
		}

		/// <summary>
		/// Update the hover color for the button
		/// </summary>
		/// <param name="color"></param>
		public void ChangeHoverColor(Color color)
		{
			_hoverColor = color;
		}

		/// <summary>
		/// Update the pressed color for the button
		/// </summary>
		/// <param name="color"></param>
		public void ChangePressedColor(Color color)
		{
			_pressColor = color;
		}


		void OnDestroy()
		{
			OnColorChange = null;
			if (_click == null)
				return;
			_click.PointerDown -= OnPressDown;
			_click.PointerUp -= OnPressUp;
			_click.PointerClick -= OnPointerClick;
			_click.PointerEnter -= OnEnter;
			_click.PointerExit -= OnExit;
		}

		void OnDisable()
		{
			if (InButtonGroup() && _state == ILButtonState.Selected && GetLastSelectedInGroup() == this)
			{
				var kvp = _lastSelectedDictionary.First(x => x.Value == this);
				_lastSelectedDictionary.Remove(kvp.Key);
			}
			DisabledState();
		}

		void OnEnable()
		{
			if (_click.enabled)
			{
				NormalState();
			}
			else
				DisabledState();
		}

		private ILButton GetLastSelectedInGroup()
		{
			if (!InButtonGroup())
			{
				return null;
			}
			ILButton lastSelected;
			_lastSelectedDictionary.TryGetValue(transform.parent, out lastSelected);
			return lastSelected;
		}

		private void OnClickOnDisable(object sender, EventArgs e)
		{
			enabled = false;
		}

		private void OnClickEnabled(object sender, EventArgs e)
		{
			if (enabled)
			{
				OnEnable();
			}
			enabled = true;
		}
		
		/// <summary>
		/// Is this button in a group?
		/// </summary>
		/// <returns></returns>
		public bool InButtonGroup() 
		{
			return !_excludeFromGroup && transform.parent != null;
		}

		/// <summary>
		/// Set all modes to the given sprite.
		/// </summary>
		/// <param name="sprite"></param>
		public void SetAllSprites(Sprite sprite)
		{
			_normalSprite = sprite;
			_hoverSprite = sprite;
			_pressSprite = sprite;
			_disabledSprite = sprite;
			_selectedSprite = sprite;

			NormalState();
		}

		public void SetAllScale(Vector3 scale)
		{
			_normalScale = scale;
			HoverScale = scale;
			_pressScale = scale;
			_disabledScale = scale;
			_selectedScale = scale;
		}

	    public void SetStateScale(ILButtonState state, Vector3 scale)
	    {
	        switch (state)
	        {
                case ILButtonState.Disable:
                    _disabledScale = scale;
                    _startingDisabledScale = scale;
                    break;
                case ILButtonState.Hover:
                    HoverScale = scale;
                    _startingHoverScale = scale;
                    break;
                case ILButtonState.Normal:
                    _normalScale = scale;
                    _startingNormalScale = scale;
                    break;
                case ILButtonState.Press:
                    _pressScale = scale;
                    _startingPressScale = scale;
                    break;
                case ILButtonState.Selected:
                    _selectedScale = scale;
                    _startingSelectedScale = scale;
                    break;
	            default:
	                Debug.LogWarning("ILButtonVisualState: Use of unsupported state [" + state + "]");
	                break;
            }
	    }

	    /// <summary>
	    /// Set each sprite state.
	    /// </summary>
	    /// <param name="normal"></param>
	    /// <param name="hover"></param>
	    /// <param name="press"></param>
	    /// <param name="disabled"></param>
	    /// <param name="selected"></param>
	    public void SetSpritesStates(Sprite normal, Sprite hover = null, Sprite press = null, Sprite disabled = null, Sprite selected = null)
		{
			_normalSprite = normal;
			if (hover != null) _hoverSprite = hover;
			if (press != null) _pressSprite = press;
			if (disabled != null) _disabledSprite = disabled;
			if (selected != null) _selectedSprite = selected;
			NormalState();
		}

		private void LerpTo(Sprite sprite, Color color, Vector3 scale)
		{
			if (_sr != null)
			{
				_sr.sprite = sprite;
			}
			else if (_image != null)
			{
				_image.sprite = sprite;
			}
			if (_multiplicitive)
			{
				color = NormalColor * color;
			}
			MyColor = color;
			var args = new ColorArgs
			{
				color = color,
			};
			OnColorChange.RaiseEvent(this, args);
			transform.localScale = scale;
		}

		/// <summary>
		/// Set visual state of button to 'Normal'.
		/// Cannot guarantee effect if player is actively interacting with element.
		/// </summary>
		public void NormalState()
		{
			_state = ILButtonState.Normal;
			LerpTo(_normalSprite, NormalColor, _normalScale);
		}

		/// <summary>
		/// Set visual state of button to 'Hover'.
		/// Cannot guarantee effect if player is actively interacting with element.
		/// </summary>
		public void HoverState()
		{
			_state = ILButtonState.Hover;
			LerpTo(_hoverSprite, _hoverColor, HoverScale);
		}

		/// <summary>
		/// Set visual state of button to 'Pressed'.
		/// Cannot guarantee effect if player is actively interacting with element.
		/// </summary>
		public void PressState()
		{
			_state = ILButtonState.Press;
			LerpTo(_pressSprite, _pressColor, _pressScale);
		}

		/// <summary>
		/// Set visual state of button to 'Selected'.
		/// Cannot guarantee effect if player is actively interacting with element.
		/// </summary>
		public void SelectedState()
		{
			ILButton lastSelected = GetLastSelectedInGroup();
			if (lastSelected != null && lastSelected != this && lastSelected.ButtonState == ILButtonState.Selected)
			{
				lastSelected.NormalState();
			}
			_state = ILButtonState.Selected;
			if (InButtonGroup() && transform.parent != null)
				_lastSelectedDictionary[transform.parent] = this;
			LerpTo(_selectedSprite, _selectedColor, _selectedScale);
		}

		/// <summary>
		/// Set visual state of button to 'Disabled'.
		/// Cannot guarantee effect if player is actively interacting with element.
		/// </summary>
		public void DisabledState()
		{
			_state = ILButtonState.Disable;
			LerpTo(_disabledSprite, _disabledColor, _disabledScale);
		}

		private void OnPressDown(object sender, PointerEventArgs e)
		{
			if (!enabled || _state == ILButtonState.Selected)
				return;

			_mouseDown = true;
			_isDragging = _hasDragable;

			PressState();
		}

		private void OnPressUp(object sender, PointerEventArgs e)
		{
			if (!enabled || !_mouseDown)
				return;
			_mouseDown = false;
			_isDragging = false;

			if (_mouseOver)
				HoverState();
			else
				NormalState();
		}

		private void OnPointerClick(object sender, PointerEventArgs pointerEventArgs)
		{
			if (_useSelectionState)
			{
				if (_toggle && _state == ILButtonState.Selected)
				{
					NormalState();
				}
				else
				{
					SelectedState();
				}
			}
		}

		private void OnEnter(object sender, PointerEventArgs e)
		{
			if (!enabled || _state == ILButtonState.Selected || _isDragging)
			{
				return;
			}

			_mouseOver = true;
			HoverState();
		}

		private void OnExit(object sender, PointerEventArgs e)
		{
			if (!enabled || _state == ILButtonState.Selected || _isDragging)
			{
				return;
			}
			_mouseOver = false;
			NormalState();
		}
	}
}
