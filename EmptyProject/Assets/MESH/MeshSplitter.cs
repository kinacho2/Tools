using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct Triangle
{
    int[] indices;
}

public struct VertexTriangles
{
    public List<int> triangles;
}

public struct MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public int x;
    public int y;

    public override string ToString()
    {
        return $"X: {x} | Y: {y} | Vertices Count: {vertices.Length} | Triangles Count: {triangles.Length}";
    }
}
public class MeshSplitter : MonoBehaviour
{
    const float offset = 0.001f;
    Task task;


    private void Start()
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        var material = GetComponent<MeshRenderer>().material;
        task = Split(mesh, material);
    }

    public async Task Split(Mesh mesh, Material material)
    {
        var list = await Split(mesh);
        int i = 0;
        foreach (var item in list)
        {
            GameObject renderer = new GameObject();
            renderer.name = gameObject.name + i;
            i++;
            renderer.AddComponent<MeshFilter>().mesh = item;
            renderer.AddComponent<MeshRenderer>().material = material;
            //renderer.AddComponent<MeshExporter>();
            renderer.transform.parent = transform;
        }
        GetComponent<MeshRenderer>().enabled = false;
    }
    public static async Task<List<Mesh>> Split(Mesh mesh)
    {
        List<Mesh> result = new List<Mesh>();

        var triangles = await GetVertexTriangles(mesh);
        var indices = mesh.triangles;
        var splited = await CleanMesh(indices, triangles);

        foreach(var list in splited)
        {
            Mesh newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.normals = mesh.normals;
            newMesh.uv = mesh.uv;
            newMesh.triangles = list.ToArray();

            result.Add(newMesh);
        }

        return result;
    }
    public static async Task<List<VertexTriangles>> GetVertexTriangles(Mesh mesh)
    {
        var indices = mesh.triangles;
        var vertices = mesh.vertices;
        var vertexTriangles = new List<VertexTriangles>();
        int j = 0;
        foreach (var v in vertices)
        {
            vertexTriangles.Add(new VertexTriangles() { triangles = new List<int>() });
            j++;
            if (j % 1000 == 0)
            {
                Debug.Log("Progress"+j*1000/vertices.Length);
                await Task.Delay(10);
            }
        }
        j = 0;
        for (int i = 0; i < indices.Length / 3; i++)
        {
            int idx0 = indices[i * 3 + 0];
            int idx1 = indices[i * 3 + 1];
            int idx2 = indices[i * 3 + 2];

            vertexTriangles[idx0].triangles.Add(i);
            vertexTriangles[idx1].triangles.Add(i);
            vertexTriangles[idx2].triangles.Add(i);
            j++;
            if (j % 1000 == 0)
            {
                Debug.Log("Progress" + j * 1000 / indices.Length);
                await Task.Delay(10);
            }
        }
        return vertexTriangles;
    }
    private void OnDestroy()
    {
        task = null;
    }
    public static async Task<List<List<int>>> CleanMesh(int[] triangles, List<VertexTriangles> vertexTriangles)
    {
        List<List<int>> retList = new List<List<int>>();
        int trianglesLenght = triangles.Length / 3;
        int[] visited = new int[trianglesLenght];
        int k = 0;
        for (int i = 0; i < trianglesLenght; i++)
        {
            visited[i] = 0;
            k++;
            if (k % 1000 == 0)
            {
                Debug.Log("Progress visited" + k);
                await Task.Delay(10);
            }
        }
        while (true)
        {
            List<int> indices = new List<int>();
            int first = GetFirstNotVisited(visited);
            Debug.LogError("first " + first);
            if (first < 0) 
                break;
            
            indices.Add(first);
            visited[first] = 1;
            k = 0;
            int j = 0;

            while (j < indices.Count)
            {
                int current = indices[j];
                List<int> l0 = vertexTriangles[triangles[current * 3 + 0]].triangles;
                List<int> l1 = vertexTriangles[triangles[current * 3 + 1]].triangles;
                List<int> l2 = vertexTriangles[triangles[current * 3 + 2]].triangles;

                foreach (var i in l0)
                {
                    if (visited[i] == 0 && (l1.Contains(i) || l2.Contains(i)))
                    {
                        visited[i] = 1;
                        indices.Add(i);
                    }
                }
                foreach (var i in l1)
                {
                    if (visited[i] == 0 && (l0.Contains(i) || l2.Contains(i)))
                    {
                        visited[i] = 1;
                        indices.Add(i);
                    }
                }
                foreach (var i in l2)
                {
                    if (visited[i] == 0 && (l1.Contains(i) || l0.Contains(i)))
                    {
                        visited[i] = 1;
                        indices.Add(i);
                    }
                }
                j++;

                k++;
                if (k % 1000 == 0)
                {
                    Debug.Log("Progress indices " + j*1000/indices.Count);
                    await Task.Delay(10);
                }
            }

            List<int> newTriangles = new List<int>();

            for (int i = 0; i < visited.Length; i++)
            {
                if (visited[i] == 1)
                {
                    newTriangles.Add(triangles[i * 3 + 0]);
                    newTriangles.Add(triangles[i * 3 + 1]);
                    newTriangles.Add(triangles[i * 3 + 2]);
                    visited[i] = 2;
                }

                k++;
                if (k % 1000 == 0)
                {
                    Debug.Log("Progress exp " + k +" "+ visited.Length);
                    await Task.Delay(10);
                }
            }

            retList.Add(newTriangles);
        }

        return retList;
    }

    public static int GetFirstNotVisited(int[] visited)
    {
        for(int i=0; i<visited.Length; i++)
        {
            if(visited[i] == 0)
                return i;
        }
        return -1;
    }
    private IEnumerator ExcrudeMesh(List<int> triangles, List<VertexTriangles> vertexTriangles)
    {
        List<int> excrude = new List<int>();

        int trianglesCount = triangles.Count / 3;
        for (int i = 0; i < trianglesCount; i++)
        {
            int idx0 = triangles[i * 3];
            int idx1 = triangles[i * 3 + 1];
            int idx2 = triangles[i * 3 + 2];
            int count = 0;
            //List<int> visited;
            foreach (var j in vertexTriangles[idx0].triangles)
            {
                if (i == j) continue;
                int idx0_1 = triangles[j * 3];
                int idx1_1 = triangles[j * 3 + 1];
                int idx2_1 = triangles[j * 3 + 2];
                if (
                    idx0 == idx0_1 && idx1 == idx1_1
                    ||
                    idx0 == idx1_1 && idx1 == idx2_1
                    ||
                    idx0 == idx2_1 && idx1 == idx0_1
                    ||
                    idx0 == idx1_1 && idx1 == idx0_1
                    ||
                    idx0 == idx2_1 && idx1 == idx1_1
                    ||
                    idx0 == idx0_1 && idx1 == idx2_1
                )
                {
                    count++;
                    break;
                }
            }
            foreach (var j in vertexTriangles[idx1].triangles)
            {
                if (i == j) continue;
                int idx0_1 = triangles[j * 3];
                int idx1_1 = triangles[j * 3 + 1];
                int idx2_1 = triangles[j * 3 + 2];
                if (
                    idx1 == idx0_1 && idx2 == idx1_1
                    ||
                    idx1 == idx1_1 && idx2 == idx2_1
                    ||
                    idx1 == idx2_1 && idx2 == idx0_1
                    ||
                    idx1 == idx1_1 && idx2 == idx0_1
                    ||
                    idx1 == idx2_1 && idx2 == idx1_1
                    ||
                    idx1 == idx0_1 && idx2 == idx2_1
                )
                {
                    count++;
                    break;
                }


            }
            foreach (var j in vertexTriangles[idx2].triangles)
            {
                if (i == j) continue;
                int idx0_1 = triangles[j * 3];
                int idx1_1 = triangles[j * 3 + 1];
                int idx2_1 = triangles[j * 3 + 2];
                if (
                    idx2 == idx0_1 && idx0 == idx1_1
                    ||
                    idx2 == idx1_1 && idx0 == idx2_1
                    ||
                    idx2 == idx2_1 && idx0 == idx0_1
                    ||
                    idx2 == idx1_1 && idx0 == idx0_1
                    ||
                    idx2 == idx2_1 && idx0 == idx1_1
                    ||
                    idx2 == idx0_1 && idx0 == idx2_1
                )
                {
                    count++;
                    break;
                }


            }

            if (count > 1)
            {
                excrude.Add(idx0);
                excrude.Add(idx1);
                excrude.Add(idx2);
            }

            if (i % 10000 == 0)
            {
                Debug.ClearDeveloperConsole();
                Debug.Log("excrude Triangles " + ((float)i) / ((float)triangles.Count) * 100);

                yield return null;
            }
        }

        yield return null;
        triangles.Clear();
        triangles = excrude;

    }
    private IEnumerator ExportNavMesh(Mesh mesh, int ID)
    {
        var normals = new Vector3[mesh.vertices.Length];

        //mesh.triangles = triangulatedNavMesh.indices;
        //var length = mesh.triangles.Length;
        //var vertices = mesh.vertices;
        //var tri = mesh.triangles;
        //int v0, v1, v2 = 0;
        //Vector3 p0, p1, p2;
        /**
        for (int i = 0; i * 3 < length; i++)
        {
            v0 = tri[i * 3 + 0];
            v1 = tri[i * 3 + 1];
            v2 = tri[i * 3 + 2];
            p0 = vertices[v0];
            p1 = vertices[v1];
            p2 = vertices[v2];

            Vector3 normal = Vector3.Cross((p1 - p0), (p2 - p0)).normalized;
            //normal = Vector3.up;
            normals[v0] += normal;
            normals[v1] += normal;
            normals[v2] += normal;
            if(i%10000 == 0)
            {
                Debug.ClearDeveloperConsole();
                Debug.Log("creating normals " + ((float)i * 3) / ((float)mesh.triangles.Length) * 100);
                yield return null;
            }
        }/**/
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = new Vector3(0, 1, 0);
            if (i % 10000 == 0)
            {
                Debug.ClearDeveloperConsole();
                Debug.Log("normalize normals " + ((float)i) / ((float)mesh.normals.Length) * 100);
                yield return null;
            }
        }

        mesh.normals = normals;

        mesh.uv = new Vector2[mesh.vertices.Length];

        string filename = Application.dataPath + "/" + Path.GetFileNameWithoutExtension(SceneManager.GetActiveScene().name) + "Nav_" + ID.ToString("00000000.##") + " .obj";

        StringBuilder sb = new StringBuilder();
        int c = 0;
        sb.Append("g ").Append(mesh.name).Append("\n");
        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, 0, v.z));
            c++;
            if (c % 10000 == 0)
            {
                Debug.ClearDeveloperConsole();
                Debug.Log("string vertices " + ((float)c) / ((float)mesh.vertices.Length) * 100);
                yield return null;
            }
        }
        c = 0;
        sb.Append("\n");
        foreach (Vector3 v in mesh.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            c++;
            if (c % 10000 == 0)
            {
                Debug.ClearDeveloperConsole();
                Debug.Log("string normals " + ((float)c) / ((float)mesh.normals.Length) * 100);
                yield return null;
            }
        }
        sb.Append("\n");
        c = 0;
        foreach (Vector3 v in mesh.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            c++;
            if (c % 10000 == 0)
            {
                Debug.ClearDeveloperConsole();
                Debug.Log("string uv " + ((float)c) / ((float)mesh.uv.Length) * 100);
                yield return null;
            }
        }
        c = 0;
        for (int material = 0; material < mesh.subMeshCount; material++)
        {
            sb.Append("\n");
            //sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            //sb.Append("usemap ").Append(mats[material].name).Append("\n");

            int[] triangles = mesh.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                c += 3;
                if (c % 10000 == 0)
                {
                    Debug.ClearDeveloperConsole();
                    Debug.Log("string triangles " + ((float)c) / ((float)triangles.Length) * 100);
                    yield return null;
                }
            }
        }
        string toExport = sb.ToString().Replace(",", ".");




        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(toExport);
        }

    }


}
