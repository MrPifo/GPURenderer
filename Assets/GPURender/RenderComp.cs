using UnityEngine;

namespace Sperlich.GPURender {
	[ExecuteInEditMode]
	public class GPUMeshRenderer : MonoBehaviour, IRender, IMeshRenderInfo, IMatrix {

		public bool isStatic;
		public bool includeChildren;

		[SerializeField]
		private Mesh _mesh;
		[SerializeField]
		private Material[] _materials;
		[SerializeField]
		private string _collection;

		public SubMeshRenderData[] RenderData { get; set; }

		bool IRender.IsStatic { get => isStatic; set => isStatic = value; }
		public bool IsRendering { get; set; }
		public bool IsDirty { get => transform.hasChanged; set => transform.hasChanged = value; }
		public bool IsPlaying => Application.isPlaying;
		public MeshSet MeshSet { get => new MeshSet(_mesh, _materials, gameObject.layer); set {
				_mesh = value.mesh;
				_materials = value.materials;
				gameObject.layer = value.layer;
			}
		}
		public int InstanceID { get => this.GetInstanceID(); set => _ = value; }
		public int BatchID { get; set; }
		public int Layer { get; set; }
		public string Collection { get; set; }
		public Mesh Mesh { get => _mesh; set => _mesh = value; }
		public Material[] Materials { get => _materials; set => _materials = value; }
		public Matrix4x4 Matrix { get => transform.localToWorldMatrix; set => _ = value; }

		void Awake() {
			if(IsPlaying) {
				if(TryGetComponent(out MeshRenderer rend) && TryGetComponent(out MeshFilter filter)) {
					Mesh = filter.sharedMesh;
					Materials = rend.sharedMaterials;
					Layer = rend.gameObject.layer;
					Collection = _collection;

					Destroy(rend);
					Destroy(filter);
				}

				if(includeChildren) {
					foreach(Transform t in transform.GetComponentsInChildren<Transform>()) {
						if(t.TryGetComponent(out MeshRenderer _) && t.TryGetComponent(out MeshFilter _)) {
							if(t.TryGetComponent(out GPUMeshRenderer _) == false) {
								t.gameObject.AddComponent<GPUMeshRenderer>();
							}
						}
					}
				}
			}
		}
		void OnEnable() {
			this.Enable();
		}
		void OnDisable() {
			this.Disable();
		}

#if UNITY_EDITOR
		void OnValidate() {
			Collection = _collection;
			if (gameObject.activeInHierarchy) {
				GPURender.Subscribe(this);
			}

			if(TryGetComponent(out MeshRenderer render)) {
				_materials = render.sharedMaterials;
			}
			if(TryGetComponent(out MeshFilter filter)) {
				_mesh = filter.sharedMesh;
			}
		}
#endif
	}
}