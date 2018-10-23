using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.IL.Scripts.Common;
using Assets.IL.Scripts.Effects;
using HutongGames.PlayMaker;
using IL.Input;
using UnityEngine;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Assets.IL.Scripts.FSMActions.Effects
{
	[ActionCategory("IL-Effects")]
	[Tooltip("Highlight given game object. Can be used to blink things.")]
	public class Highlight : FsmStateAction
	{
		public enum HighlightBehavior
		{
			Blink,
			Solid,
			Pulsate
		}

		[RequiredField]
		[Tooltip("Object where the renderer will change color.")]
		public FsmGameObject ObjectToHighlight;

		[RequiredField]
		[Tooltip("What kind of highlight do you want?")]
		[ObjectType(typeof(HighlightBehavior))]
		public FsmEnum HighlightType;

		[Tooltip("What color should we use to highlight this object?")]
		public FsmColor HighlightColor = Color.cyan;

		[Tooltip("If true, then highlighting won't happen without an enabled clickable object.")]
		public FsmBool RequireClickable;

		[Tooltip("Provide a parent game object and recursively highlight the children game objects.")]
		public FsmBool Recursive;

		[Tooltip("Automatically Stop Highlighting after X (ignored if ZERO or less)")]
		public FsmFloat Duration = -1;

		public override void OnEnter()
		{
			if (ObjectToHighlight.Value == null)
			{
				Debug.LogWarning("Could not highlight gameobject due to null: " + HighlightType.Value);
				Finish();
				return;
			}

			var highlights = new List<GameObject>();
            GatherObjectsToHighlight(ref highlights);

			foreach (var highlight in highlights)
			{
				if (!highlight.activeInHierarchy) continue;
				HighlightObject((HighlightBehavior)HighlightType.Value, highlight, RequireClickable.Value, HighlightColor.Value);
			}
			if (Duration.Value > 0)
			{
				StartCoroutine(AutoStop());
			}
			Finish();
		}

        protected virtual void GatherObjectsToHighlight( ref List<GameObject> list )
        {
			if (Recursive.Value)
			{
				list.AddRange(ObjectToHighlight.Value.transform.GetChildren().Select(x => x.gameObject));
				if ((HighlightBehavior) HighlightType.Value == HighlightBehavior.Pulsate)
				{
					// If we found children, don't add self (double pulse looks terrible)
					if (list.Count >= 1)
					{
						return;
					}
				}
	        }
			list.Add(ObjectToHighlight.Value);
		}

		IEnumerator AutoStop()
		{
			yield return new WaitForSeconds(Duration.Value);

            var highlights = new List<GameObject>();
            GatherObjectsToHighlight(ref highlights);

            foreach( GameObject go in highlights)
            {
                Unhighlight.UnhighlightObject(go, false);
            }
        }

		public static void HighlightObject(HighlightBehavior highlightBehavior, GameObject toHighlight, bool requireClickable, Color highlightColor)
		{
			switch (highlightBehavior)
			{
				case HighlightBehavior.Solid:
					var swap = AddSwapColor(toHighlight, requireClickable, highlightColor);
					swap.Blink = false;
					break;
				case HighlightBehavior.Blink:
					swap = AddSwapColor(toHighlight, requireClickable, highlightColor);
					swap.Blink = true;
					break;
				case HighlightBehavior.Pulsate:
					var pulse = toHighlight.GetOrAddComponent<PulseScale>();
					pulse.RequireClickableEnabled = requireClickable;
					pulse.enabled = true;
					break;
			}
		}

		private static SwapColor AddSwapColor(GameObject toHighlight, bool requireClickable, Color highlightColor)
		{
			var component = toHighlight.GetOrAddComponent<SwapColor>();
			component.RequireClickable = requireClickable;
			component.HighlightColor = highlightColor;
			component.Color = highlightColor;
			component.enabled = true;
			return component;
		}

		public override void Reset()
		{
			ObjectToHighlight = null;
			HighlightColor.Value = Color.cyan;
			RequireClickable = false;
			Recursive = false;
		}
	}

    [ActionCategory("IL-Effects")]
	[Tooltip("Highlight given game object if it meets the specified conditional. Can be used to blink things.")]
	public class ConditionalHighlight : Highlight
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The condition to evaluate whether or not to hightlight here.")]
		public FsmBool Condition;

		public override void OnEnter ()
		{
			if( Condition.Value )
			{
				base.OnEnter ();
			}
			else
			{
				Finish();
			}
		}
	}

	[ActionCategory("IL-Effects")]
	[Tooltip("Highlight all game objects in the FSM Array. Can be used to blink things.")]
	public class HighlightArray : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Array of game objects to highlight.")]
		public FsmArray ArrayToHighlight;

		[RequiredField]
		[Tooltip("What kind of highlight do you want?")]
		[ObjectType(typeof(Highlight.HighlightBehavior))]
		public FsmEnum HighlightType;

		[Tooltip("What color should we use to highlight this object?")]
		public FsmColor HighlightColor = Color.cyan;

		[Tooltip("If true, then highlighting won't happen without an enabled clickable object.")]
		public FsmBool RequireClickable;

		[Tooltip("If true then don't highlight buttons that are in the selected state.")]
		public FsmBool ExcludeSelected;

        [Tooltip("Automatically Stop Highlighting after X (ignored if ZERO or less)")]
        public FsmFloat Duration = -1;

		public override void OnEnter()
		{
			if (ArrayToHighlight.Values == null)
			{
				Debug.LogWarning("Could not highlight gameobject due to null: " + HighlightType.Value);
				Finish();
				return;
			}

			for (var i = 0; i < ArrayToHighlight.Values.Length; i++)
			{
				var arrayElement = ArrayToHighlight.Values[i] as GameObject;
				if (arrayElement == null || !arrayElement.activeInHierarchy)
					continue;
				var button = arrayElement.GetComponent<ILButton>();
				if (ExcludeSelected.Value && button != null && button.ButtonState == ILButtonState.Selected)
					continue;
				HighlightElement(arrayElement);
			}
            if(Duration.Value > 0)
            {
                StartCoroutine(AutoStop());
            }

			Finish();
		}

		private void HighlightElement(GameObject arrayElement)
		{
			switch ((Highlight.HighlightBehavior)HighlightType.Value)
			{
				case Highlight.HighlightBehavior.Solid:
					AddSwapColor(arrayElement, false);
					break;
				case Highlight.HighlightBehavior.Blink:
					AddSwapColor(arrayElement, true);
					break;
				case Highlight.HighlightBehavior.Pulsate:
					AddPulse(arrayElement);
					break;
			}
		}

		private void AddSwapColor(GameObject objectToHighlight, bool blink)
		{
			var component = objectToHighlight.GetOrAddComponent<SwapColor>();
			component.RequireClickable = RequireClickable.Value;
			component.HighlightColor = HighlightColor.Value;
			component.Color = HighlightColor.Value;
			component.enabled = true;
			component.Blink = blink;
		}

		private void AddPulse(GameObject objectToPulse)
		{
			var component = objectToPulse.GetOrAddComponent<PulseScale>();
			component.RequireClickableEnabled = RequireClickable.Value;
			component.enabled = true;
		}

		public override void Reset()
		{
			ArrayToHighlight = null;
			HighlightColor.Value = Color.cyan;
			RequireClickable = false;
		}

        IEnumerator AutoStop()
        {
            yield return new WaitForSeconds(Duration.Value);
            for (var i = 0; i < ArrayToHighlight.Values.Length; i++)
            {
                var arrayElement = ArrayToHighlight.Values[i] as GameObject;
                if (arrayElement == null || !arrayElement.activeInHierarchy)
                    continue;
                Unhighlight.UnhighlightObject(arrayElement, RequireClickable.Value);
            }
        }
	}

	[ActionCategory("IL-Effects")]
	[Tooltip("Highlight all game objects in the FSM Array based on a conditional. Can be used to blink things.")]
	public class ConditionalHighlightArray : HighlightArray
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The condition on which we should highlight this array.")]
		public FsmBool Condition;

		public override void OnEnter ()
		{
			if( Condition.Value )
			{
				base.OnEnter ();
			}
			else
			{
				Finish();
			}
		}
	}

	[ActionCategory("IL-Effects")]
	[Tooltip("Revert highlighted object to normal.")]
	public class Unhighlight : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("Object where the renderer will change color.")]
		public FsmGameObject ObjectToUnHighlight;

		[Tooltip("Provide a parent game object and recursively unhighlight the children game objects.")]
		public FsmBool Recursive;

		[Tooltip("If true, then unhighlighting won't happen without an enabled clickable object.")]
		public FsmBool RequireClickable;

		public override void Reset()
		{
			ObjectToUnHighlight = null;
			Recursive = false;
			RequireClickable = false;
		}

		public override void OnEnter()
		{
			if (ObjectToUnHighlight.Value == null)
			{
				Debug.LogWarning("Could not unhighlight gameobject due to null");
				Finish();
				return;
			}

			var unhighlights = new List<GameObject>();
            GatherObjectsToUnhightlight(ref unhighlights);

			foreach (var unhighlight in unhighlights)
			{
				UnhighlightObject(unhighlight, RequireClickable.Value);
			}

			Finish();
		}

		public static void UnhighlightObject(GameObject target, bool requireClickable)
		{
			var swapColor = target.GetComponent<SwapColor>();
			if (swapColor != null)
			{
				swapColor.RequireClickable = requireClickable;
				swapColor.enabled = false;
			}

			var effects = new List<MonoBehaviour>
			{
				target.GetComponent<PulseScale>()
			};
			foreach (var monoBehaviour in effects)
			{
				if (monoBehaviour != null)
				{
					monoBehaviour.enabled = false;
				}
			}
			//var text = target.GetComponent<SmartText>();
			//if (text != null)
			//{
			//	text.UnHighlight();
			//}
		}

        protected virtual void GatherObjectsToUnhightlight( ref List<GameObject> list )
        {
            list.Add(ObjectToUnHighlight.Value);

            if (Recursive.Value)
            {
                list.AddRange(ObjectToUnHighlight.Value.transform.GetChildren().Select(x => x.gameObject));
            }
        }
	}

    [ActionCategory("IL-Effects")]
	[Tooltip("Revert highlighted object based on a conditional.")]
	public class ConditionalUnhighlight : Unhighlight
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Condition for when we should actually unhighlight this object.")]
		public FsmBool Condition;

		public override void OnEnter ()
		{
			if( Condition.Value )
			{
				base.OnEnter ();
			}
			else
			{
				Finish();
			}
		}
	}


	[ActionCategory("IL-Effects")]
	[Tooltip("Revert highlighted objects in the array to normal.")]
	public class UnhighlightArray : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Array of game objects to unhighlight.")]
		public FsmArray ArrayToUnhighlight;

		[Tooltip("If true, then unhighlighting won't happen without an enabled clickable object.")]
		public FsmBool RequireClickable;

		public override void OnEnter()
		{
			if (ArrayToUnhighlight.Values == null)
			{
				Debug.LogWarning("Could not unhighlight array due to null");
				Finish();
				return;
			}

			for (var i = 0; i < ArrayToUnhighlight.Values.Length; i++)
			{
				var arrayElement = ArrayToUnhighlight.Values[i] as GameObject;
				var swapColor = arrayElement.GetComponent<SwapColor>();
				if (swapColor != null)
				{
					swapColor.RequireClickable = RequireClickable.Value;
					swapColor.enabled = false;
				}
				var effects = new List<MonoBehaviour>
				{
					arrayElement.GetComponent<PulseScale>()
				};

				foreach (var monoBehaviour in effects)
				{
					if (monoBehaviour != null)
					{
						monoBehaviour.enabled = false;
					}
				}
			}

			Finish();
		}

		public override void Reset()
		{
			ArrayToUnhighlight = null;
			RequireClickable = false;
		}
	}

	[ActionCategory("IL-Effects")]
	[Tooltip("Revert highlighted objects in the array to normal based on a conditional.")]
	public class ConditionalUnhighlightArray : UnhighlightArray
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Condition for when we should actually unhighlight this object.")]
		public FsmBool Condition;

		public override void OnEnter ()
		{
			if( Condition.Value )
			{
				base.OnEnter ();
			}
			else
			{
				Finish();
			}
		}
	}
}
