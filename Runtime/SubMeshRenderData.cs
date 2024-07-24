using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.GPURender {
    public class SubMeshRenderData {

		public Matrix4x4 matrix;
		public readonly RenderBatch batch;

		public int BatchID => batch.batchID;

		public SubMeshRenderData(Matrix4x4 matrix, Mesh mesh, Material material, int subMeshIndex, int layer) {
			this.matrix = matrix;
			this.batch = new RenderBatch(mesh, material, subMeshIndex, layer);
		}
	}
}