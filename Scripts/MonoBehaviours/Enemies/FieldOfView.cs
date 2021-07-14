using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Generic.Utiles;

namespace Game.MonoBehaviours.Enemies
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class FieldOfView : MonoBehaviour
    {
        [Header("Set in inspector")]
        [SerializeField] Color normalColor;
        [SerializeField] Color huntingColor;

        // Required components
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        /// <summary>
        /// Field of view radius is usually related to the character viewing distance and determines the max distance of the mesh 
        /// </summary>
        float radius;
        /// <summary>
        /// The range determines the total angle of the mesh created being -1 = 360deg, 0 = 180deg and 1 = 0deg
        /// </summary>
        float range;
        /// <summary>
        /// Float value that determines the vision range in the Y direction
        /// </summary>
        float vision_Y;

        // Parameters used for the creation of the mesh
        int lines = 20; // Smoothness of the mesh
        [SerializeField] int subdisivions = 6;
        float startAngle;
        float angleStep;

        // New mesh components
        Mesh mesh;
        Vector3[] vertices;
        Vector2[] uvs;
        int[] triangles;

        // Mesh material
        Material material;

        // Once generated this flag allows the mesh to be updated in the LateUpdate call
        [SerializeField] bool generated = false;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            material = meshRenderer.material;
        }

        private void LateUpdate()
        {
            if (!generated)
                return;

            if (meshRenderer.isVisible)
                UpdateMesh();
        }

        public void GenerateMesh(float _radius, float _range, float _vision_Y)
        {
            // Create a new mesh and assign it to the mesh filter
            mesh = new Mesh();
            if (meshFilter != null)
                meshFilter.mesh = mesh;

            // The number of vertices depends on how many lines we want to use to build the mesh
            // Standard field of view will use as many vertices as lines + 1 for the center and + 1 for the end
            vertices = new Vector3[lines*subdisivions + 1 + 1];
            vertices[0] = Vector3.zero; // Assign first vertice already to the origin position
            uvs = new Vector2[vertices.Length]; // Uvs are not modified in here, but must be included in the creation of the mesh            
            triangles = new int[6 * vertices.Length]; // Each triangle needs 3 vertices, thus we need triple triangles than vertices are

            // Get radius and range parameters
            radius = _radius;
            range = _range;
            vision_Y = _vision_Y;

            // Calculate starting angle from range considering the mesh created following the default XZ plane (no rotation)
            startAngle = 0f;
            if (range < 0f)
                startAngle = (1 - Mathf.Abs(range)) * (270 - 180) + 180;
            else
                startAngle = Mathf.Abs(range) * (360 - 270) + 270;

            // Determine angle step from the amount of lines we want to create
            angleStep = (360 - startAngle) * 2 / (lines);

            // Now we can set the generated flag as true
            generated = true;
        }
        void UpdateMesh()
        {
            // First calculate the angle offset from the default XZ plane
            float angleOffset = Vector3.SignedAngle(Vector3.forward, transform.forward.normalized, Vector3.up);
            float angle = startAngle;
            int verticesIndex = 1;
            int trianglesInidex = 0;

            // We start creating the mesh from vertice 1 as vertice 0 is the origin
            for (int i = 1; i < lines + 1; i++)
            {
                Vector3 direction = transform.forward.normalized * radius * Mathf.Cos(Mathf.Deg2Rad * angle) + transform.right.normalized * radius * Mathf.Sin(Mathf.Deg2Rad * angle);

                // Hit any obstacle
                RaycastHit hitStraight;
                if (Physics.Raycast(transform.position + Vector3.up * vision_Y, direction.normalized, out hitStraight, radius, LayerMasks.FieldOfViewMask()))
                    direction = hitStraight.point - transform.position;

                // Wrapp to terrain
                RaycastHit hitDown;
                for (int subdivision = 1; subdivision <= subdisivions; subdivision++)
                {
                    // The player height
                    float visionInNegativeY = 0f;
                    Vector3 newDirection = direction * (subdivision / (float)subdisivions);

                    // Used to draw in editor the mesh
                    if (Application.isEditor)
                        visionInNegativeY = 0.5f;
                    else
                        visionInNegativeY = Player.S.NavMeshBodyHeight();

                    if (Physics.Raycast(newDirection + transform.position + Vector3.up * vision_Y, -transform.up.normalized, out hitDown, visionInNegativeY + vision_Y, LayerMasks.FieldOfViewMask()))
                    { 
                        float yHeight = hitDown.point.y + 0.05f - transform.position.y;
                        newDirection.y = yHeight;
                        newDirection = Quaternion.AngleAxis(-angleOffset, Vector3.up) * newDirection;
                        vertices[verticesIndex] = newDirection;
  
                        if (verticesIndex >= subdisivions+1)
                        {
                            if (subdivision == 1)
                            {
                                triangles[trianglesInidex] = 0;
                                triangles[trianglesInidex + 1] = verticesIndex - subdisivions;
                                triangles[trianglesInidex + 2] = verticesIndex;
                                trianglesInidex += 3;
                            }
                            else
                            {
                                triangles[trianglesInidex] = verticesIndex - (subdisivions+1);
                                triangles[trianglesInidex + 1] = verticesIndex - subdisivions;
                                triangles[trianglesInidex + 2] = verticesIndex;
                                triangles[trianglesInidex + 3] = verticesIndex;
                                triangles[trianglesInidex + 4] = verticesIndex - 1;
                                triangles[trianglesInidex + 5] = verticesIndex - (subdisivions + 1);
                                trianglesInidex += 6;
                            }
                        }
                    }
                    else
                    {
                        newDirection = Quaternion.AngleAxis(-angleOffset, Vector3.up) * newDirection;
                        vertices[verticesIndex] = newDirection;
                    }
                    verticesIndex++;
                }

                // Increase angle step and recalculate when above 360
                angle += angleStep;
                if (angle > 360)
                    angle = angle - 360;
            }

            // Assign parameters to mesh
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
        }

        #region Setters

        public void SetNormalColor() => material.color = normalColor;
        public void SetHuntingColor() => material.color = huntingColor;
        public void SetRadius(float _radius) => radius = _radius;

        #endregion

        #region Accessors

        public bool HasGeneratedMesh() => generated;

        #endregion


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (generated)
            {
                UpdateMesh();
                mesh.RecalculateNormals();
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireMesh(mesh, transform.position + Vector3.up*0.01f, transform.rotation);
        }
#endif
    }

}
