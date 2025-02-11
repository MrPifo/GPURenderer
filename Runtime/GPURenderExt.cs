using System.Collections.ObjectModel;
using UnityEngine;

namespace Sperlich.GPURender {
	public static class GPURenderExt {

		public static void Refresh(this IRender render) {
			GPURender.RefreshInstance(render);
		}
		public static void Enable(this IRender render) {
			GPURender.Subscribe(render);
		}
		public static void Disable(this IRender render) {
			GPURender.Unsubscribe(render);
		}
		public static GPUMesh CreateRenderer(this GameObject obj, bool autoEnable = true) => obj.CreateRenderer(default, autoEnable);
		public static GPUMesh CreateRenderer(this GameObject obj, Collection collection, bool autoEnable = true) {
			return obj.transform.CreateRenderer(collection, autoEnable);
		}
		public static GPUMesh CreateRenderer(this Component comp, bool autoEnable = true) => comp.CreateRenderer(default, autoEnable);
		public static GPUMesh CreateRenderer(this Component comp, Collection collection, bool autoEnable = true) {
			if (comp.TryGetRenderers(out MeshRenderer render, out MeshFilter filter) == false) {
				Debug.LogError($"ERROR: GameObject {comp.name} is missing either MeshRenderer or MeshFilter.");
				return null;
			}

			GPUMesh mesh = new GPUMesh(render, filter, comp.gameObject.layer, collection);
			if (autoEnable) {
				mesh.Enable();
			}
			return mesh;
		}
		public static GPUTransform ConvertHierarchy(this GameObject obj) => obj.transform.ConvertHierarchy();
		public static GPUTransform ConvertHierarchy(this Component comp) {
			// Check if this Transform has attached Renderers, otherwise create an empty GPUTransform
			GPUTransform root;
			if(comp.transform.TryGetRenderers(out _, out _)) {
				root = new GPUMesh(comp.transform);
			} else {
				root = new GPUTransform(comp.transform);
			}

			foreach(Transform ct in comp.transform) {
				GPUTransform child = ct.ConvertHierarchy();
				child.Parent = root;
				//Debug.Log(child);
			}

			return root;
		}
		public static bool TryGetRenderers(this GameObject obj, out MeshRenderer renderer, out MeshFilter filter) => obj.TryGetRenderers(out renderer, out filter);
		public static bool TryGetRenderers(this Component comp, out MeshRenderer renderer, out MeshFilter filter) {
			if (comp.TryGetComponent(out filter) && comp.TryGetComponent(out renderer)) {
				return true;
			}

			filter = null;
			renderer = null;
			return false;
		}

		public static void SwapMeshInfo(this IMeshRenderInfo info, Mesh mesh, Material[] materials, int layer) => info.SwapMeshInfo(new MeshSet(mesh, materials, layer));
		/// <summary>
		/// Swaps the currents Mesh with the new configuration. If this object was already rendering, it will continue rendering with the new configuration.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="materials"></param>
		/// <param name="layer"></param>
		public static void SwapMeshInfo(this IMeshRenderInfo info, MeshSet set) {
			IRender render = (IRender)info;
			bool wasRendering = render.IsRendering;
			if (render.IsRendering) {
				GPURender.Unsubscribe(render);
			}

			info.MeshSet = set;

			if (wasRendering) {
				GPURender.Subscribe(render);
			}
		}
		public static void SwapCollection(this IRender render, Collection newCollection) {
			render.Collection = newCollection;

			if (render.IsRendering) {
				GPURender.Unsubscribe(render);
			}

			GPURender.Subscribe(render);
		}
		public static void SwapMaterials(this IMeshRenderInfo info, Material[] materials) {
			IRender render = (IRender)info;
			bool wasRendering = render.IsRendering;
			if (render.IsRendering) {
				GPURender.Unsubscribe(render);
			}

			info.MeshSet = new MeshSet(info.MeshSet) {
				materials = materials
			};

			if (wasRendering) {
				GPURender.Subscribe(render);
			}
		}
		public static void SwapMaterials(this IMeshRenderInfo info, Material material, int index) {
			IRender render = (IRender)info;
			bool wasRendering = render.IsRendering;
			if (render.IsRendering) {
				GPURender.Unsubscribe(render);
			}

			Material[] mats = info.MeshSet.materials;
			mats[index] = material;
			info.MeshSet = new MeshSet(info.MeshSet) {
				materials = mats
			};

			if (wasRendering) {
				GPURender.Subscribe(render);
			}
		}
	}
}
