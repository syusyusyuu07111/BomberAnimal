using UnityEngine;

public class AimCamera : MonoBehaviour
{
    private GameObject Player;
    private Transform PlayerTransform;
    [SerializeField]private Vector3 offset=new Vector3(0,3,3);
    private void Start()
    {
        Player = GameObject.Find("Player");
        Camera.main.transform.position = Player.transform.position + offset;
    }
    private void LateUpdate()
    {
        Camera.main.transform.position = Player.transform.position + offset;
        transform.LookAt(PlayerTransform);
    }
}
