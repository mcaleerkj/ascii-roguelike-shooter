using UnityEngine;

[ExecuteAlways]
public class PixelSnapper : MonoBehaviour
{
    public Camera targetCamera;
    public bool includeChildren = false;

    void LateUpdate()
    {
        var cam = targetCamera ? targetCamera : Camera.main;
        if (!cam) return;

        // Snap self
        transform.position = PixelSnap.SnapWorldToPixel(cam, transform.position);

        // Optionally snap children (avoid if parent has non-1 scale)
        if (includeChildren && Mathf.Approximately(transform.lossyScale.x, 1f) && Mathf.Approximately(transform.lossyScale.y, 1f))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var c = transform.GetChild(i);
                c.position = PixelSnap.SnapWorldToPixel(cam, c.position);
            }
        }
    }
}
