using UnityEngine;

[DisallowMultipleComponent]
public class DestructibleStencil : MonoBehaviour
{
    [Tooltip("���̃I�u�W�F�N�g��p�� StencilRef�B0�Ȃ玩���̔�")]
    [Range(1, 255)] public int stencilRef = 0;

    [Tooltip("��Ȃ�q���玩�����W")]
    public Renderer[] targetRenderers;

    static int _next = 1;

    void OnEnable() { Apply(); }
    void OnValidate() { if (enabled) Apply(); }

    void Apply()
    {
        if (stencilRef <= 0)
        {
            stencilRef = _next++;
            if (_next > 255) _next = 1;
        }

        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);

        var mpb = new MaterialPropertyBlock();
        foreach (var r in targetRenderers)
        {
            if (!r) continue;
            r.GetPropertyBlock(mpb);
            mpb.SetInt("_StencilRef", stencilRef);
            r.SetPropertyBlock(mpb);
        }
    }
}
