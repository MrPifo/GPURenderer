using UnityEngine;

namespace Sperlich.GPURender {
	public class GPUMesh : GPUTransform, IRender, IMeshRenderInfo {

		public bool IsStatic { get; set; }
		public bool IsRendering { get; set; }
		public Collection Collection { get; set; }
		public int InstanceID { get; set; } = GPURender.GenerateUniqueID();
		public int BatchID { get; set; }

		public MeshSet MeshSet { get; set; }
		public SubMeshRenderData[] RenderData { get; set; } = new SubMeshRenderData[0];

		public GPUMesh(string name = "") : base(name) {

		}
		public GPUMesh(GameObject obj, Collection collection = default) : base(obj.transform, obj == null ? "" : obj.name) {
			if(obj == null) {
				throw new System.ArgumentNullException(nameof(obj));
			}

			Collection = collection;
			if (obj.transform.TryGetRenderers(out MeshRenderer render, out MeshFilter filter)) {
				MeshSet = new(filter.sharedMesh, render.sharedMaterials, obj.layer);
				MeshSet.ThrowErrorIfInvalid();

				this.Enable();
			}
		}
		public GPUMesh(Component comp, Collection collection = default) : this(comp.gameObject, collection) { }
		public GPUMesh(MeshRenderer render, MeshFilter filter, int layer, Collection collection = default) : base(filter == null ? "" : filter.name) {
			if (render == null) {
				throw new System.ArgumentNullException(nameof(render));
			}
			if (filter == null) {
				throw new System.ArgumentNullException(nameof(filter));
			}

			Collection = collection;
			MeshSet = new(filter.sharedMesh, render.sharedMaterials, layer);
			MeshSet.ThrowErrorIfInvalid();

			this.Enable();
		}
		public GPUMesh(Mesh mesh, Material[] materials, int layer, Collection collection = default, string name = "") : base(name) {
			Collection = collection;
			MeshSet = new(mesh, materials, layer);
			MeshSet.ThrowErrorIfInvalid();

			this.Enable();
		}
		public GPUMesh(MeshSet set, Collection collection = default, string name = "") : base(name) {
			Collection = collection;
			MeshSet = set;
			MeshSet.ThrowErrorIfInvalid();

			this.Enable();
		}
	}
}
