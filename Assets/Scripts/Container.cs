using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Container : MonoBehaviour
{
    [SerializeField] private ParticleSystem _syringeEffect;

    private float _Fill = 0;
    private bool _isFilling = false;
    private bool _isFilled = false;
    private MeshRenderer _meshRenderer;

    public GameObject boundaryObj;
    private GameObject[] boundaryObjects;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        SetPlungeDirection();

        //The rest of this function caches the boundaries according to the collider
        boundaryObjects = new GameObject[boundaryObj.transform.childCount];
        for (int i = 0; i < boundaryObjects.Length; i++)
            boundaryObjects[i] = boundaryObj.transform.GetChild(i).gameObject;
    }

    /// <summary>
    /// This sets the direction the fluid will be filled from
    /// </summary>
    private void SetPlungeDirection()
    {
        float _xValue = Random.value > 0.5f ? 1f : -1f;
        _meshRenderer.material.SetFloat(nameof(_xValue), _xValue);
        if (_xValue > 0f)
            _syringeEffect.transform.rotation = Quaternion.LookRotation(Vector3.left);
        else
            _syringeEffect.transform.rotation = Quaternion.LookRotation(Vector3.right);
    }

    /// <summary>
    /// Updates the penetration/liquid dispense points
    /// </summary>
    /// <param name="position">Mouse/Touch position</param>
    /// <param name="penetrationPoint">The point in which the injection can be inserted</param>
    public void UpdateSyringeEffectPosition(Vector3 position, out Vector3 penetrationPoint)
    {
        if (_isFilling)
        {
            penetrationPoint = _syringeEffect.transform.position;
            return;
        }
        float clampedX = Mathf.Clamp(position.x, GetMinX(), GetMaxX());
        float clampedZ = Mathf.Clamp(position.z, GetMinZ(), GetMaxZ());
        Vector3 newPos = new(clampedX, _syringeEffect.transform.position.y, clampedZ);
        _syringeEffect.transform.position = newPos;


        //position.y = _syringeEffect.transform.position.y;
        //_syringeEffect.transform.position = position;

        penetrationPoint = _syringeEffect.transform.position;
    }

    /// <summary>
    /// Injects the fluid with the color over time
    /// </summary>
    /// <param name="_LiquidColor">Color of the fluid</param>
    /// <param name="fillValue">Quantity filled</param>
    /// <param name="onComplete">Event triggered whe injection is completed</param>
    public void InjectFluid(Color _LiquidColor, float fillValue, Action onComplete)
    {
        if (_isFilled) return;
        _isFilling = true;
        _meshRenderer.material.SetColor(nameof(_LiquidColor), _LiquidColor);        //Update the fluid's color
        ParticleSystem.MainModule mainModule = _syringeEffect.main;
        mainModule.startColor = _LiquidColor;                 //Update the particle's color

        if (!_syringeEffect.isPlaying)
            _syringeEffect.Play();
        _Fill = fillValue;
        _isFilled = _Fill >= 1f;
        if (_isFilled)
        {
            _Fill = 1f;
            _isFilling = false;
            _syringeEffect.Stop();
            onComplete();
        }
        _meshRenderer.material.SetFloat(nameof(_Fill), _Fill);
    }

    /// <summary>
    /// Suspend injection
    /// </summary>
    public void PauseInjection()
    {
        _syringeEffect.Stop();
        _isFilling = false;
    }

    #region CALCULATE POSSIBLE BOUNDARY POINTS BASED ON COLLIDERS
    private float GetMinX()
    {
        float minX = float.MaxValue;

        foreach (GameObject boundary in boundaryObjects)
        {
            if (!boundary.TryGetComponent(out BoxCollider collider)) continue;
            float boundaryMinX = boundary.transform.position.x - collider.size.x / 2f;
            if (boundaryMinX < minX)
                minX = boundaryMinX;
        }
        return minX;
    }

    private float GetMaxX()
    {
        float maxX = float.MinValue;

        foreach (GameObject boundary in boundaryObjects)
        {
            if (!boundary.TryGetComponent(out BoxCollider collider)) continue;
            float boundaryMaxX = boundary.transform.position.x + collider.size.x / 2f;
            if (boundaryMaxX > maxX)
                maxX = boundaryMaxX;
        }
        return maxX;
    }

    private float GetMinZ()
    {
        float minZ = float.MaxValue;

        foreach (GameObject boundary in boundaryObjects)
        {
            if (!boundary.TryGetComponent(out BoxCollider collider)) continue;
            float boundaryMinZ = boundary.transform.position.z - collider.size.z / 2f;
            if (boundaryMinZ < minZ)
                minZ = boundaryMinZ;
        }
        return minZ;
    }

    private float GetMaxZ()
    {
        float maxZ = float.MinValue;

        foreach (GameObject boundary in boundaryObjects)
        {
            if (!boundary.TryGetComponent(out BoxCollider collider)) continue;
            float boundaryMaxZ = boundary.transform.position.z + collider.size.z / 2f;
            if (boundaryMaxZ > maxZ)
                maxZ = boundaryMaxZ;
        }
        return maxZ;
    }
    #endregion
}