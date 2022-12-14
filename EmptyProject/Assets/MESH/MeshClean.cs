using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MeshClean : MonoBehaviour
{
    const float offset = 0.001f;
    Task createTask;
    private void Start()
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        var material = GetComponent<MeshRenderer>().material;
        createTask = Combine(mesh, material);
    }
    public async Task Combine(Mesh mesh, Material material)
    {
        await CombineVertices(mesh);
        await Task.Delay(500);
        gameObject.AddComponent<OBJExporter>();

    }


    public async Task CombineVertices(Mesh mesh)
    {
        var indices = mesh.triangles;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int j = 0;
        for (int i = 0; i < indices.Length / 3; i++)
        {
            j++;
            if (j % 500 == 0)
            {
                Debug.Log($"Combine {j} / {indices.Length / 3}");
                await Task.Delay(10);
            }
            int idx0 = indices[i * 3 + 0];
            int idx1 = indices[i * 3 + 1];
            int idx2 = indices[i * 3 + 2];

            if ((mesh.vertices[idx0] - mesh.vertices[idx1]).sqrMagnitude < offset
                ||
                (mesh.vertices[idx0] - mesh.vertices[idx2]).sqrMagnitude < offset
                ||
                (mesh.vertices[idx1] - mesh.vertices[idx2]).sqrMagnitude < offset
                )
            {
                continue;
            }

            int index0 = GetIndex(vertices, mesh.vertices[idx0]);
            int index1 = GetIndex(vertices, mesh.vertices[idx1]);
            int index2 = GetIndex(vertices, mesh.vertices[idx2]);
            if (index0 < 0)
            {
                triangles.Add(vertices.Count);
                vertices.Add(mesh.vertices[idx0]);
                //normals.Add(mesh.normals[idx0]);
            }
            else triangles.Add(index0);
            if (index1 < 0)
            {
                triangles.Add(vertices.Count);
                vertices.Add(mesh.vertices[idx1]);
                //normals.Add(mesh.normals[idx1]);

            }
            else triangles.Add(index1);
            if (index2 < 0)
            {
                triangles.Add(vertices.Count);
                vertices.Add(mesh.vertices[idx2]);
                //normals.Add(mesh.normals[idx2]);
            }
            else triangles.Add(index2);


        }
        mesh.triangles = triangles.ToArray();
        mesh.vertices = vertices.ToArray();
        //mesh.normals = normals.ToArray();

    }
    public int GetIndex(List<Vector3> vertices, Vector3 vertex)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if ((vertices[i] - vertex).sqrMagnitude < offset)
                return i;
        }
        return -1;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
