using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.GPURender {
	[CustomEditor(typeof(GPURender))]
	class GPURenderEditor : EditorWindow {

		private float repaintSpeed;
		private Dictionary<string, (bool, VisualElement)> collectionToggle = new();
		private Dictionary<(string, RenderBlock), bool> queueDataMatrixToggle = new();
		private Dictionary<(string, RenderBlock), bool> queueDataToggle = new();
		private Dictionary<int, bool> queueInstanceToggle = new();

		private ListView collectionList;

		[MenuItem("Window/GPURender")]
		public static void ShowWindow() {
			GetWindow(typeof(GPURenderEditor));
		}

		public void CreateGUI() {
			var root = rootVisualElement;
			root.style.paddingTop = 16;
			root.style.paddingLeft = 16;
			root.style.paddingRight = 16;
			root.style.paddingBottom = 16;

			NewHeaderLabel(root, "mode");
			NewHeaderLabel(root, "renderTime");
			NewHeaderLabel(root, "updateTime");
			NewHeaderLabel(root, "totalObjects");
			CreateEmpty(root, 20);
			NewHeaderLabel(root, "", "Collections:");

			
			collectionList = new ListView {
				makeItem = () => GetCollectionList(),
				bindItem = (e, i) => {
					string coll = GPURender.Collections.Where(c => c.Value.Count > 0).Select(t => t.Key).OrderBy(t => t).ToList()[i];
					var blocks = GPURender.Collections[coll].SelectMany(b => GPURender.RenderBlockLookup[b]).Distinct().ToList();
					int amount = blocks.Sum(b => b.Amount);
					
					e.Q<Foldout>("foldout").text = $"{coll} [{amount}]";
					e.Q<ListView>("blocks").bindItem = (e, i) => {
						RenderBlock block = blocks[i];
						e.Q<Label>("block_name").text = $"{block.batch.mesh.name} [{block.Amount}]";
					};
					e.Q<ListView>("blocks").itemsSource = blocks;
					e.Q<ListView>("blocks").RefreshItems();
				},
				style = {
					height = 800,
					
				}
			};

			root.Add(collectionList);
			root.schedule.Execute(() => {
				VisualElement root = base.rootVisualElement;
				root.Q<Label>("mode").text = $"Mode: {GPURender.playMode}";
				root.Q<Label>("renderTime").text = $"Render Time: {GPURender.RenderTime:F2}ms";
				root.Q<Label>("updateTime").text = $"Update Time: {GPURender.UpdateTime:F2}ms";
				root.Q<Label>("totalObjects").text = $"Total Objects: {GPURender.RenderBlocks.Sum(b => b.Amount)}";
				var collections = GPURender.Collections.Where(c => c.Value.Count > 0).Select(t => t.Key).OrderBy(t => t).ToList();
				collectionList.itemsSource = collections;

				collectionList.RefreshItems();
			}).Every(100);
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
		VisualElement CreateEmpty(VisualElement root, float height) {
			var empty = new VisualElement() {
				style = {
					height = height
				}
			};
			root.Add(empty);

			return empty;
		}
		VisualElement GetCollectionList() {
			var container = new VisualElement() {
				style = {
					marginBottom = 6,
					marginTop = 12,
					height = 200
				}
			};
			var box = new Box() {
				style = {
					
				}
			};
			Foldout foldout = new Foldout() {
				name = "foldout",
				style = {
					
				}
			};
			var batchList = new ListView() {
				name = "blocks",
				makeItem = () => GetBlockList(),
				style = {
					
				}
			};

			container.Add(box);
			box.Add(foldout);
			foldout.Add(batchList);
			return container;
		}
		VisualElement GetBlockList() {
			var root = new VisualElement();
			NewHeaderLabel(root, "block_name");

			return root;
		}
		/*void OnGUI() {
			EditorGUI.indentLevel++;
			//EditorGUILayout.LabelField($"Blocks: {GPURender.RenderBlocks.Count} - Lookups: {GPURender.RenderBlockLookup.Count}/{GPURender.RenderBlockLookup.Sum(b => b.Value.Length)} - {GPURender.Collections.Count}/{GPURender.Collections.Sum(c => c.Value.Count)}");
			var titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 35 };
			EditorGUILayout.LabelField("GPU-Renderer", titleStyle, GUILayout.ExpandWidth(true));
			EditorGUILayout.LabelField($"Mode: {GPURender.playMode}");
			EditorGUILayout.LabelField($"Render Time: {GPURender.RenderTime:F2}ms");
			EditorGUILayout.LabelField($"Update Time: {GPURender.UpdateTime:F2}ms");
			EditorGUILayout.LabelField($"Total Objects: {GPURender.RenderBlocks.Sum(b => b.Amount)}");
			EditorGUILayout.Space(10);

			foreach (var pair in GPURender.Collections) {
				if (collectionToggle.ContainsKey(pair.Key) == false) {
					collectionToggle.Add(pair.Key, false);
				}

				List<RenderBlock> blocks = pair.Value.SelectMany(b => GPURender.RenderBlockLookup[b]).Distinct().ToList();
				int amount = blocks.Sum(b => b.Amount);
				collectionToggle[pair.Key] = EditorGUILayout.Foldout(collectionToggle[pair.Key], $"Collection [{amount}]: {pair.Key}");
			}

			/*foreach(var pair in GPURender.Collections) {
				if(collectionToggle.ContainsKey(pair.Key) == false) {
					collectionToggle.Add(pair.Key, false);
				}
				collectionToggle[pair.Key] = EditorGUILayout.Foldout(collectionToggle[pair.Key], $"Collection: {pair.Key}");

				if (collectionToggle[pair.Key]) {
					EditorGUI.indentLevel++;
					List<RenderBlock> blocks = pair.Value.SelectMany(b => GPURender.RenderBlockLookup[b]).Distinct().ToList();

					for (int i = 0; i < blocks.Count; i++) {
						RenderBlock info = blocks[i];
						if (queueDataToggle.ContainsKey((pair.Key, info)) == false) {
							queueDataToggle.Add((pair.Key, info), false);
						}

						queueDataToggle[(pair.Key, info)] = EditorGUILayout.Foldout(queueDataToggle[(pair.Key, info)], $"{info.batch.mesh.name} [{info.batch.subMeshIndex} | {info.Amount}]");
						if (queueDataToggle[(pair.Key, info)]) {
							EditorGUI.indentLevel++;

							EditorGUILayout.LabelField($"ID: {info.BatchID}");
							EditorGUILayout.LabelField($"Layer: {LayerMask.LayerToName(info.batch.layer)}|{info.batch.layer}");
							EditorGUILayout.LabelField($"Material: {info.batch.material.name}");

							if (queueDataMatrixToggle.ContainsKey((pair.Key, info)) == false) {
								queueDataMatrixToggle.Add((pair.Key, GPURender.RenderBlocks[i]), false);
							}
							queueDataMatrixToggle[(pair.Key, info)] = EditorGUILayout.Foldout(queueDataMatrixToggle[(pair.Key, info)], $"Matrixes");
							if (queueDataMatrixToggle[(pair.Key, info)]) {
								EditorGUI.indentLevel++;
								for (int m = 0; m < info.matrixes.Count; m++) {
									Matrix4x4 matrix = info.matrixes[m];
									int instanceId = info.idIndexes.FirstOrDefault(x => x.Value == m).Key;
									//EditorGUILayout.LabelField($"ID: {info.idIndexes.FirstOrDefault(x => x.Value == m)}");

									if (queueInstanceToggle.ContainsKey(instanceId) == false) {
										queueInstanceToggle.Add(instanceId, false);
									}
									queueInstanceToggle[instanceId] = EditorGUILayout.Foldout(queueInstanceToggle[instanceId], $"{instanceId}");
									if (queueInstanceToggle[instanceId]) {
										EditorGUI.indentLevel++;
										EditorGUILayout.LabelField($"Pos: {matrix.GetPosition()}");
										if (matrix.ValidTRS()) {
											EditorGUILayout.LabelField($"Rot: {matrix.rotation.eulerAngles}");
											EditorGUILayout.LabelField($"Scale: {matrix.lossyScale}");
										} else {
											EditorGUILayout.LabelField($"Matrix4x4 invalid");
										}
										EditorGUI.indentLevel--;
									}
								}
								EditorGUI.indentLevel--;
							}

							EditorGUI.indentLevel--;
						}
					}

					EditorGUI.indentLevel--;
				}
			}
		}

			EditorGUI.indentLevel--;
		}*/
	}

	[CustomPropertyDrawer(typeof(RenderBlock), true)]
	public class RenderQueueDrawer2 : PropertyDrawer {

		public Dictionary<string, bool> elementToggles = new();
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginDisabledGroup(true);

			EditorGUILayout.BeginVertical();
			//EditorGUILayout.LabelField($" [{property.FindPropertyRelative("meshIndex").intValue}] : {property.FindPropertyRelative("material").objectReferenceValue.name}");
			EditorGUILayout.LabelField("Amount: " + property.FindPropertyRelative("amount").intValue);
			//EditorGUILayout.LabelField("Layer:" + property.FindPropertyRelative("layer").intValue);
			//EditorGUILayout.LabelField("Render Time: " + property.FindPropertyRelative("renderTime").floatValue + "ms");
			EditorGUILayout.EndVertical();

			EditorGUI.EndDisabledGroup();
			EditorGUI.EndProperty();
		}
	}
}