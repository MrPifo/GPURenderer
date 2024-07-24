using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.GPURender {
	public class RenderBlock {

		public readonly RenderBatch batch;
		public readonly List<Matrix4x4> matrixes = new();
		public readonly Dictionary<int, int> idIndexes = new();
		public int Amount => matrixes.Count;
		public int BatchID => batch.batchID;

		//public Dictionary<ulong, (IGPU instance, int index)> instances;
		//public (Material material, Mesh mesh, int subMeshIndex) Key;

		public RenderBlock(Mesh mesh, Material mat, int index, int layer) {
			this.batch = new RenderBatch(mesh, mat, index, layer);
		}
		public RenderBlock(SubMeshRenderData data) {
			this.batch = data.batch;
		}

		public void Add(IRender render, Matrix4x4 matrix) {
			idIndexes.Add(render.InstanceID, matrixes.Count);
			matrixes.Add(matrix);

			render.BatchID = batch.batchID;
		}
		public void Update(int instanceId, Matrix4x4 matrix) {
			matrixes[idIndexes[instanceId]] = matrix;
		}
		public void Remove(int instanceId) {
			int index = idIndexes[instanceId];
			idIndexes.Remove(instanceId);
			matrixes.RemoveAt(index);

			foreach (int key in new List<int>(idIndexes.Keys)) {
				if (idIndexes[key] > index) {
					idIndexes[key] = idIndexes[key] - 1;
				}
			}

			//Debug.Log("Removed: " + id);
		}
		public bool Has(int id) {
			return idIndexes.ContainsKey(id);
		}
	}
}