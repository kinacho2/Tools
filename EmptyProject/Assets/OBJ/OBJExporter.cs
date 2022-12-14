using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class OBJExporter : MonoBehaviour
{

    private void Start()
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        StartCoroutine(ExportObj(mesh));
    }
    public IEnumerator ExportObj(Mesh mesh)
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

        string filename = Application.dataPath + "/Export/Obj/" + gameObject.name  + " .obj";

        StringBuilder sb = new StringBuilder();
        int c = 0;
        sb.Append("g ").Append(mesh.name).Append("\n");
        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
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
