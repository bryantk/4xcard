
using System;

namespace Assets.IL.Scripts.Common
{
    [Flags]
    public enum CardType
    {
        None = 0,
        Basic = 1,
        Work = 2,
        Special = 4,

        All = 7
    }

    public enum InteractionStates
    {
        PointerClick,
        PointerDown,
        PointerUp,
        PointerEnter,
        PointerExit,
        DragStart,
        Drag,
        DragEnd,
        None
    }

    public enum ILButtonState
    {
        Normal,
        Hover,
        Press,
        Disable,
        Selected
    }

    public enum DragInteractionStates
    {
        DragIntoZone,
        DragOutOfZone,
        DropOnZone,
        DropOutOfZone
    }

    public enum MultiChoiceInteractionMode
    {
        ClickToReturn,
        Swap,
        DragToReturn
    }

    [Flags]
    public enum MultiChoiceInputInteractionMode
    {
        None = 0,
        Click = 1 << 0,
        Drag = 1 << 1,

        ClickAndDrag = Click | Drag
    }

    public enum AudioChannel
    {
        Instruction, //Only allow one audio at a time, set the Timeout, will stop the previous audio to play the next
        LoopingSound, //Allows many audios to play, will loop each audio
        MultiSound, //Allows many audios to play, will not set the timeout, each audio is unique
        SingleSound //Allows only one audio at a time, will not set the Timeout, will stop the previous audio to play the next
    }

    public enum Language
    {
        None = 0,
        Japanese = 1,
        Spanish = 2,
        Mandarin = 3,
        Korean = 4,
        English = 5,
        PortugueseBrazil = 6,
        French = 7,
        HaitianCreole = 8,
        Arabic = 9,
        Marshallese = 10,
        Vietnamese = 11,
        Russian = 12,
        Tagalog = 13,
        Cantonese = 14,
        Hmong = 15,
        Somali = 16
    }

    public enum CompressionType
    {
        // Audio
        WMA = 0, // Silverlight
        OGG = 1, // Unity (standalone, webplayer)
        MP3 = 2, // Unity (tablet)

        // Graphic
        ILJ = 3, // Silverlight
        PNG = 4, // Unity
        JPG = 5, // Unity

        // Animation
        XAP = 6, // Silverlight
        SIL = 11, // JSON animation

        // Video
        ISM = 7, // Silverlight
        OGV = 8, // Unity (standalone, webplayer)
        MP4 = 9, // Unity (tablet)

        // Printouts
        PDF = 10, // Silverlight

        // Activity
        TAR = 20, // Tablet

        // Asset Bundles
        IAB = 30,   //iOS Asset Bundle
        AAB = 31,   //Android Asset Bundle
        SAB = 32,   //Standalone asset bundle
        MAB = 33,   //Metro asset bundle
        NAB = 34,   //NaCl asset bundle
        WAB = 35,   //WebGL/HTML5 asset bundle
        WPAB = 37,   //WebPlayer asset bundle

        // Streaming Video
        MPD = 36
    }


    public enum DropZoneBehavior
    {
        RETURN_TO_START_POSITION,
        INSTANTLY_CENTER_ON_DROP_ZONE,
        TWEEN_TO_CENTER_OF_DROP_ZONE,
        STAY_WHERE_DROPPED
    }


	[Flags]
	public enum ArrowDirections
	{
		None = 0,
		Right = 1,
		Up = 2,
		Left = 4,
		Down = 8,
		All = 15
	}

	public static class FlagEnumExtensions
    {
        public static bool HasFlag<T>(this T value, T check)
        {
            var valueNum = Convert.ToInt32(value);
            var checkNum = Convert.ToInt32(check);

            return (valueNum & checkNum) == checkNum;
        }
    }
}
