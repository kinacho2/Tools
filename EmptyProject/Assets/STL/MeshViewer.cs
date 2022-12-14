using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.Stl;
using UnityEditor;

public class MeshViewer : MonoBehaviour
{
    [SerializeField] TextAsset StlFile;
    [SerializeField] Material Material;
    // Start is called before the first frame update
    void Start()
    {
        var file= AssetDatabase.GetAssetPath(StlFile).Replace("Assets", "");
        var path = Application.dataPath + file;
        var meshes = Importer.Import(path);
        int i = 0;
        foreach (var mesh in meshes)
        {
            GameObject renderer = new GameObject();
            renderer.name = file.Replace("/", "").Replace(".txt","") + i; 
            i++;
            renderer.AddComponent<MeshFilter>().mesh = mesh;
            renderer.AddComponent<MeshRenderer>().material = Material;
            renderer.AddComponent<MeshClean>();
                //renderer.AddComponent<MeshExporter>();
        }
    }

   
}
