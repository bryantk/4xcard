using System;
using HutongGames.PlayMaker;
using IL.Input;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace IL.Actions.Input
{
	[ActionCategory("IL-Interaction")]
	[Obsolete("Deprecated - use 'EnableClickable'")]
	[@Tooltip("Disable a clickable object (User can not interact with it). Will grey out the clickable if it is an ILButton.")]
	public class DisableClickable : FsmStateAction
	{
		[RequiredField]
		public FsmGameObject Gameobject;
		[@Tooltip("If true, this will disable child game objects with the clickable component")]
		[RequiredField]
		public FsmBool DisableChildClickables;

		public override void Reset()
		{
			Gameobject = null;
			DisableChildClickables = false;
		}

		public override void OnEnter()
		{
			Clickable[] clickables = DisableChildClickables.Value
				? Gameobject.Value.GetComponentsInChildren<Clickable>()
				: Gameobject.Value.GetComponents<Clickable>();

			foreach (var clickable in clickables)
			{
				clickable.Enabled = false;
			}

			Finish();
		}
	}

	[ActionCategory("IL-Interaction")]
	[@Tooltip("Enable a clickable object. Will set to normal color if it is an ILButton.")]
	public class EnableClickable : FsmStateAction
	{
		[RequiredField]
		public FsmGameObject Gameobject;
		[@Tooltip("If true, this will enable child game objects with the clickable component")]
		[RequiredField]
		public FsmBool EnableChildClickables;
		[@Tooltip("Set the clickable to enabled to true or false.")]
		public FsmBool SetEnabled = true;
		[@Tooltip("Rarely Used - this will prevent changes to the IL-Button component and other components that might be affected by this action.")]
		public FsmBool OnlyClickableComponent = false;

		public override void Reset()
		{
			Gameobject = null;
			EnableChildClickables = false;
			OnlyClickableComponent = false;
		}

		public override void OnEnter()
		{
			Clickable[] clickables = EnableChildClickables.Value
				? Gameobject.Value.GetComponentsInChildren<Clickable>()
				: Gameobject.Value.GetComponents<Clickable>();

			foreach (var clickable in clickables)
			{
				if (OnlyClickableComponent.Value)
				{
					clickable.enabled = SetEnabled.Value;
				}
				else
				{
					clickable.Enabled = SetEnabled.Value;
				}
			}
			Finish();
		}

	    public override string AutoName()
	    {
	        try
	        {
	            string returnString = "";
	            returnString += SetEnabled.Value ? "Enable " : "Disable ";
	            string gameObjectName = Gameobject.UsesVariable && !Gameobject.IsNone
	                ? Gameobject.Name //If using an FSM variable that isn't None use that vairable's name
	                : Gameobject.Value == null //Otherwise if the target's game object is not null use that name
	                    ? "" // If the game object value is null leave name blank
	                    : Gameobject.Value.name;
	            returnString += gameObjectName + (EnableChildClickables.Value ? " Clickables" : " Clickable");
	            return returnString;
            }
	        catch
	        {
	            return base.AutoName();
	        }
	    }
	}

    [ActionCategory("IL-Interaction")]
    [@Tooltip("Enable or disable a clickable object array (User can still click the object if clicking is enabled, but not drag it")]
    public class EnableClickableArray : FsmStateAction
    {
        [RequiredField]
        [@Tooltip("The clickable game objectarray")]
        [ArrayEditor(VariableType.GameObject)]
        [UIHint(UIHint.Variable)]
        public FsmArray GameObjects;

        [@Tooltip("Check will enable, unchecked will disable")]
        public FsmBool SetEnabled = true;

        [@Tooltip("Rarely Used - this will prevent changes to the IL-Button component and other components that might be affected by this action.")]
        public FsmBool OnlyClickableComponent = false;

        public override void Reset()
        {
            GameObjects = null;
            Enabled = true;
        }

        public override void OnEnter()
        {
            foreach (System.Object obj in GameObjects.Values)
            {
                UnityEngine.GameObject gameObject = obj as UnityEngine.GameObject;
                Clickable[] clickables = gameObject.GetComponents<Clickable>();
                foreach (var clickable in clickables)
                {
                    if (OnlyClickableComponent.Value)
                    {
                        clickable.enabled = SetEnabled.Value;
                    }
                    else
                    {
                        clickable.Enabled = SetEnabled.Value;
                    }
                }
            }

            Finish();
        }
    }

}