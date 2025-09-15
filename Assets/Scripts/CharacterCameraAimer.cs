using UnityEngine;

public class CharacterCameraAimer : MonoBehaviour
{
    [Header("References")]
    public Transform bodyYawRoot;   // 体のヨー（水平）を合わせる対象
    public Transform muzzle;        // 任意：デバッグ用
    public Camera aimCamera;        // 未設定なら Camera.main

    [Header("Follow Speeds")]
    public float yawFollowSpeed = 720f;
    void Awake()
    {
        if (!aimCamera) aimCamera = Camera.main;
        if (!bodyYawRoot) bodyYawRoot = transform;
    }

    void LateUpdate()
    {
        if (!aimCamera || !bodyYawRoot) return;
        float dt = Time.unscaledDeltaTime;
        // --- ヨー（体の向き） ---
        float camYaw = aimCamera.transform.eulerAngles.y;
        float bodyYaw = bodyYawRoot.eulerAngles.y;
        float newBodyYaw = Mathf.MoveTowardsAngle(bodyYaw, camYaw, yawFollowSpeed * dt);
        bodyYawRoot.rotation = Quaternion.Euler(0f, newBodyYaw, 0f);
    }
}
