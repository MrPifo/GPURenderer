using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Sperlich.UIToolkit;
using System;

namespace Sperlich.GPURender {
	[CustomEditor(typeof(GPURender))]
	public class GPURenderEditor : EditorWindow {

		private float repaintSpeed;
		private Dictionary<Collection, bool> collectionToggle = new();
		private Dictionary<(Collection, RenderBlock), bool> queueDataMatrixToggle = new();
		private Dictionary<(Collection, RenderBlock), bool> queueDataToggle = new();
		private Dictionary<int, bool> queueInstanceToggle = new();

		private List<string> collections = new();
		private ListView collectionListView;
		private Dictionary<string, VisualElement> collectionViewLists;

		[MenuItem("Window/GPURender")]
		public static void ShowWindow() {
			GetWindow(typeof(GPURenderEditor));
		}

		void OnInspectorUpdate() {
			Repaint();
		}

		void OnGUI() {
			var titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontSize = 26, fixedHeight = 30 };

			EditorGUI.indentLevel++;
			EditorGUILayout.Space(10);
			EditorGUILayout.LabelField("GPURender", titleStyle);
			EditorGUILayout.Space(10);
			
			EditorGUILayout.LabelField($"Mode: {GPURender.playMode}");
			EditorGUILayout.LabelField($"Render Time: {GPURender.RenderTime:F2}ms");
			EditorGUILayout.LabelField($"Update Time: {GPURender.UpdateTime:F2}ms");
			EditorGUILayout.LabelField($"Total Objects: {GPURender.RenderBlocks.Sum(b => b.Amount)}");
			EditorGUILayout.Space(10);

			foreach(var pair in GPURender.Collections) {
				if(collectionToggle.ContainsKey(pair.Key) == false) {
					collectionToggle.Add(pair.Key, false);
				}
				collectionToggle[pair.Key] = EditorGUILayout.Foldout(collectionToggle[pair.Key], $"Collection: {pair.Key}");

				if (collectionToggle[pair.Key]) {
					EditorGUI.indentLevel++;
					List<RenderBlock> blocks = pair.Value.SelectMany(b => GPURender.RenderBlockLookup[b]).Distinct().ToList();

                    foreach (RenderBlock info in pair.Value.SelectMany(b => GPURender.RenderBlockLookup[b]).Distinct()) {
						if (queueDataToggle.ContainsKey((pair.Key, info)) == false) {
							queueDataToggle.Add((pair.Key, info), false);
						}

						queueDataToggle[(pair.Key, info)] = EditorGUILayout.Foldout(queueDataToggle[(pair.Key, info)], $"{info.batch.mesh.name}");
						if (queueDataToggle[(pair.Key, info)]) {
							EditorGUI.indentLevel++;

							EditorGUILayout.LabelField($"ID: {info.BatchID}");
							EditorGUILayout.LabelField($"Index: {info.batch.subMeshIndex}");
							EditorGUILayout.LabelField($"Layer: {LayerMask.LayerToName(info.batch.layer)} | {info.batch.layer}");
							EditorGUILayout.LabelField($"Material: {info.batch.material.name}");
							EditorGUILayout.LabelField($"Amount: {info.Amount}");

							EditorGUI.indentLevel--;
						}
					}

					EditorGUI.indentLevel--;
				}
			}

			EditorGUI.indentLevel--;
		}
	}

	/*
	 			/*foreach (var pair in GPURender.Collections) {
				if (collectionToggle.ContainsKey(pair.Key) == false) {
					collectionToggle.Add(pair.Key, false);
				}

				List<RenderBlock> blocks = pair.Value.SelectMany(b => GPURender.RenderBlockLookup[b]).Distinct().ToList();
				int amount = blocks.Sum(b => b.Amount);
				collectionToggle[pair.Key] = EditorGUILayout.Foldout(collectionToggle[pair.Key], $"Collection [{amount}]: {pair.Key}");
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
	if (queueDataMatrixToggle.ContainsKey((pair.Key, info)) == false) {
								queueDataMatrixToggle.Add((pair.Key, info), false);
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

							*/
}