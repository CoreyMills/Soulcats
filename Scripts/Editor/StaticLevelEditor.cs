using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;

[CustomEditor(typeof(StaticLevel))]
public class StaticLevelEditor : Editor {

	public override void OnInspectorGUI(){	
		base.OnInspectorGUI ();
		StaticLevel level = (StaticLevel)target;

		if (GUILayout.Button ("\nTOGGLE RENDERERS\n")) {

			// flip
			level.renderersVisible = !level.renderersVisible;

			// toggle renderers
			Renderer[] renderers = level.gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in renderers) {
				renderer.enabled = level.renderersVisible;
			}

			// set dirty
			EditorUtility.SetDirty (level);
			EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}
	}
}
