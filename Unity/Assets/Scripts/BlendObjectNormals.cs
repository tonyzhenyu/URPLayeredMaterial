using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendObjectNormals : MonoBehaviour
{
    public MeshFilter target;
    public MeshFilter self;// { get { return gameObject.GetComponent<MeshFilter>(); } }
    [Range(0,1)]public float blendfactor = 1;
    public float distweight = 0;
    public float min = 0;
    public float max = 1;
    [ContextMenu("TestBlend")]
    public void BlendNormals()
    {
        var vertexs_target = target.mesh.vertices;
        var vertexs_self = self.mesh.vertices;

        self.mesh.MarkDynamic();
        self.mesh.RecalculateNormals();

        List<Vector3> normalsLst = new List<Vector3>();

        for (int i = 0; i < vertexs_self.Length; i++)
        {
            //float normalWeightTimes = 1;
            Vector3 normal = self.mesh.normals[i];
            for (int j = 0; j < vertexs_target.Length; j++)
            {
                float dist = Vector3.Distance(transform.worldToLocalMatrix * new Vector4(vertexs_self[i].x, vertexs_self[i].y, vertexs_self[i].z, 1)
                    , target.gameObject.transform.localToWorldMatrix * new Vector4(vertexs_target[j].x, vertexs_target[j].y, vertexs_target[j].z, 1));

                if (dist < distweight + min)
                {
                    
                }
                else if (dist > distweight + max)
                {

                }
                else
                {
                    normal = target.mesh.normals[j].normalized;
                }
            }
            normalsLst.Add(normal);
        }
        self.mesh.SetNormals(normalsLst);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
