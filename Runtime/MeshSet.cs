using UnityEngine;

namespace Sperlich.GPURender {
	[System.Serializable]
	public struct MeshSet {

		public Mesh mesh;
		public Material[] materials;
		public int layer;

		public int SubMeshCount => mesh == null ? 0 : mesh.subMeshCount;

		public bool IsValid {
			get {
				if(mesh == null) {
					return false;
				}
				if(materials == null) {
					return false;
				}
				if(materials.Length != mesh.subMeshCount) {
					return false;
				}

				return true;
			}
		}

		public MeshSet(Mesh mesh, Material[] materials, int layer) {
			this.mesh = mesh;
			this.materials = materials;
			this.layer = layer;
		}
		public MeshSet(MeshSet meshSet) {
			this.mesh = meshSet.mesh;
			this.materials = meshSet.materials;
			this.layer = meshSet.layer;
		}

		public void ThrowErrorIfInvalid() {
			if (mesh == null) {
				throw new System.ArgumentNullException(nameof(mesh));
			}
			if (materials == null) {
				throw new System.ArgumentNullException(nameof(materials));
			}
			if (materials.Length != mesh.subMeshCount) {
				throw new System.ArgumentOutOfRangeException($"Materials must be of same length as mesh subindex count {materials.Length}/{mesh.subMeshCount}");
			}
		}
		public SubMeshRenderData[] GetRenderData(Matrix4x4 matrix) {
			SubMeshRenderData[] data = new SubMeshRenderData[SubMeshCount];

			for (int i = 0; i < SubMeshCount; i++) {
				data[i] = new SubMeshRenderData(matrix, mesh, materials[i], i, layer);
			}

			return data;
		}
	}
}
