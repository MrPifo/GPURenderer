using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.GPURender {
	public interface ITransform {

		public bool IsDirty { get; set; }
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector3 Scale { get; set; }
		public IReadOnlyList<ITransform> Children { get; }
		public ITransform Parent { get; set; }
		public Matrix4x4 Matrix { get; set; }

		public void UpdateTransform();
		public void SetDirty(bool includeChildren = false);
		public void SetParent(ITransform parent, bool keepTransform);
	}
}
