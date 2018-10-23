using Assets.IL.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IL.Input
{
	/// <summary>
	/// Linked to an ILButton. When the button changes color, this will too.
	/// </summary>
	public class RelatedButton : MonoBehaviour
	{
		[SerializeField] private ILButton _targetButton;

	    [SerializeField] private bool _overrideNormalColor = false;
        [SerializeField] private bool _overrideHoverColor = false;
        [SerializeField] private bool _overridePressedColor = false;
        [SerializeField] private bool _overrideDisabledColor = false;
	    [SerializeField] private bool _overrideSelectedColor = false;

        [SerializeField] private Color _normalColorOverride = Color.white;
        [SerializeField] private Color _hoverColorOverride = Color.white;
        [SerializeField] private Color _pressedColorOverride = Color.white;
        [SerializeField] private Color _disabledColorOverride = Color.white;
        [SerializeField] private Color _selectedColorOverride = Color.white;


        private SpriteRenderer _sr;
		private Image _image;
		private TMP_Text _tmp;

		// Use this for initialization
		void Awake()
		{
			_sr = GetComponent<SpriteRenderer>();
			_image = GetComponent<Image>();
			_tmp = GetComponent<TMP_Text>();
		}

		void OnEnable()
		{
			_targetButton.OnColorChange += ColorChange;
		}

		void OnDisable()
		{
			_targetButton.OnColorChange -= ColorChange;
		}

		private void ColorChange(object sender, ColorArgs colorArgs)
		{
		    var newColor = colorArgs.color;
            if (sender.GetType() == (typeof(ILButton)))
            {
                var state = ((ILButton) sender).ButtonState;
                switch (state)
                {
                    case ILButtonState.Normal:
                        if (_overrideNormalColor)
                            newColor = _normalColorOverride;
                        break;
                    case ILButtonState.Hover:
                        if (_overrideHoverColor)
                            newColor = _hoverColorOverride;
                        break;
                    case ILButtonState.Press:
                        if (_overridePressedColor)
                            newColor = _pressedColorOverride;
                        break;
                    case ILButtonState.Disable:
                        if (_overrideDisabledColor)
                            newColor = _disabledColorOverride;
                        break;
                    case ILButtonState.Selected:
                        if (_overrideSelectedColor)
                            newColor = _selectedColorOverride;
                        break;
                    default:
                        Debug.LogWarning("RelatedButton does not support ILButton state ["+ state + "]");
                        break;
                }
            }

            if (_sr != null)
				_sr.color = newColor;
			else if (_image != null)
				_image.color = newColor;
			else if (_tmp != null)
				_tmp.color = newColor;
		}
	}
}