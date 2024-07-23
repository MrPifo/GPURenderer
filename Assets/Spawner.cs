using Sperlich.GPURender;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public float speed;
	public bool noGameObjects;
	public List<GameObject> list;
	public GameObject exampleParentTest;
	private GPUTransform root;
	private ITransform child1;

	void Awake() {
		root = new GPUMesh(list[0]);
		root.Position = new Vector3(0, 1, 0);

		child1 = new GPUMesh(list[1]);
		var child2 = new GPUMesh(list[2]);
		child1.Position = new Vector3(3, 1, 0);
		child2.Position = new Vector3(0, 1, 3);
		child1.SetParent(root, true);
		child2.SetParent(child1, true);

		List<GPUMesh> meshes = new();
		
		StartCoroutine(ILoop());
		StartCoroutine(IAnimate());


		//root = exampleParentTest.ConvertHierarchy();
		//child1 = root.Children[1].Children[0];
		//root.Position += new Vector3(0, 1, 0);
		//root.SetAllDirty();
		//exampleParentTest.SetActive(false);

		IEnumerator ILoop() {
			while(true) {
				if (noGameObjects == false) {
					GameObject obj = Instantiate(list[Random.Range(0, list.Count)], transform);
					obj.transform.position = new Vector3(Random.Range(-8, 8), Random.Range(8, 16), Random.Range(-8, 8));
					obj.GetComponent<GPUMeshRenderer>().Refresh();
				} else {
					var mesh = list[Random.Range(0, list.Count)].CreateRenderer(list[Random.Range(0, list.Count)].tag);
					mesh.Position = new Vector3(Random.Range(-8, 8), Random.Range(8, 16), Random.Range(-8, 8));
					meshes.Add(mesh);
				}

				if (speed > 0) {
					yield return new WaitForSeconds(speed);
				} else {
					yield return null;
				}
			}
		}

		IEnumerator IAnimate() {
			while(true) {
				foreach (var mesh in meshes) {
					mesh.Position = new Vector3(mesh.Position.x, Mathf.Sin(Time.time * (mesh.GetHashCode() % 100) / 30f) * 2 + 5, mesh.Position.z);
					mesh.Rotation *= Quaternion.Euler(mesh.Position * Time.deltaTime * 10);
				}
				yield return null;
			}
		}
	}

	void Update() {
		Vector3 moveDir = Vector3.zero;
		Vector2 childRot = new Vector2(0, 0);
		float upDown = 0;

		if(Input.GetKey(KeyCode.W)) {
			moveDir.z = 1;
		} else if(Input.GetKey(KeyCode.S)) {
			moveDir.z = -1;
		}
		if (Input.GetKey(KeyCode.D)) {
			moveDir.x = 1;
		} else if (Input.GetKey(KeyCode.A)) {
			moveDir.x = -1;
		}

		if(Input.GetKey(KeyCode.Space)) {
			upDown = 1;
		} else if(Input.GetKey(KeyCode.LeftShift)) {
			upDown = -1;
		}

		if(Input.GetKey(KeyCode.RightArrow)) {
			childRot.x = 1;
		} else if(Input.GetKey(KeyCode.LeftArrow)) {
			childRot.x = -1;
		}
		if(Input.GetKey(KeyCode.UpArrow)) {
			childRot.y = 1;
		} else if (Input.GetKey(KeyCode.DownArrow)) {
			childRot.y = -1;
		}

		root.Position += moveDir * Time.deltaTime * 5;
		child1.Position += new Vector3(0, upDown * Time.deltaTime * 5, 0);
		child1.Rotation *= Quaternion.Euler(childRot.x * Time.deltaTime * 120, 0, childRot.y * Time.deltaTime * 125);

		if (moveDir != Vector3.zero) {
			root.Rotation = Quaternion.Slerp(root.Rotation, Quaternion.LookRotation(moveDir.normalized, Vector3.up), Time.deltaTime * 8);
		}
	}
}
