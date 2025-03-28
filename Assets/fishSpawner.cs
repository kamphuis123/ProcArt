using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class fishSpawner : MonoBehaviour
{
    [Header("Size Controls")]
    [Range(0.1f, 10f)]
    public float globalScale = 1f;

    [Range(0.1f, 10f)]
    public float headScale = 1f;

    [Range(0.1f, 10f)]
    public float bodyScale = 1f;

    [Range(0.1f, 10f)]
    public float tailScale = 1f;

    [Header("Target Wiggle Settings")]
    public float wiggleSpeed = 3f;          
    public float wiggleAmount = 0.5f;     
    public bool wiggleEnabled = true;

    [Header("Head Tapering")]
    [Range(0.1f, 0.5f)]
    public float headTipRatio = 0.25f; 
    [Range(0.5f, 1f)]
    public float headBaseScale = 0.75f; 

    [Header("Constraints")]
    [Range(0f, 10f)]
    public float minDistanceConstraint;
    [Range(0f, 10f)]
    public float distanceConstraint;

    [Header("Structure")]
    [Range(1f, 100f)]
    public int jointcount;
    [Range(1f, 30f)]
    public int neckLength;

    [Header("Movement")]
    public GameObject target;
    public int targetX;
    public int targetY;
    public float movementSpeed;

    [Header("GameObjects")]
    public GameObject fishJoint;
    public GameObject fishVertex;

    [Header("Joint/Vertex Lists")]
    public List<GameObject> jointList;
    public List<GameObject> vertexList;

    Mesh mesh;
    public Vector3[] polygonPoints;
    public int[] polygonTriangles;

    private Camera cam;

    public AudioSource audioSource;

    void Start()
    {
        mesh = new Mesh();
        this.GetComponent<MeshFilter>().mesh = mesh;
        cam = Camera.main;
        audioSource.Play();
    }

    void Update()
    {
        neckLength = Mathf.Clamp(neckLength, 1, jointcount - 1);
        drawFilled();

        //Spawn Joints/vertices when needed
        if (jointList.Count < jointcount)
        {
            var newJoint = Instantiate(fishJoint, transform.position, Quaternion.identity);
            var newVertexA = Instantiate(fishVertex, new Vector2(newJoint.transform.position.x, newJoint.transform.position.y + 0.2f), Quaternion.identity, newJoint.transform);
            vertexList.Add(newVertexA);
            var newVertexB = Instantiate(fishVertex, new Vector2(newJoint.transform.position.x, newJoint.transform.position.y - 0.2f), Quaternion.identity, newJoint.transform);
            vertexList.Add(newVertexB);
            jointList.Add(newJoint);
        }
        else if (jointcount < jointList.Count)
        {
            int jointsToRemove = jointList.Count - jointcount;
            for (int i = 0; i < jointsToRemove; i++)
            {
                GameObject jointToRemove = jointList[jointList.Count - 1];
                foreach (Transform child in jointToRemove.transform)
                {
                    if (vertexList.Contains(child.gameObject))
                    {
                        vertexList.Remove(child.gameObject);
                    }
                    Destroy(child.gameObject);
                }
                jointList.RemoveAt(jointList.Count - 1);
                Destroy(jointToRemove);
            }
        }

        followJoints();
        bodyChanges();
    }

    void bodyChanges()
    {

        // Calculate scaled sizes
        float scaledHeadSize = globalScale * headScale;
        float scaledBodySize = globalScale * bodyScale;
        float scaledTailSize = globalScale * tailScale;

        // Head parameters
        int peakHeadJoint = Mathf.Clamp(Mathf.RoundToInt(neckLength * 0.3f), 1, neckLength - 1);


        for (int i = 0; i < jointList.Count; i++)
        {
            var currentJoint = jointList[i];
            float scaleValue;

            if (i == 0)
            {
                // Very tip - smallest size (20% of head scale)
                scaleValue = scaledHeadSize * 0.2f;
            }
            else if (i < peakHeadJoint)
            {
                // Scaling up to full head size
                float t = (float)i / peakHeadJoint;
                scaleValue = Mathf.Lerp(scaledHeadSize * 0.5f, scaledHeadSize, t);
            }
            else if (i == peakHeadJoint)
            {
                // Peak head size
                scaleValue = scaledHeadSize;
            }
            else if (i < neckLength)
            {
                // Tapering down to body size
                float t = (float)(i - peakHeadJoint) / (neckLength - peakHeadJoint);
                scaleValue = Mathf.Lerp(scaledHeadSize, scaledBodySize, t);
            }
            else
            {
                // Body to tail section
                float t = (float)(i - neckLength) / (jointList.Count - neckLength);
                scaleValue = Mathf.Lerp(scaledBodySize, scaledTailSize, t);
            }

            currentJoint.transform.localScale = Vector2.one * scaleValue;
        }
    }

    void followJoints()
    {
        // Calculate wiggled target position
        Vector2 baseTargetPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 wiggledPos = baseTargetPos;

        if (wiggleEnabled && jointList.Count > 0)
        {
            // Fish to target Vector
            Vector2 fishToTarget = baseTargetPos - (Vector2)jointList[0].transform.position;

            audioSource.volume = fishToTarget.magnitude / 100;
            audioSource.pitch = fishToTarget.magnitude / 100;


            if (fishToTarget.magnitude > 10.0f) // Only wiggle if not too close
            {
                // Get perpendicular direction for wiggling
                Vector2 perpendicular = new Vector2(-fishToTarget.y, fishToTarget.x).normalized;

                // Sine wave wiggle in the direction
                float wiggleOffset = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
                wiggledPos = baseTargetPos + perpendicular * wiggleOffset;
            }
        }

        
        target.transform.position = wiggledPos;

        
        for (int i = 0; i < jointList.Count; i++)
        {
            var currentjoint = jointList[i];

            if (i != 0)
            {
                //Rotate and constrain 
                var prevjoint = jointList[i - 1];
                Vector2 direction = (Vector2)currentjoint.transform.position - (Vector2)prevjoint.transform.position;
                float distance = direction.magnitude;

                if (distance > distanceConstraint)
                {
                    Vector2 clampedPosition = (Vector2)prevjoint.transform.position + direction.normalized * distanceConstraint;
                    currentjoint.transform.position = clampedPosition;
                }
                else if (distance < minDistanceConstraint)
                {
                    Vector2 clampedPosition = (Vector2)prevjoint.transform.position + direction.normalized * minDistanceConstraint;
                    currentjoint.transform.position = clampedPosition;
                }

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0, 0, angle);
                currentjoint.transform.rotation = Quaternion.Lerp(currentjoint.transform.rotation, rot, 10f * Time.deltaTime);
            }
            else
            {
                //Rotate head towards target
                currentjoint.transform.position = Vector3.Lerp(currentjoint.transform.position, target.transform.position, movementSpeed * Time.deltaTime);

                Vector2 dir = (Vector2)currentjoint.transform.position - (Vector2)target.transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0, 0, angle);
                currentjoint.transform.rotation = Quaternion.Lerp(currentjoint.transform.rotation, rot, 10f * Time.deltaTime);
            }
        }
    }

    void drawFilled()
    {
        polygonPoints = getPolyPoints(vertexList).ToArray();
        polygonTriangles = DrawFilledTriangles(polygonPoints);
        mesh.Clear();
        mesh.vertices = polygonPoints;
        mesh.triangles = polygonTriangles;
    }

    List<Vector3> getPolyPoints(List<GameObject> vertices)
    {
        return vertices.Select(v => v.transform.position).ToList();
    }

    int[] DrawFilledTriangles(Vector3[] points)
    {
        int triangleAmount = points.Length - 2;
        List<int> newTriangles = new List<int>();
        for (int i = 0; i < triangleAmount; i++)
        {
            newTriangles.Add(i);
            newTriangles.Add(i + 2);
            newTriangles.Add(i + 1);
        }
        return newTriangles.ToArray();
    }
}