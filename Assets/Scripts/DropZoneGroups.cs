using IL.Input;
using UnityEngine;

/// <summary>
/// Make dropzones of draggables A and B exclusive (No A on A or B on B. Only A to B or B to A)
/// </summary>
public class DropZoneGroups : MonoBehaviour {

	[SerializeField]
	private Clickable[] _GroupA;
	[SerializeField]
	private Clickable[] _GroupB;

	private DropZone[] _groupADropZones;
	private DropZone[] _groupBDropZones;

	
	// Use this for initialization
	void Start ()
	{
		_groupADropZones = new DropZone[_GroupA.Length];
		for (int index = 0; index < _GroupA.Length; index++)
		{
			var item = _GroupA[index];
			if (item == null) continue;

			_groupADropZones[index] = item.GetComponent<DropZone>();
			item.DragStart += OnGroupADragStart;
			item.DragEnd += OnGroupADragEnd;
		}

		_groupBDropZones = new DropZone[_GroupB.Length];
		for (int index = 0; index < _GroupB.Length; index++)
		{
			var item = _GroupB[index];
			if (item == null) continue;

			_groupBDropZones[index] = item.GetComponent<DropZone>();
			item.DragStart += OnGroupBDragStart;
			item.DragEnd += OnGroupBDragEnd;
		}
	}



	private void OnGroupADragStart(object sender, PointerEventArgs pointerEventArgs)
	{
		foreach (var dropZonesA in _groupADropZones)
		{
			if (dropZonesA == null) continue;
			dropZonesA.enabled = false;
		}
	}

	private void OnGroupADragEnd(object sender, PointerEventArgs pointerEventArgs)
	{
		foreach (var dropZonesA in _groupADropZones)
		{
			if (dropZonesA == null) continue;
			dropZonesA.enabled = true;
		}
	}


	private void OnGroupBDragStart(object sender, PointerEventArgs pointerEventArgs)
	{
		foreach (var dropZonesB in _groupBDropZones)
		{
			if (dropZonesB == null) continue;
			dropZonesB.enabled = false;
		}
	}

	private void OnGroupBDragEnd(object sender, PointerEventArgs pointerEventArgs)
	{
		foreach (var dropZonesB in _groupBDropZones)
		{
			if (dropZonesB == null) continue;
			dropZonesB.enabled = true;
		}
	}
}
