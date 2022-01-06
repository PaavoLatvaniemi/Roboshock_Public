using UnityEngine;

public class PropellerSpin : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] Vector3 direction = new Vector3(1, 0, 0);
    
    void Update()
    {
        transform.Rotate(direction * speed * Time.deltaTime, Space.Self);
    }
}
