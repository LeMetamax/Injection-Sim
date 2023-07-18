using UnityEngine;
using DG.Tweening;

public class InjectionSimBodies : MonoBehaviour
{
    [SerializeField] private float _fallDuration = 1.5f;
    [SerializeField] private Transform _containersParent;
    [SerializeField] private Transform _containersEndPosParent;

    private Transform[] _containers;
    private Transform[] _containersEndPos;

    private int _currentVialIndex = 0;

    public static InjectionSimBodies Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        //The rest of this Awake function sets up the containers and their destination points
        //In the hierarchy, you must make sure that the child index of a container is also the child index of a container's end pos
        _containers = new Transform[_containersParent.childCount];
        for (int i = 0; i < _containers.Length; i++)
            _containers[i] = _containersParent.GetChild(i);
        _containersEndPos = new Transform[_containersEndPosParent.childCount];
        for (int i = 0; i < _containersEndPos.Length; i++)
            _containersEndPos[i] = _containersEndPosParent.GetChild(i);
    }

    /// <summary>
    /// Use DOTween to move the next container into place
    /// </summary>
    public void LoadNextContainer()
    {
        if (_currentVialIndex >= _containers.Length) return;    //Be sure that the next container exists by making sure the current index is not mored than all available vials
        _containers[_currentVialIndex].gameObject.SetActive(true);      //Enable the next container
        Vector3 endPos = _containersEndPos[_currentVialIndex].position;     //Get the destination position of the next container so that DoTween will know where to put it
        _containers[_currentVialIndex].DOMove(endPos, _fallDuration);       //Move the next container to the destination
        _currentVialIndex++;    //Select the next container in the array
    }
}