using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sperlich.GPURender {
	public class GPURender {

		internal static string playMode;
		internal static List<RenderBlock> RenderBlocks = new();
		internal static Dictionary<IRender, RenderBlock[]> RenderBlockLookup = new();
		internal static Dictionary<string, List<IRender>> Collections = new();

		private static int LastGivenID { get; set; }
		private static GPURenderEditorInstance _instance;
		public static float RenderTime { get; private set; }
		public static float UpdateTime { get; private set; }
		public static GPURenderEditorInstance SceneInstance {
			get {
				if(_instance == null) {
					var obj = Object.FindAnyObjectByType<GPURenderEditorInstance>(FindObjectsInactive.Include);
					if (obj == null) {
#if UNITY_EDITOR
						_instance = CreateSceneViewInstance();
#endif
					} else {
						_instance = obj.GetComponent<GPURenderEditorInstance>();
					}
				}

				return _instance;
			}
		}
		private static System.Random Randomizer { get; set; } = new();
		private static System.Diagnostics.Stopwatch RenderWatch { get; set; } = new();
		public static string DefaultCollection = "Default";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void RuntimeInit() {
			playMode = "Runtime";
			Clear();
			RenderPipelineManager.beginContextRendering -= RenderContext;
			RenderPipelineManager.beginContextRendering += RenderContext;
		}
		
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		public static void EditorInit() {
			playMode = "Editor";
			RenderPipelineManager.beginContextRendering -= RenderContext;
			UnityEditor.EditorApplication.playModeStateChanged -= PlayModeChanged;
			UnityEditor.EditorApplication.playModeStateChanged += PlayModeChanged;
			CheckDestroyAndCreateInstance();
		}
		static void PlayModeChanged(UnityEditor.PlayModeStateChange mode) {
			if (mode == UnityEditor.PlayModeStateChange.ExitingPlayMode) {
				playMode = "Editor";
				Clear();
				CheckDestroyAndCreateInstance();
				RenderPipelineManager.beginContextRendering -= RenderContext;
			}
		}
#endif

		#region API
		public static void Render() {
			RenderContext(default, null);
		}
		public static void Clear() {
			foreach(var coll in Collections.Keys.ToList()) {
				ClearCollection(coll);
			}

			LastGivenID = 0;
			Collections.Clear();
			RenderBlocks.Clear();
			RenderBlockLookup.Clear();
		}
		public static void ClearCollection(string collection) {
			if(Collections.ContainsKey(collection)) {
				foreach(IRender render in Collections.Values.SelectMany(l => l).ToList()) {
					Unsubscribe(render);
				}

				if (Collections.ContainsKey(collection)) {
					Collections.Remove(collection);
				}
			}
		}
		public static void Subscribe(IRender instance) => Subscribe(instance, true);
		public static void Subscribe(IRender instance, bool buildRenderData) {
			if (RenderBlockLookup.ContainsKey(instance)) {
				return;
			}

			if (instance is IMeshRenderInfo meshData) {
				IMatrix matrix = instance.IMatrix;
				if (buildRenderData) {
					instance.RenderData = meshData.MeshSet.GetRenderData(matrix.Matrix);
				}
			}

			if(string.IsNullOrEmpty(instance.Collection)) {
				instance.Collection = DefaultCollection;
			}
			if (Collections.ContainsKey(instance.Collection) == false) {
				Collections.Add(instance.Collection, new());
			}

			Collections[instance.Collection].Add(instance);
			RenderBlockLookup.Add(instance, new RenderBlock[instance.RenderData.Length]);
			AddToQueue(instance);
		}
		public static void Unsubscribe(IRender instance) {
			if (RenderBlockLookup.ContainsKey(instance)) {
				string collection = string.IsNullOrEmpty(instance.Collection) ? DefaultCollection : instance.Collection;
				foreach(var block in RenderBlockLookup[instance]) {
					block.Remove(instance.InstanceID);

					if(block.Amount == 0) {
						RenderBlocks.Remove(block);
					}
				}

				RenderBlockLookup.Remove(instance);
				Collections[collection].Remove(instance);
				if (Collections[collection].Count == 0) {
					Collections.Remove(collection);
				}
				instance.IsRendering = false;
			}
		}
		public static void RefreshInstance(IRender render) {
			foreach(var block in RenderBlockLookup[render]) {
				block.Update(render.InstanceID, render.IMatrix.Matrix);
			}

			render.IMatrix.IsDirty = false;
		}
		public static int GenerateUniqueID() {
			LastGivenID++;
			if (LastGivenID == int.MaxValue) {
				LastGivenID = int.MinValue;
			}
			return LastGivenID;
		}
		#endregion

		static void RenderContext(ScriptableRenderContext context, List<Camera> cams) {
			RenderWatch.Restart();
			CheckDirty();
			UpdateTime = RenderTime = (float)RenderWatch.Elapsed.TotalMilliseconds;
			RenderWatch.Restart();

			foreach (RenderBlock block in RenderBlocks) {
				Graphics.DrawMeshInstanced(block.batch.mesh, block.batch.subMeshIndex, block.batch.material, block.matrixes, null, ShadowCastingMode.On, true, block.batch.layer);
			}

			RenderWatch.Stop();
			RenderTime = (float)RenderWatch.Elapsed.TotalMilliseconds;
		}
		static void CheckDirty() {
			foreach(var value in RenderBlockLookup.Keys) {
				if (value.IsStatic) continue;


				if(value.IMatrix.IsDirty) {
					RefreshInstance(value);
				}
			}
		}

		static void AddToQueue(IRender instance) {
			for(int i = 0; i < instance.RenderData.Length; i++) {
				SubMeshRenderData data = instance.RenderData[i];
				if (data.batch.mesh == null) {
					Debug.LogWarning($"Cannot instance without a Mesh.");
					continue;
				}
				if (data.batch.material == null) {
					Debug.LogWarning($"Cannot instance without a Material.");
					continue;
				}
				if (data.batch.material.enableInstancing == false) {
					Debug.LogWarning($"Instancing not enabled for Material {data.batch.material.name}.");
					continue;
				}

				bool createNewQueue = true;
				RenderBlock block = null;
				// Check if there already exists a RenderBlock
				foreach (RenderBlock b in RenderBlocks) {
					if (b.BatchID != data.BatchID) continue;
					if (b.Has(instance.InstanceID)) continue;

					block = b;
					createNewQueue = false;
					break;
				}
				if (createNewQueue) {
					block = new RenderBlock(data);
					RenderBlocks.Add(block);
				}

				block.Add(instance, data.matrix);
				RenderBlockLookup[instance][i] = block;
			}

			instance.IsRendering = true;
		}
		static bool TryGetRenderBlock(int id, out RenderBlock block) {
			foreach (RenderBlock b in RenderBlocks) {
				if(b.Has(id)) {
					block = b;
					return true;
				}
			}

			block = null;
			return false;
		}

#if UNITY_EDITOR
		public static GPURenderEditorInstance CreateSceneViewInstance() {
			HideFlags flags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.DontSaveInEditor;
			var instance = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("GPURender Editor Instance", flags).AddComponent<GPURenderEditorInstance>();
			return instance;
		}
		public static void CheckDestroyAndCreateInstance() {
			List<GameObject> objectsInScene = new();
			foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>()) {
				if (UnityEditor.EditorUtility.IsPersistent(go.transform.root.gameObject) == false && go.TryGetComponent(out GPURenderEditorInstance _)) {
					objectsInScene.Add(go);
				}
			}

			foreach (GameObject obj in objectsInScene) {
				Object.DestroyImmediate(obj);
			}

			CreateSceneViewInstance();
		}
#endif
	}
}