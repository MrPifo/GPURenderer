using Sperlich.GPURender;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer((typeof(RenderBlock)))]
public class RenderBlockDrawer : PropertyDrawer {

	public override VisualElement CreatePropertyGUI(SerializedProperty property) {
		var root = new VisualElement();

		var label = new Label(property.displayName);
		
		return root;
	}

	Label NewHeaderLabel(VisualElement root, string name, string text = null) {
		var label = new Label() {
			style = {
				fontSize = 16,
			},
			name = name,
			text = text ?? ""
		};

		root.Add(label);
		return label;
	}
}
