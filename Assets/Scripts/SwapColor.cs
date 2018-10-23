using IL.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwapColor : MonoBehaviour
{
	public Color HighlightColor;
	[SerializeField]
	private float _interval = 0.3f; //300 ms taken from old flash manager
	public bool Blink = false;

	[SerializeField, Range(0f, 1f)]
	private float _dutyCycle = 1f;
	[SerializeField]
	private int _period = 100;

	private Color _normalColor;
	private SpriteRenderer _spriteRenderer;
	private Image _image;
	private TextMeshPro _tmp;
	private TextMeshProUGUI _tmpGUI;
	private Timer _timer;
	private ILButton _button;
	private Renderer _renderer;

	private int _cyclesBeforeDowntime = 100;
	private int _currentCycle = 100;
	private Clickable _clickable;

	public Color Color
	{
		get {
			if (_button != null)
			{
				return _button.MyColor;
			}
			if (_spriteRenderer != null)
			{
				return _spriteRenderer.color;
			}
			if (_image != null)
			{
				return _image.color;
			}
			if (_tmp != null)
			{
				return _tmp.color;
			}
			if (_tmpGUI != null)
			{
				return _tmpGUI.color;
			}
            if (_renderer != null)
			{
				return _renderer.material.color;
			}
			return default(Color);
		}
		set
		{
			if (IsDisabledClickable)
			{
				//if the clickable isn't enabled we will not set the color
				return;
			}

			if (_button != null)
			{
				_button.MyColor = value;
			}
			else if (_spriteRenderer != null)
			{
				_spriteRenderer.color = value;
			}
			else if (_image != null)
			{
				_image.color = value;
			}
			else if (_tmp != null)
			{
				_tmp.color = value;
			}
			else if (_tmpGUI != null)
			{
				_tmpGUI.color = value;
			}
            else if (_renderer != null)
			{
				_renderer.material.color = value;
			}
		}
	}

	[Tooltip("If true, then highlighting won't happen without an enabled clickable object.")]
	public bool RequireClickable;

	private bool IsDisabledClickable
	{
		get { return RequireClickable && _clickable != null && !_clickable.Enabled; }
	}


	public virtual void Awake()
	{
		_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		_image = gameObject.GetComponent<Image>();
		_tmp = gameObject.GetComponent<TextMeshPro>();
		_tmpGUI = gameObject.GetComponent<TextMeshProUGUI>();
		_clickable = gameObject.GetComponent<Clickable>();
		_button = gameObject.GetComponent<ILButton>();
		_renderer = gameObject.GetComponent<Renderer>();
		_normalColor = Color;
		_timer = new Timer(_interval);
	}

	public void OnEnable()
	{
		_timer = new Timer(_interval);
		ResetDutyCycle();
	}

	private void ResetDutyCycle()
	{
		_cyclesBeforeDowntime = Mathf.Clamp((int)((1 - _dutyCycle) * _period), 0, _period); ;
		_currentCycle = _period;
	}

	public void Update()
	{
		//we don't want to highlight clickables that aren't enabled
		if (IsDisabledClickable)
			return;

		if (!Blink)
			return;
		if (_timer.Tick(Time.deltaTime))
		{
			_currentCycle--;
			if (_currentCycle >= _cyclesBeforeDowntime)
				Color = Color == _normalColor ? HighlightColor : _normalColor;
			else
				Color = _normalColor;
			_timer.Restart();
			if (_currentCycle <= 0)
				ResetDutyCycle();
		}
	}

	public void RevertColor()
	{
		if (IsDisabledClickable)
			return;

		Color = _normalColor;
	}

	public void OnDisable()
	{
		Blink = false;
		RevertColor();
	}
}