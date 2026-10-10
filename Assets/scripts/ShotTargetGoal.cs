using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ShotTargetGoal : MonoBehaviour
{
    private readonly HashSet<DisappearOnShot> remainingTargets = new HashSet<DisappearOnShot>();
    private bool completed;
    private GameObject planetFragment;
    private Rigidbody fragmentBody;
    private BoxCollider fragmentCollider;

    public event Action ChallengeCompleted;
    public int RemainingTargetCount => remainingTargets.Count;

    private void Start()
    {
        FindAndPreparePlanetFragment();

        foreach (DisappearOnShot target in FindObjectsOfType<DisappearOnShot>(true))
        {
            if (target.gameObject.scene != gameObject.scene) continue;
            remainingTargets.Add(target);
            target.Shot += OnTargetShot;
        }

        if (remainingTargets.Count == 0)
            Debug.LogError("В лунной зоне нет планет-мишеней с DisappearOnShot.", this);
    }

    private void OnTargetShot(DisappearOnShot target)
    {
        if (completed || !remainingTargets.Remove(target)) return;

        target.Shot -= OnTargetShot;
        if (remainingTargets.Count != 0) return;

        completed = true;
        DropPlanetFragment(target.transform.position);
        ChallengeCompleted?.Invoke();
    }

    private void FindAndPreparePlanetFragment()
    {
        Transform[] sceneTransforms = FindObjectsOfType<Transform>(true);
        foreach (Transform candidate in sceneTransforms)
        {
            if (candidate.gameObject.scene != gameObject.scene ||
                !string.Equals(candidate.name, "planet_fragment", StringComparison.OrdinalIgnoreCase))
                continue;

            planetFragment = candidate.gameObject;
            break;
        }

        if (planetFragment == null)
        {
            planetFragment = CreateFallbackFragment();
            Debug.LogWarning("Не найден planet_fragment.fbx; создан запасной осколок.", this);
        }

        planetFragment = ConfigureFragmentPhysics(planetFragment);
        planetFragment.SetActive(false);
    }

    private static GameObject CreateFallbackFragment()
    {
        GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fragment.name = "planet_fragment";
        fragment.transform.localScale = new Vector3(0.55f, 0.38f, 0.48f);
        Renderer renderer = fragment.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.73f, 0.35f, 0.16f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.28f);
            renderer.sharedMaterial = material;
        }
        return fragment;
    }

    private GameObject ConfigureFragmentPhysics(GameObject visual)
    {
        Vector3 visualPosition = visual.transform.position;
        Quaternion visualRotation = visual.transform.rotation;
        GameObject physicsRoot = new GameObject("Физика осколка планеты");
        physicsRoot.transform.SetPositionAndRotation(visualPosition, visualRotation);
        visual.transform.SetParent(physicsRoot.transform, true);
        GameObject fragment = physicsRoot;

        Renderer[] renderers = fragment.GetComponentsInChildren<Renderer>(true);
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.one * 0.5f);
        bool hasBounds = false;
        foreach (Renderer renderer in renderers)
        {
            Bounds worldBounds = renderer.bounds;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = worldBounds.center + Vector3.Scale(worldBounds.extents, new Vector3(x, y, z));
                Vector3 localCorner = fragment.transform.InverseTransformPoint(corner);
                if (!hasBounds)
                {
                    localBounds = new Bounds(localCorner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(localCorner);
                }
            }
        }

        foreach (Collider collider in fragment.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;

        fragmentCollider = fragment.AddComponent<BoxCollider>();
        fragmentCollider.enabled = true;
        fragmentCollider.isTrigger = false;
        fragmentCollider.center = localBounds.center;
        fragmentCollider.size = new Vector3(
            Mathf.Max(0.2f, localBounds.size.x),
            Mathf.Max(0.2f, localBounds.size.y),
            Mathf.Max(0.2f, localBounds.size.z));

        Transform pickupArea = fragment.transform.Find("Зона подбора осколка");
        if (pickupArea == null)
        {
            GameObject areaObject = new GameObject("Зона подбора осколка");
            pickupArea = areaObject.transform;
            pickupArea.SetParent(fragment.transform, false);
        }

        SphereCollider pickupCollider = pickupArea.GetComponent<SphereCollider>();
        if (pickupCollider == null) pickupCollider = pickupArea.gameObject.AddComponent<SphereCollider>();
        pickupCollider.isTrigger = true;
        pickupCollider.radius = Mathf.Clamp(localBounds.extents.magnitude + 0.65f, 1.05f, 2.2f);

        if (fragment.GetComponent<PlanetFragmentPickup>() == null)
            fragment.AddComponent<PlanetFragmentPickup>();

        fragmentBody = fragment.GetComponent<Rigidbody>();
        if (fragmentBody == null) fragmentBody = fragment.AddComponent<Rigidbody>();
        fragmentBody.mass = 0.8f;
        fragmentBody.drag = 0.25f;
        fragmentBody.angularDrag = 0.8f;
        fragmentBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        fragmentBody.interpolation = RigidbodyInterpolation.Interpolate;
        fragmentBody.useGravity = false;
        fragmentBody.isKinematic = true;
        return fragment;
    }

    private void DropPlanetFragment(Vector3 lastPlanetPosition)
    {
        if (planetFragment == null || fragmentBody == null || fragmentCollider == null) return;

        Vector3 start = lastPlanetPosition + Vector3.up * 3.2f;
        Quaternion startRotation = Quaternion.Euler(12f, 36f, -8f);
        Quaternion landingRotation = Quaternion.Euler(0f, 36f, 0f);
        float groundY = FindLunarSurfaceHeight();
        float lowestPoint = fragmentCollider.center.y - fragmentCollider.size.y * 0.5f;
        Vector3 landing = new Vector3(
            lastPlanetPosition.x + 0.35f,
            groundY - lowestPoint + 0.025f,
            lastPlanetPosition.z + 0.2f);

        planetFragment.transform.SetPositionAndRotation(start, startRotation);
        planetFragment.SetActive(true);
        fragmentBody.velocity = Vector3.zero;
        fragmentBody.angularVelocity = Vector3.zero;
        fragmentBody.useGravity = false;
        fragmentBody.isKinematic = true;
        StartCoroutine(AnimateFragmentDrop(start, landing, startRotation, landingRotation));
    }

    private static float FindLunarSurfaceHeight()
    {
        GameObject podium = GameObject.Find("Подиум экспозиции");
        if (podium != null)
        {
            Renderer renderer = podium.GetComponent<Renderer>();
            if (renderer != null) return renderer.bounds.max.y;
        }

        GameObject platform = GameObject.Find("Cube (6)");
        Collider platformCollider = platform != null ? platform.GetComponent<Collider>() : null;
        return platformCollider != null ? platformCollider.bounds.max.y + 0.2f : 0f;
    }

    private IEnumerator AnimateFragmentDrop(Vector3 start, Vector3 landing, Quaternion startRotation, Quaternion landingRotation)
    {
        const float duration = 1.15f;
        float elapsed = 0f;
        while (elapsed < duration && planetFragment != null)
        {
            yield return new WaitForFixedUpdate();
            elapsed += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - progress) * (1f - progress);
            Vector3 position = Vector3.Lerp(start, landing, eased);
            position.y += Mathf.Sin(progress * Mathf.PI) * 0.35f;
            fragmentBody.MovePosition(position);
            fragmentBody.MoveRotation(Quaternion.Slerp(startRotation, landingRotation, eased));
        }

        if (planetFragment == null) yield break;
        fragmentBody.MovePosition(landing);
        fragmentBody.MoveRotation(landingRotation);
        fragmentBody.velocity = Vector3.zero;
        fragmentBody.angularVelocity = Vector3.zero;
    }

    private void OnDestroy()
    {
        foreach (DisappearOnShot target in remainingTargets)
        {
            if (target != null) target.Shot -= OnTargetShot;
        }
        remainingTargets.Clear();
    }
}
