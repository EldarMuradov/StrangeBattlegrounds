using System.Collections;
using UnityEngine;

public class RecoilScript : MonoBehaviour
{
    public void ShakeRotateCamera(float duration, float angleDeg, Vector2 direction)
    {
        StartCoroutine(ShakeRotateCor(duration, angleDeg, direction));
    }
    private IEnumerator ShakeRotateCor(float duration, float angleDeg, Vector2 direction)
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.localRotation;
        float halfDuration = duration / 2;
        direction = direction.normalized;
        while (elapsed < duration)
        {
            Vector2 currentDirection = direction;
            float t = elapsed < halfDuration ? elapsed / halfDuration : (duration - elapsed) / halfDuration;
            float currentAngle = Mathf.Lerp(0f, angleDeg, t);
            currentDirection *= Mathf.Tan(currentAngle * Mathf.Deg2Rad);
            Vector3 resDirection = ((Vector3)currentDirection + Vector3.forward).normalized;
            transform.localRotation = Quaternion.FromToRotation(Vector3.forward, resDirection);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = startRotation;
    }
}
