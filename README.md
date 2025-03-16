# Sperlich.GPURender

A high-performance mesh instancing system for Unity that provides efficient GPU-accelerated rendering through batched draw calls. This library allows you to easily instance and manage thousands of mesh renderers while maintaining excellent performance.

## Features

- Efficient GPU instancing for rendering large numbers of identical meshes
- Hierarchical transform system that mimics Unity's transform hierarchy
- Collection-based organization for easy management of render groups
- Runtime and editor support
- Automatic batching of compatible meshes
- Simple API for creating, managing, and updating renderable objects
- Supports both static and dynamic objects

## Installation

1. Add this package to your Unity project using one of the following methods:
   - Via Unity Package Manager: Add from git URL `https://github.com/MrPifo/GPURenderer.git`
   - Manual installation: Copy the `Sperlich.GPURender` folder into your project's Assets folder

2. Import the namespace in your scripts:
```csharp
using Sperlich.GPURender;
```

## Quick Start

### Basic Usage

```csharp
// Create a GPU mesh from a GameObject
GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
GPUMesh gpuCube = new GPUMesh(cube);

// Enable it for rendering
gpuCube.Enable();

// Move it around
gpuCube.Position = new Vector3(5, 0, 0);
```

### Using the Component Approach

```csharp
// Add the RenderMesh component to an existing GameObject
GameObject model = Instantiate(yourPrefab);
RenderMesh renderer = model.AddComponent<RenderMesh>();
renderer.Collection = Collection.Default;
```

### Working with Collections

```csharp
// Create a GPUMesh and assign it to a specific collection
GPUMesh mesh = new GPUMesh(meshRenderer, meshFilter, layer, Collection.Default, name);
mesh.Enable();

// Later, you can manage entire collections at once
GPURender.ClearCollection(Collection.Default);
```

### Creating a Hierarchy of Objects

```csharp
// Create a parent transform
GPUTransform parent = new GPUTransform("Parent");

// Create a child transform
GPUMesh child = new GPUMesh(childObject);
child.Parent = parent;

// Moving the parent will also move the child
parent.Position = new Vector3(0, 10, 0);
```

## Key API Reference

### GPUMesh

Main class for rendering meshes through GPU instancing.

```csharp
// Constructors
GPUMesh(GameObject obj, Collection collection = default)
GPUMesh(Component comp, Collection collection = default)
GPUMesh(MeshRenderer render, MeshFilter filter, int layer, Collection collection = default)
GPUMesh(Mesh mesh, Material[] materials, int layer, Collection collection = default, string name = "")
GPUMesh(MeshSet set, Collection collection = default, string name = "")

// Properties
Vector3 Position { get; set; }
Quaternion Rotation { get; set; }
Vector3 Scale { get; set; }
bool IsRendering { get; set; }
Collection Collection { get; set; }

// Methods
void Enable()
void Disable()
void Refresh()
void SwapMeshInfo(Mesh mesh, Material[] materials, int layer)
void SwapMaterials(Material[] materials)
void SwapCollection(Collection newCollection)
```

### GPUTransform

Base class for transformable objects in the GPU rendering system.

```csharp
// Properties
Vector3 Position { get; set; }
Quaternion Rotation { get; set; }
Vector3 Scale { get; set; }
ITransform Parent { get; set; }
Matrix4x4 Matrix { get; }
IReadOnlyList<ITransform> Children { get; }

// Methods
void SetParent(ITransform parent, bool keepTransform)
void UpdateTransform()
void SetDirty(bool includeChildren = false)
```

### GPURender

Static class that manages the rendering system.

```csharp
// Methods
static void Subscribe(IRender instance)
static void Unsubscribe(IRender instance)
static void RefreshInstance(IRender render)
static void Clear()
static void ClearCollection(Collection collection)
static int GenerateUniqueID()
static void SetDirty(ITransform transform)
```

### RenderMesh

MonoBehaviour component for easy integration with Unity's GameObject system.

```csharp
// Properties
bool isStatic
bool includeChildren
Collection Collection { get; set; }

// Methods
void SetMeshData(Mesh mesh, Material[] materials, int layer = -1)
void SetMaterial(Material material, int index)
void SetMaterials(Material[] materials)
void SetLayer(int layer)
void SetCollection(Collection collection)
```

## Working with MeshSet

The `MeshSet` struct is a fundamental data container that holds all rendering information for meshes in the system.

### Overview

```csharp
public struct MeshSet {
    public Mesh mesh;
    public Material[] materials;
    public int layer;
    
    public int SubMeshCount { get; }
    public bool IsValid { get; }
    
    // Constructors and methods
}
```

A `MeshSet` contains:
- The mesh to be rendered
- An array of materials (one per submesh)
- The layer for rendering

### Creating a MeshSet

```csharp
// Create from components
Mesh myMesh = GetComponent<MeshFilter>().sharedMesh;
Material[] myMaterials = GetComponent<MeshRenderer>().sharedMaterials;
int myLayer = gameObject.layer;
MeshSet meshSet = new MeshSet(myMesh, myMaterials, myLayer);

// Create by copying an existing MeshSet
MeshSet copySet = new MeshSet(meshSet);
```

### Validation

Always ensure your MeshSet is valid before use:

```csharp
// Check validity
if (meshSet.IsValid) {
    // Use the meshSet
}

// Or use the throw method when you want immediate errors
meshSet.ThrowErrorIfInvalid();
```

### Usage Examples

```csharp
// Create a GPU mesh with a custom MeshSet
MeshSet tankMeshSet = new MeshSet(tankMesh, tankMaterials, tankLayer);
GPUMesh tank = new GPUMesh(tankMeshSet, Collection.Tank, "Tank01");
tank.Enable();

// Swap mesh data at runtime
GPUMesh renderer = existingObject.CreateRenderer();
MeshSet damagedVersion = new MeshSet(damagedMesh, damagedMaterials, renderer.MeshSet.layer);
renderer.SwapMeshInfo(damagedVersion);
```

### Common Operations

```csharp
// Getting render data for GPU instancing
Matrix4x4 worldMatrix = transform.localToWorldMatrix;
SubMeshRenderData[] renderData = meshSet.GetRenderData(worldMatrix);

// Changing materials while keeping the same mesh
Material[] newMaterials = GetUpgradedMaterials();
MeshSet upgradedSet = new MeshSet(meshSet.mesh, newMaterials, meshSet.layer);
gpuMesh.SwapMeshInfo(upgradedSet);
```

### Extension Methods

```csharp
// GameObject/Component Extensions
GPUMesh CreateRenderer(this GameObject obj, bool autoEnable = true)
GPUMesh CreateRenderer(this GameObject obj, Collection collection, bool autoEnable = true)
GPUTransform ConvertHierarchy(this GameObject obj)

// IRender Extensions
void Enable(this IRender render)
void Disable(this IRender render)
void Refresh(this IRender render)
void SwapCollection(this IRender render, Collection newCollection)

// IMeshRenderInfo Extensions
void SwapMeshInfo(this IMeshRenderInfo info, Mesh mesh, Material[] materials, int layer)
void SwapMeshInfo(this IMeshRenderInfo info, MeshSet set)
void SwapMaterials(this IMeshRenderInfo info, Material[] materials)
void SwapMaterials(this IMeshRenderInfo info, Material material, int index)
```

## Performance Considerations

- Enable GPU instancing on all materials you plan to render with this system
- Group similar meshes into the same Collection for better batching
- Use `IsStatic = true` for objects that don't move frequently
- Minimize changes to transform hierarchies at runtime

## Requirements

- Unity 2019.4 or higher
- URP or HDRP rendering pipeline (Built-in pipeline is also supported)
- Shader support for GPU instancing
- Ensure Materials support GPU-Instancing

## License

[Include your license information here]
