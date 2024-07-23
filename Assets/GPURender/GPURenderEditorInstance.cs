using Sperlich.GPURender;
using UnityEngine;

[ExecuteAlways]
public class GPURenderEditorInstance : MonoBehaviour {

	void Update() {
		GPURender.Render();
	}

}