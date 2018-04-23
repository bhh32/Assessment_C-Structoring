using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (Randomizer))]
public class InspectorTool : Editor {

	public override void OnInspectorGUI (){
		
		DrawDefaultInspector ();

		Randomizer BuildProcess = (Randomizer)target;
		if (GUILayout.Button ("Build")) {
			BuildProcess.Clean ();
			BuildProcess.MuzzleBuild ();
			BuildProcess.HostBuild ();
			BuildProcess.FoundBuild ();
			BuildProcess.RoundBuild ();
		}
	}
}
