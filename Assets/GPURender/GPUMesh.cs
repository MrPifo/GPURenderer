using UnityEngine;

namespace Sperlich.GPURender {
	public class GPUMesh : GPUTransform, IRender, IMeshRenderInfo {

		public bool IsStatic { get; set; }
		public bool IsRendering { get; set; }
		public string Collection { get; set; }
		public int InstanceID { get; set; } = GPURender.GenerateUniqueID();
		public int BatchID { get; set; }

		public MeshSet MeshSet { get; set; }
		public SubMeshRenderData[] RenderData { get; set; } = new SubMeshRenderData[0];

		public GPUMesh(string name = "") : base(name) {

		}
		public GPUMesh(GameObject obj, string collection = null) : base(obj.transform, obj == null ? "" : obj.name) {
			if(obj == null) {
				throw new System.ArgumentNullException(nameof(obj));
			}

			Collection = collection ?? null;
			if (obj.transform.TryGetRenderers(out MeshRenderer render, out MeshFilter filter)) {
				MeshSet = new(filter.sharedMesh, render.sharedMaterials, obj.layer);
				MeshSet.ThrowErrorIfInvalid();

				this.Enable();
			}
		}
		public GPUMesh(Component comp, string collection = null) : this(comp.gameObject, collection) { }
		public GPUMesh(MeshRenderer render, MeshFilter filter, int layer, string collection = null) : base(filter == null ? "" : filter.name) {
			if (render == null) {
				throw new System.ArgumentNullException(nameof(render));
			}
			if (filter == null) {
				throw new System.ArgumentNullException(nameof(filter));
			}

			Collection = collection ?? null;
			MeshSet = new(filter.sharedMesh, render.sharedMaterials, layer);
			MeshSet.ThrowErrorIfInvalid();

			this.Enable();
		}
		public GPUMesh(Mesh mesh, Material[] materials, int layer, string collection = null, string name = "") : base(name) {
			Collection = collection ?? null;
			MeshSet = new(mesh, materials, layer);
			MeshSet.ThrowErrorIfInvalid();

			this.Enable();
		}
		public GPUMesh(MeshSet set, string collection = null, string name = "") : base(name) {
			Collection = collection ?? null;
			MeshSet = set;
			MeshSet.ThrowErrorIfInvalid();

			this.Enable();
		}
	}
}
