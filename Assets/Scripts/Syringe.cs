using System;
using UnityEngine;

public class Syringe : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;        //Mesh renderer representing the liquid in the syringe
    [SerializeField] private Transform _plunger;                //Syringe plunger
    [SerializeField] private Transform _plungerEndPoint;        //The stoping point of the plunger when it is being pushed

    private Wobble _wobble;                                     //A component that simulates a fluid shaking because it was moved/rotated

    public bool IsFilled { get; private set; }                  //A property denoting if the injection is filled with liquid from a vial or not

    private Vector3 _initPlungerLocalPos;                       //The syringe's initial position on it's local axis at the first frame

    //NOTE: I USE "nameof(zzzzz)" TO PREVENT STRING ERRORS BECAUSE "nameof(zzzzz)" CONVERTS THE NAME OF A VARIABLE/CLASS/FUNCTION INTO A STRING. IT IS VERY HANDY

    private void Awake()
    {
        _wobble = _meshRenderer.GetComponent<Wobble>();         //Cache the wobble component

        _initPlungerLocalPos = _plunger.localPosition;          //Cache the initial local position

        //The rest of this function makes it so that when the game begins, the syringe will be empty
        _plunger.localPosition = _plungerEndPoint.localPosition;
        float _Fill = 0f;
        _meshRenderer.material.SetFloat(nameof(_Fill), _Fill);
    }

    /// <summary>
    /// Vial injection visualization
    /// </summary>
    /// <param name="fillValue">Clamped range of 0 to 1 depicting the quantity of vial injected</param>
    public void DecreaseVisibleFluid(float fillValue)
    {
        if (!IsFilled) return;      //Do not do anything if this syringe is not filled
        _wobble.enabled = true;     //enable the wobble component

        float _Fill = 1 - fillValue;        //Since the fillValue returns a floating point value that is between 0 and 1(in that order), we negate it, so that the _Fill value will return a floating point value that is between 1 and 0(in this order..reversed)
        _plunger.localPosition = Vector3.Lerp(_initPlungerLocalPos, _plungerEndPoint.localPosition, fillValue);         //Use unity's linear interpolation formula to smoothly move the plunger downwards and towards _plungerEndPoint.localPosition simulating the effect that the syringe is being injected based on the values of fillValue
        _meshRenderer.material.SetFloat(nameof(_Fill), _Fill);                       //Decrease the syringe's fluid based on the calculations gotten from _Fill
        if (_Fill <= 0f)
            IsFilled = false;                   //Once the fluid is completely drained, we know that this syringe is now empty and is in need to be filled
    }

    /// <summary>
    /// Vial extraction visualization
    /// </summary>
    /// <param name="drawAmount">Clamped range of 0 to 1 depicting the quantity of vial drawn</param>
    /// <param name="_LiquidColor">Liquid color to apply on liquid mesh</param>
    /// <param name="onDrawCompleted">Event for when the extraction is completed</param>
    public void DrawSyringe(float drawAmount, Color _LiquidColor, Action onDrawCompleted)
    {
        if (IsFilled) return;      //Do not do anything if this syringe is filled
        _wobble.enabled = false;     //disable the wobble component

        float _Fill = 1 - drawAmount;        //Since the fillValue returns a floating point value that is between 0 and 1(in that order), we negate it, so that the _Fill value be reversed(ie going from 1 to 0)
        _plunger.localPosition = Vector3.Lerp(_initPlungerLocalPos, _plungerEndPoint.localPosition, _Fill);         //Use unity's linear interpolation formula to smoothly move the plunger upwards and towards _initPlungerLocalPos.. simulating the effect that the syringe is extracting fluid based on the values of _Fill
        _meshRenderer.material.SetFloat(nameof(_Fill), drawAmount);                       //Increase the syringe's fluid based on the calculations gotten from _Fill
        _meshRenderer.material.SetColor(nameof(_LiquidColor), _LiquidColor);                //Set the syringe's liquid color
        if (_Fill <= 0f)
        {
            IsFilled = true;        //Acknowledge that the syringe is filled
            onDrawCompleted();      //Trigger an event once the fluid extraction is completed
        }
    }
}