using UnityEngine;
using System.Collections;

public class GunVisual : MonoBehaviour
{
    [Header("Visual Refs")]
    public GameObject muzzleFlash;     // asigna MuzzleFlash
    public LineRenderer bulletLine;    // asigna BulletTracer
    public GameObject impactSparkPrefab; // opcional (puede ir null)

    [Header("Timing")]
    public float flashTime = 0.05f;
    public float tracerTime = 0.05f;
    public float sparkLife = 0.2f;

    public void PlayShotEffect(Vector3 startWorld, Vector3 endWorld)
    {
        if (muzzleFlash != null)
            StartCoroutine(FlashRoutine());

        if (bulletLine != null)
            StartCoroutine(TracerRoutine(startWorld, endWorld));

        if (impactSparkPrefab != null)
            SpawnSpark(endWorld);
    }

    IEnumerator FlashRoutine()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(flashTime);
        muzzleFlash.SetActive(false);
    }

    IEnumerator TracerRoutine(Vector3 startWorld, Vector3 endWorld)
    {
        bulletLine.enabled = true;
        bulletLine.SetPosition(0, startWorld);
        bulletLine.SetPosition(1, endWorld);

        yield return new WaitForSeconds(tracerTime);

        bulletLine.enabled = false;
    }

    void SpawnSpark(Vector3 pos)
    {
        if (impactSparkPrefab == null) return;

        var spark = Instantiate(impactSparkPrefab, pos, Quaternion.identity);
        Destroy(spark, sparkLife);
    }
}