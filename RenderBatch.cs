using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sperlich.GPURender {
	public readonly struct RenderBatch : IEquatable<RenderBatch> {

		public readonly int layer;
		public readonly int subMeshIndex;
		public readonly Material material;
		public readonly Mesh mesh;
		public readonly int batchID;

		public RenderBatch(Mesh mesh, Material material, int subMeshIndex, int layer) {
			this.mesh = mesh;
			this.material = material;
			this.subMeshIndex = subMeshIndex;
			this.layer = layer;
			batchID = GetBatchID(mesh, material, subMeshIndex, layer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetBatchID(Mesh mesh, Material material, int subMeshIndex, int layer) {
			return System.HashCode.Combine(mesh.GetHashCode(), material.GetHashCode(), subMeshIndex.GetHashCode(), layer.GetHashCode());
		}

		public override bool Equals(object obj) {
			return obj is RenderBatch batch && Equals(batch);
		}
		public bool Equals(RenderBatch other) {
			return batchID == other.batchID;
		}
		public override int GetHashCode() {
			return batchID.GetHashCode();
		}
		public static bool operator ==(RenderBatch left, RenderBatch right) {
			return left.Equals(right);
		}
		public static bool operator !=(RenderBatch left, RenderBatch right) {
			return !(left == right);
		}
	}
}
