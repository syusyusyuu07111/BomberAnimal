using UnityEngine;

public class CharacterCameraAimer : MonoBehaviour
{
    [Header("References")]
    public Transform bodyYawRoot;   // 体のヨー（水平）を合わせる対象
    public Transform aimPivot;      // 上下を向かせる対象（上半身/武器ルート）
    public Transform muzzle;        // 任意：デバッグ用
    public Camera aimCamera;        // 未設定なら Camera.main

    [Header("Follow Speeds")]
    public float yawFollowSpeed = 720f;
    public float pitchFollowSpeed = 720f;

    [Header("Pitch Clamp")]
    public float minPitch = -40f;   // 下向き限界
    public float maxPitch = 70f;   // 上向き限界

    void Awake()
    {
        if (!aimCamera) aimCamera = Camera.main;
        if (!bodyYawRoot) bodyYawRoot = transform;
    }

    void LateUpdate()
    {
        if (!aimCamera || !bodyYawRoot || !aimPivot) return;

        float dt = Time.unscaledDeltaTime;

        // --- ヨー（体の向き） ---
        float camYaw = aimCamera.transform.eulerAngles.y;
        float bodyYaw = bodyYawRoot.eulerAngles.y;
        float newBodyYaw = Mathf.MoveTowardsAngle(bodyYaw, camYaw, yawFollowSpeed * dt);
        bodyYawRoot.rotation = Quaternion.Euler(0f, newBodyYaw, 0f);

        // --- ピッチ（上下角） ---
        Vector3 fwd = aimCamera.transform.forward.normalized;
        float camPitch = Mathf.Asin(Mathf.Clamp(fwd.y, -1f, 1f)) * Mathf.Rad2Deg;
        camPitch = Mathf.Clamp(camPitch, minPitch, maxPitch);

        // ✨ Unityの回転系では「上を向く = X をマイナス回転」なので反転して適用
        Quaternion targetRot =
            Quaternion.Euler(0f, newBodyYaw, 0f) * Quaternion.Euler(-camPitch, 0f, 0f);

        aimPivot.rotation = Quaternion.RotateTowards(aimPivot.rotation, targetRot, pitchFollowSpeed * dt);

        if (muzzle) Debug.DrawRay(muzzle.position, muzzle.forward * 2f, Color.cyan);
    }
}
