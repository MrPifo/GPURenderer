using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.GPURender {
	public class GPUTransform : ITransform, IMatrix, IDescriptor {

		private Vector3 _localPos;
		public Vector3 Position {
			get => _localPos;
			set {
				_localPos = value;
				_localMatrix.SetColumn(3, new Vector4(value.x, value.y, value.z, 1));

				UpdateTransform();
			}
		}
		private Quaternion _localRot = Quaternion.identity;
		public Quaternion Rotation {
			get => _localRot;
			set {
				value.Normalize();
				_localRot = value;
				_localMatrix.SetTRS(_localPos, value, _localScale);

				UpdateTransform();
			}
		}
		private Vector3 _localScale = Vector3.one;
		public Vector3 Scale {
			get => _localScale;
			set {
				_localScale = value;
				_localMatrix.SetTRS(_localPos, _localRot, value);

				UpdateTransform();
			}
		}

		private ITransform _parent;
		public ITransform Parent {
			get => _parent;
			set {
				if (_parent != null && value != _parent) {
					// Remove Entity from Parent if the new Parent changes
					((List<ITransform>)_parent.Children).Remove(this);
				}
				if (value != null) {
					((List<ITransform>)value.Children).Add(this);
				}

				_parent = value;
				UpdateTransform();
			}
		}

		public Vector3 Forward;

		private Matrix4x4 _worldMatrix = Matrix4x4.identity;
		public Matrix4x4 Matrix {
			get {
				return _worldMatrix;
			}
			set {
				throw new NotSupportedException("Setting the World-Matrix is not supported.");
			}
		}
		private Matrix4x4 _localMatrix = Matrix4x4.identity;
		public Matrix4x4 LocalMatrix {
			get {
				return _localMatrix;
			}
			set {
				_localMatrix = value;
				_localPos = value.GetPosition();
				_localRot = value.rotation;
				_localScale = value.lossyScale;

				UpdateTransform();
			}
		}
		public int MatrixIndex { get; internal set; } = -1;
		public bool IsDirty { get; set; }
		public string Name { get; set; }

		public IReadOnlyList<ITransform> Children { get; } = new List<ITransform>();

		public GPUTransform(string name) {
			this.Name = name;
		}
		public GPUTransform(Component comp, string name = "") : this(name == string.Empty ? comp.name : name) {
			LocalMatrix = Matrix4x4.TRS(comp.transform.localPosition, comp.transform.localRotation, comp.transform.localScale);
		}

		/// <summary>
		/// Sets the Transforms parent.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="keepTransform">Keep the transform the same relative to the world upon parenting.</param>
		public void SetParent(ITransform parent, bool keepTransform) {
			if(keepTransform) {
				Matrix4x4 worldMatrix = Matrix;
				Parent = parent;

				if (parent != null) {
					LocalMatrix = parent.Matrix.inverse * worldMatrix;
				} else {
					LocalMatrix = worldMatrix;
				}
			} else {
				Parent = parent;
			}
		}
		public void UpdateTransform() {
			var localMatrix = _localMatrix;

			if(Parent != null) {
				localMatrix = Parent.Matrix * localMatrix;
			}

			_worldMatrix = localMatrix;

			foreach (ITransform t in Children) {
				t.UpdateTransform();
			}

			SetDirty(false);
		}
		public void SetDirty(bool includeChildren = false) {
			GPURender.SetDirty(this);

			if (includeChildren) {
				foreach (ITransform t in Children) {
					t.SetDirty(includeChildren);
				}
			}
		}

		public override string ToString() {
			string text = Name;
			if(Parent != null) {
				text += $" <- Parent: {((IDescriptor)Parent).Name}";
			}

			if(Children.Count > 0) {
				text += $" | Children [{Children.Count}] -> ";
			}
			foreach(ITransform c in Children) {
				text += $"{((IDescriptor)c).Name}, ";
			}

			return text;
		}
	}
}
