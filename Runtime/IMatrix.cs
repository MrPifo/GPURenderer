using UnityEngine;

namespace Sperlich.GPURender {
	public interface IMatrix {

		public bool IsDirty { get; set; }
		public Matrix4x4 Matrix { get; set; }

		/*public virtual void SetPosition(Vector3 pos) {
			Matrix = Matrix4x4.TRS(pos, Matrix.rotation, Matrix.lossyScale);
			IsDirty = true;
		}
		public virtual void SetRotation(Quaternion rot) {
			Matrix = Matrix4x4.TRS(Matrix.GetColumn(3), rot, Matrix.lossyScale);
			IsDirty = true;
		}
		public virtual void SetPositionRotation(Vector3 pos, Quaternion rot) {
			Matrix = Matrix4x4.TRS(pos, rot, Matrix.lossyScale);
			IsDirty = true;
		}
		public virtual void SetTransform(Vector3 pos, Quaternion rot, Vector3 scale) {
			Matrix = Matrix4x4.TRS(pos, rot, scale);
			IsDirty = true;
		}*/
	}
}
