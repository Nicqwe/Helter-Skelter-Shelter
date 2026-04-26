using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public enum BuildPartType
    {
        None,
        Pillar,
        BeamX,
        BeamZ,
        TopPlate
    }

    [Header("Prefabs")]
    public GameObject pillarPrefab;
    public GameObject beamXPrefab;
    public GameObject beamZPrefab;
    public GameObject topPlatePrefab;

    [Header("Build Settings")]
    public BuildPartType currentPart = BuildPartType.None;
    public float gridSize = 0.2f;
    public LayerMask raycastLayerMask;

    [Header("Beam Tuning")]
    public float beamAlignmentTolerance = 0.25f;
    public float beamLengthOffset = -0.05f;

    [Header("Parents")]
    public Transform placedPartsParent;

    [Header("Selection")]
    public GameObject selectedObject;

    [Header("Beam Placement State")]
    public GameObject firstSelectedPillar;

    void Update()
    {
        HandlePartSelection();
        HandlePlacement();
        HandleSelection();
        HandleDelete();
        HandleClearAll();
    }

    void HandlePartSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentPart = BuildPartType.Pillar;
            firstSelectedPillar = null;
            Debug.Log("Selected part: Pillar");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentPart = BuildPartType.BeamX;
            firstSelectedPillar = null;
            Debug.Log("Selected part: BeamX");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentPart = BuildPartType.BeamZ;
            firstSelectedPillar = null;
            Debug.Log("Selected part: BeamZ");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentPart = BuildPartType.TopPlate;
            firstSelectedPillar = null;
            Debug.Log("Selected part: TopPlate");
        }
    }

    void HandlePlacement()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (currentPart == BuildPartType.None) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, raycastLayerMask))
        {
            if (currentPart == BuildPartType.BeamX || currentPart == BuildPartType.BeamZ)
            {
                HandleBeamPlacement(hit);
                return;
            }

            GameObject prefabToSpawn = GetCurrentPrefab();
            if (prefabToSpawn == null) return;

            Vector3 spawnPos;
            bool canPlace = TryGetPlacementPosition(hit, out spawnPos);

            if (!canPlace)
            {
                Debug.Log("Cannot place this part here.");
                return;
            }

            GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            if (placedPartsParent != null)
            {
                obj.transform.SetParent(placedPartsParent);
            }
        }
    }

    void HandleBeamPlacement(RaycastHit hit)
    {
        GameObject hitObj = hit.collider.gameObject;

        if (!hitObj.CompareTag("Pillar"))
        {
            Debug.Log("A beam must be placed by selecting pillars.");
            return;
        }

        if (firstSelectedPillar == null)
        {
            firstSelectedPillar = hitObj;
            Debug.Log("First pillar selected: " + firstSelectedPillar.name);
            return;
        }

        if (hitObj == firstSelectedPillar)
        {
            Debug.Log("The second selection cannot be the same pillar.");
            return;
        }

        CreateBeamBetweenPillars(firstSelectedPillar, hitObj);
        firstSelectedPillar = null;
    }

    void CreateBeamBetweenPillars(GameObject pillarA, GameObject pillarB)
    {
        Vector3 posA = pillarA.transform.position;
        Vector3 posB = pillarB.transform.position;

        float pillarTopY = GetObjectTopY(pillarA);

        if (Mathf.Abs(GetObjectTopY(pillarA) - GetObjectTopY(pillarB)) > 0.05f)
        {
            Debug.Log("The two pillars are not at the same height.");
            return;
        }

        GameObject beamPrefab = null;

        if (currentPart == BuildPartType.BeamX)
        {
            if (Mathf.Abs(posA.z - posB.z) > beamAlignmentTolerance)
            {
                Debug.Log("BeamX requires the two pillars to be roughly aligned along the X direction.");
                return;
            }

            beamPrefab = beamXPrefab;
        }
        else if (currentPart == BuildPartType.BeamZ)
        {
            if (Mathf.Abs(posA.x - posB.x) > beamAlignmentTolerance)
            {
                Debug.Log("BeamZ requires the two pillars to be roughly aligned along the Z direction.");
                return;
            }

            beamPrefab = beamZPrefab;
        }

        if (beamPrefab == null) return;

        Vector3 centerPos = (posA + posB) / 2f;
        float beamHalfHeight = beamPrefab.transform.localScale.y / 2f;

        Vector3 spawnPos = new Vector3(
            SnapValue(centerPos.x),
            pillarTopY + beamHalfHeight,
            SnapValue(centerPos.z)
        );

        GameObject beam = Instantiate(beamPrefab, spawnPos, Quaternion.identity);

        if (placedPartsParent != null)
        {
            beam.transform.SetParent(placedPartsParent);
        }

        AdjustBeamLength(beam, pillarA, pillarB);

        Debug.Log("Beam created: " + beam.name);
    }

    void AdjustBeamLength(GameObject beam, GameObject pillarA, GameObject pillarB)
    {
        Vector3 posA = pillarA.transform.position;
        Vector3 posB = pillarB.transform.position;

        float distance = Vector3.Distance(
            new Vector3(posA.x, 0f, posA.z),
            new Vector3(posB.x, 0f, posB.z)
        );

        distance += beamLengthOffset;

        Vector3 scale = beam.transform.localScale;

        if (currentPart == BuildPartType.BeamX)
        {
            scale.x = Mathf.Max(0.2f, distance);
        }
        else if (currentPart == BuildPartType.BeamZ)
        {
            scale.z = Mathf.Max(0.2f, distance);
        }

        beam.transform.localScale = scale;
    }

    bool TryGetPlacementPosition(RaycastHit hit, out Vector3 spawnPos)
    {
        spawnPos = Vector3.zero;

        GameObject hitObj = hit.collider.gameObject;
        Vector3 hitPoint = hit.point;

        switch (currentPart)
        {
            case BuildPartType.Pillar:
                if (!hitObj.CompareTag("BuildSurface"))
                    return false;

                spawnPos = GetPillarPlacementPosition(hitObj, hitPoint);
                return true;

            case BuildPartType.TopPlate:
                if (!hitObj.CompareTag("Beam"))
                    return false;

                spawnPos = GetTopPlatePlacementPosition(hitObj);
                return true;
        }

        return false;
    }

    Vector3 GetPillarPlacementPosition(GameObject baseObj, Vector3 hitPoint)
    {
        float baseTopY = GetObjectTopY(baseObj);
        float pillarHalfHeight = pillarPrefab.transform.localScale.y / 2f;

        float x = SnapValue(hitPoint.x);
        float z = SnapValue(hitPoint.z);
        float y = baseTopY + pillarHalfHeight;

        return new Vector3(x, y, z);
    }

    Vector3 GetTopPlatePlacementPosition(GameObject beamObj)
    {
        float beamTopY = GetObjectTopY(beamObj);
        float topPlateHalfHeight = topPlatePrefab.transform.localScale.y / 2f;

        Vector3 beamPos = beamObj.transform.position;
        float x = SnapValue(beamPos.x);
        float z = SnapValue(beamPos.z);
        float y = beamTopY + topPlateHalfHeight;

        return new Vector3(x, y, z);
    }

    float GetObjectTopY(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds.max.y;
        }

        return obj.transform.position.y;
    }

    float SnapValue(float value)
    {
        return Mathf.Round(value / gridSize) * gridSize;
    }

    void HandleSelection()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (placedPartsParent != null && hit.collider.gameObject.transform.parent == placedPartsParent)
            {
                selectedObject = hit.collider.gameObject;
                Debug.Log("Selected object: " + selectedObject.name);
            }
        }
    }

    void HandleDelete()
    {
        if (Input.GetKeyDown(KeyCode.Delete) && selectedObject != null)
        {
            Destroy(selectedObject);
            selectedObject = null;
            Debug.Log("Selected object deleted.");
        }
    }

    void HandleClearAll()
    {
        if (Input.GetKeyDown(KeyCode.C) && placedPartsParent != null)
        {
            for (int i = placedPartsParent.childCount - 1; i >= 0; i--)
            {
                Destroy(placedPartsParent.GetChild(i).gameObject);
            }

            selectedObject = null;
            firstSelectedPillar = null;
            Debug.Log("All placed parts cleared.");
        }
    }

    GameObject GetCurrentPrefab()
    {
        switch (currentPart)
        {
            case BuildPartType.Pillar:
                return pillarPrefab;
            case BuildPartType.BeamX:
                return beamXPrefab;
            case BuildPartType.BeamZ:
                return beamZPrefab;
            case BuildPartType.TopPlate:
                return topPlatePrefab;
            default:
                return null;
        }
    }
}