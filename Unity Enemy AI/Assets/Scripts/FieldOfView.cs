using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    private Mesh mesh;
    private Vector3 origin;
    private float startingAngle;    
    private float fov;
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
    }
    private void LateUpdate(){
        //Field of view
        fov = 90f;
        int rayCount = 50;
        float angle = startingAngle;
        float angleIncrease = fov / rayCount;
        float viewDistance = 3f;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit hit = new RaycastHit();
            Ray ray =  new Ray(origin, GetVectorFromAngle(angle));
           

            if (Physics.Raycast(ray, out hit, viewDistance))
            {
                vertex = hit.point;    
                Debug.Log("I hit something");
            } else
            {
                vertex = origin + GetVectorFromAngle(angle) * viewDistance;
            }

            vertices[vertexIndex] = vertex;
            if (i>0)
            {
            triangles[triangleIndex + 0] = 0;
            triangles[triangleIndex + 1] = vertexIndex - 1;
            triangles[triangleIndex + 2] = vertexIndex;

            triangleIndex += 3;
            }

            vertexIndex ++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

    }

// From Codemonkey Utils
    private static Vector3 GetVectorFromAngle(float angle){
        float angleRad = angle * (Mathf.PI/180f);
        return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));

    }
    private static float GetAngleFromVectorFloat(Vector3 dir){
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x)*Mathf.Rad2Deg;
        if (n<0) n+= 360;
        return n;
    }

    public void SetOrigin(Vector3 origin){
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection){
        startingAngle = GetAngleFromVectorFloat(aimDirection) - fov/2;
    }

}
