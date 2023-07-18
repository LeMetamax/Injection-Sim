using UnityEngine;

public class Injection : MonoBehaviour
{
    [Header("Draw Sample Settings")]
    [Tooltip("All fluid vials that the injection can draw from should set its layer here here")]
    [SerializeField] private LayerMask _fluidSampleLayer;
    [Tooltip("How long it will take to draw from a vial and into the syringe to maximum capacity")]
    [SerializeField] private float _drawSampleTime = 1.5f;

    [Header("Inject Sample Settings")]
    [Tooltip("All fluid vials that the injection can inject into should set its layer here here")]
    [SerializeField] private LayerMask _receipientLayer;
    [Tooltip("How long it will take to inject a receipient container to maximum capacity")]
    [SerializeField] private float _injectTime = 3f;

    private Camera _camera;                                             //Camera needed for raycasting purposes
    private Syringe _syringe;                                           //The _syringe itself. This handles all the visualizations of the syringe like the liquid being emptied/refilled or the plunger being pushed or pulled
    private InjectionSimBodies _injectionBodies;                        //This handles logic for the containers this injection will inject into...which is just loading the next container

    private float _timer = 0f;                                          //Independent time in seconds used for the calculation of time taken to inject or extract fluid
    private float _initVerticalPosition;                                //This injection's position at the game's first frame
    private Color _sampleColor;                                         //The color that will be injected into a container

    private void Awake()
    {
        _camera = Camera.main;      //Cache the game's camera to its variable
        _syringe = transform.GetChild(0).GetComponent<Syringe>();       //Cache the syringe(attached as a child to this injection)
    }

    private void Start()
    {
        _injectionBodies = InjectionSimBodies.Instance;         //Gets the only instance of the container bodies
        _injectionBodies.LoadNextContainer();                   //Load the first container

        _initVerticalPosition = transform.position.y;           //Cache the current vertical position
    }
    
    private void Update()
    {
#if UNITY_EDITOR
        Vector3 mousePosition = Input.mousePosition;        //This code will not be pushed to build because it is an editor specific code. It gets the mouse position in screen coordinates
#else
        if (Input.touchCount == 0) return;
        Vector3 mousePosition = Input.GetTouch(0).position;     //This is a build specific code that only works on touch screen devices. It gets the touch position in screen coordinates. It uses the first finger currently on the screen
#endif
        Ray ray = _camera.ScreenPointToRay(mousePosition);      //Cast a ray from the camera to the world
        if (!Physics.Raycast(ray, out RaycastHit hitInfo)) return;      //Does the raycast hit anything?
        Vector3 worldPosition = hitInfo.point;                          //Get the world position of whatever the raycast hits
        Vector3 myPosition = worldPosition;
        myPosition.y = _initVerticalPosition;
        transform.position = myPosition;                                //Sets the position of this injection to whatever the raycast hits... This is why the injection follows the mouse position

        if (!_syringe.IsFilled)                         //Is the injection syringe NOT filled?
        {
            //Here, we obtain raycast informations from the mouse position to any object on the receipient layer. This will be needed to handle the extraction from vial sample mechanics
            bool hittingSampleContainer = Physics.Raycast(ray, out RaycastHit hitSample, Mathf.Infinity, _fluidSampleLayer);
            if (!hittingSampleContainer) return;

            HandleDrawSampleLogic(hitSample);

            return;
        }

        //Here, we obtain raycast informations from the mouse position to any object on the receipient layer. This will be needed to handle the injection mechanics. Only happens when the syringe is filled
        bool hittingReceipientContainer = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _receipientLayer);
        if (!hittingReceipientContainer) return;

        HandleInjectionLogic(hit, worldPosition);
    }

    private void HandleDrawSampleLogic(RaycastHit hit)
    {
        if (!hit.transform.TryGetComponent(out MeshRenderer mRenderer)) return;
#if UNITY_EDITOR
        if (!Input.GetMouseButton(0))        //This code will not be pushed to build because it is an editor specific code. It checks if the left mouse button is not being held down
            return;
#else
        Touch touch = Input.GetTouch(0);
        if (!touch.phase.Equals(TouchPhase.Stationary))        //This is a build specific code that only works on touch screen devices. It checks if the player is not holding down the screen with a single touch(or the first touch presently on the screen)
            return;
#endif
        _timer += Time.deltaTime;   //increase the _timer by 1 every second
        float drawAmount = Mathf.Lerp(0f, 1f, _timer / _drawSampleTime);        //Use linear interpolation to ensure that it takes the seconds in "_drawSampleTime" to move the "drawAmount" value from 0 to 1. We use 0 to 1 for the case of simplicity in the sense that 0 means that the fluid is not drawn, 1 meaning that the fluid is completely drawn, 0.5 meaning that the fluid is halfway

        Color _BaseColor = mRenderer.material.GetColor(nameof(_BaseColor));     //cache the vial's color
        if (drawAmount < 1f)
            transform.position = mRenderer.transform.position;              //While drawing fluid from a vial, we make sure the syringe needle is inside the vial
        else
            _sampleColor = _BaseColor;      //Get the vial's color from cache

        _syringe.DrawSyringe(drawAmount, _BaseColor, () => { _timer = 0f; });           //Show visually that the fluid is being drawn(this is done by increasing the fluid in the syringe). We also set the syringe's color here and set "_timer" to 0 when it is completed
    }

    /// <summary>
    /// Injection logic into a container
    /// </summary>
    /// <param name="hit">details obtained from a raycast</param>
    /// <param name="mousePosition">Mouse position in screen coordinates</param>
    private void HandleInjectionLogic(RaycastHit hit, Vector3 worldPosition)
    {
        if (!hit.transform.TryGetComponent(out Container container)) return;        //This ensures that we are really about to hit a container(Because it should have Container class as a component)

        container.UpdateSyringeEffectPosition(worldPosition, out Vector3 penetrationPoint);   //Inject into the selected container and obtain its penetration point

#if UNITY_EDITOR
        if (!Input.GetMouseButton(0))        //This code will not be pushed to build because it is an editor specific code. It checks if the left mouse button is not being held down
#else
        Touch touch = Input.GetTouch(0);
        if (!touch.phase.Equals(TouchPhase.Stationary))        //This is a build specific code that only works on touch screen devices. It checks if the player is not holding down the screen with a single touch(or the first touch presently on the screen)
#endif
        {
            //Suspend injection when the mouse isn't held on
            container.PauseInjection();
            return;
        }
        //The rest of this function will execute only when the player is touching down on the screen or the mouse is held doown

        transform.position = penetrationPoint;      //During injection, we ensure that the syringe stays constant in the container

        _timer += Time.deltaTime;   //increase the _timer by 1 every second
        float injectAmount = Mathf.Lerp(0f, 1f, _timer / _injectTime);  //Smoothly interpolate the time in seconds to the amount of time specified in "_injectTime". This will be used to fill the container and reduce syringe fluid over time

        container.InjectFluid(_sampleColor, injectAmount, OnInjectionDone);    //Inject the fluid into the receipient's container over time specified in "_injectTime"
        _syringe.DecreaseVisibleFluid(injectAmount);                            //Show visually that the fluid is being injected(this is done by reducing the fluid in the syringe)
    }

    /// <summary>
    /// Events that happens when a container is filled
    /// </summary>
    private void OnInjectionDone()
    {
        _timer = 0f;    //Reset the timer
        _injectionBodies.LoadNextContainer();        //Bring out the next container
    }
}