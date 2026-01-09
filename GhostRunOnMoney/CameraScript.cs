
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] Transform ghostTransform;
    void Start()
    {

        Transform ghostTransform = FindObjectOfType<GhostScript>().transform;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(ghostTransform.position.x, transform.position.y, ghostTransform.position.z), 0.1f);
    }
}
