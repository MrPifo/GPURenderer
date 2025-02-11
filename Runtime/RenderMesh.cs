using System;
using UnityEngine;

namespace Sperlich.GPURender {
	[ExecuteInEditMode]
	public class RenderMesh : MonoBehaviour, IRender, IMeshRenderInfo, IMatrix {

		[SerializeField]
		private Collection _collection;
		public bool isStatic;
		public bool includeChildren;

		private Nullable<int> _instanceId;
		public SubMeshRenderData[] RenderData { get; set; }

		bool IRender.IsStatic { get => isStatic; set => isStatic = value; }
		public bool IsRendering { get; set; }
		public bool IsDirty { get => transform.hasChanged; set => transform.hasChanged = value; }
		public bool IsPlaying => Application.isPlaying;
		public MeshSet MeshSet { get; set; } = default;
		public int InstanceID {
			get {
				if(_instanceId.HasValue == false) {
					_instanceId = GetInstanceID();
				}

				return _instanceId.Value;
			}
			set => _ = value;
		}
		public int BatchID { get; set; }
		public int Layer { get => gameObject.layer; set => gameObject.layer = value; }
		public Collection Collection { get => _collection; set => _collection = value; }
		public Matrix4x4 Matrix { get => transform.localToWorldMatrix; set => _ = value; }

		void Awake() {
			if(IsPlaying) {
				if(TryGetComponent(out MeshRenderer rend) && TryGetComponent(out MeshFilter filter)) {
					MeshSet = new MeshSet(filter.sharedMesh, rend.sharedMaterials, gameObject.layer);

					Destroy(rend);
					Destroy(filter);
				}

				if(includeChildren) {
					foreach(Transform t in transform.GetComponentsInChildren<Transform>()) {
						if(t.TryGetComponent(out MeshRenderer _) && t.TryGetComponent(out MeshFilter _)) {
							if(t.TryGetComponent(out RenderMesh _) == false) {
								t.gameObject.SetActive(false);
								var renderMesh = t.gameObject.AddComponent<RenderMesh>();
								renderMesh.isStatic = isStatic;
								renderMesh.Collection = _collection;
								t.gameObject.SetActive(true);
							}
						}
					}
				}
			}
		}
		void OnEnable() {
			if (MeshSet.IsValid) {
				this.Enable();
			}
		}
		void OnDisable() {
			this.Disable();
		}
		void OnDestroy() {
			this.Disable();
		}

		public void SetMeshData(Mesh mesh, Material[] materials, int layer = -1) {
			MeshSet = new MeshSet(mesh, materials, layer == -1 ? gameObject.layer : layer);

			if (MeshSet.IsValid) {
				this.SwapMeshInfo(MeshSet);
			}
		}
		public void SetMaterial(Material material, int index) {
			this.SwapMaterials(material, index);
		}
		public void SetMaterials(Material[] materials) {
			this.SwapMaterials(materials);
		}
		public void SetLayer(Enum layerEnum) {
			int layer = Convert.ToInt32(layerEnum);

			SetLayer(layer);
		}
		public void SetLayer(int layer) {
			MeshSet = new MeshSet(MeshSet.mesh, MeshSet.materials, layer);

			if (MeshSet.IsValid) {
				this.SwapMeshInfo(MeshSet);
			}
		}
		public void SetCollection(Collection collection) {
			if (MeshSet.IsValid && IsRendering) {
				this.SwapCollection(collection);
			}
		}

#if UNITY_EDITOR
		void OnValidate() {
			if (Application.isPlaying == false) {
				Collection = _collection;
				Mesh mesh = null;
				Material[] materials = null;

				if (TryGetComponent(out MeshRenderer render)) {
					materials = render.sharedMaterials;
				}
				if (TryGetComponent(out MeshFilter filter)) {
					mesh = filter.sharedMesh;
				}

				MeshSet = new MeshSet(mesh, materials, gameObject.layer);

				if (gameObject.activeInHierarchy && MeshSet.IsValid) {
					this.Enable();
				}
			}
		}
#endif
	}
}