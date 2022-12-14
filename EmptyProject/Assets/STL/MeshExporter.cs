using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.Stl;
using System.IO;

public class MeshExporter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        {
            var mesh = GetComponent<MeshFilter>().mesh;
            mesh.name = gameObject.name;
            var path = (Application.dataPath + "/Export/"+gameObject.name+".stl");
            Exporter.WriteFile(path, new Mesh[] { mesh }, FileType.Binary);
            //string model = Exporter.WriteString(new Mesh[] { mesh });
            //File.WriteAllText(path, model);
            
            //Exporter.Export(Application.dataPath + "/Export", new GameObject[] { gameObject }, FileType.Ascii);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
