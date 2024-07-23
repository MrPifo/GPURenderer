namespace Sperlich.GPURender {
	public interface IRender {

		public int InstanceID { get; set; }
		public int BatchID { get; set; }
		public bool IsStatic { get; set; }
		public bool IsRendering { get; set; }
		public string Collection { get; set; }
		public SubMeshRenderData[] RenderData { get; set; }
		public IMatrix IMatrix => (IMatrix)this;

	}
}
