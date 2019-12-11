using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class VirtualFaderAxis : MonoBehaviour
{

    [SerializeField] TEST _test;

    public static bool NearlyEqual(float a, float b, float epsilon)
    {
        float absA = Mathf.Abs(a);
        float absB = Mathf.Abs(b);
        float diff = Mathf.Abs(a - b);

        if (a == b)
        { // shortcut, handles infinities
            return true;
        }
        //else if (a == 0 || b == 0 || absA + absB < float.nor)
        //{
        // a or b is zero or both are extremely close to it
        // relative error is less meaningful here
        //return diff < (epsilon * Float.MIN_NORMAL);
        //}
        else
        { // use relative error
            return diff < epsilon;
        }
    }

    public enum AxisSliders {
        X_AXIS_MIN,
        X_AXIS_MAX,
        Y_AXIS_MIN,
        Y_AXIS_MAX,
        Z_AXIS_MIN,
        Z_AXIS_MAX
    }

    [SerializeField] GameObject _minXSlider;
    [SerializeField] GameObject _maxXSlider;

    [SerializeField] GameObject _minYSlider;
    [SerializeField] GameObject _maxYSlider;

    [SerializeField] GameObject _minZSlider;
    [SerializeField] GameObject _maxZSlider;

    PhotonView _photonView;
    public float _minXSliderValue = 0.0f;
    public float _maxXSliderValue = 1.0f;
    public float _minYSliderValue = 0.0f;
    public float _maxYSliderValue = 1.0f;
    public float _minZSliderValue = 0.0f;
    public float _maxZSliderValue = 1.0f;

    float _minXSliderValuePhysical = 0.0f;
    float _maxXSliderValuePhysical = 1.0f;
    float _minYSliderValuePhysical = 0.0f;
    float _maxYSliderValuePhysical = 1.0f;
    float _minZSliderValuePhysical = 0.0f;
    float _maxZSliderValuePhysical = 1.0f;

    float _lastMinXSliderValue = 0.0f;
    float _lastMaxXSliderValue = 1.0f;
    float _lastMinYSliderValue = 0.0f;
    float _lastMaxYSliderValue = 1.0f;
    float _lastMinZSliderValue = 0.0f;
    float _lastMaxZSliderValue = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //float minTR_XVal = Mathf.Clamp01(_minXSlider.transform.localPosition.y / 0.11f);
        //float maxTR_XVal = Mathf.Clamp01(_maxXSlider.transform.localPosition.y / 0.11f);
        //float minTR_YVal = Mathf.Clamp01(_minYSlider.transform.localPosition.y / 0.11f);
        //float maxTR_YVal = Mathf.Clamp01(_maxYSlider.transform.localPosition.y / 0.11f);
        //float minTR_ZVal = Mathf.Clamp01(_minZSlider.transform.localPosition.y / 0.11f);
        //float maxTR_ZVal = Mathf.Clamp01(_maxZSlider.transform.localPosition.y / 0.11f);
        
        float minXVal = _minXSliderValue;
        float maxXVal = _maxXSliderValue;
        float minYVal = _minYSliderValue;
        float maxYVal = _maxYSliderValue;
        float minZVal = _minZSliderValue;
        float maxZVal = _maxZSliderValue;
        
        //if (_lastMinXSliderValue != minTR_XVal) { minXVal = minTR_XVal; }
        //if (_lastMaxXSliderValue != maxTR_XVal) { maxXVal = maxTR_XVal; }
        //if (_lastMinYSliderValue != minTR_YVal) { minYVal = minTR_YVal; }
        //if (_lastMaxYSliderValue != maxTR_YVal) { maxYVal = maxTR_YVal; }
        //if (_lastMinZSliderValue != minTR_ZVal) { minZVal = minTR_ZVal; }
        //if (_lastMaxZSliderValue != maxTR_ZVal) { maxZVal = maxTR_ZVal; }
        
        if (!NearlyEqual(_lastMinXSliderValue, minXVal, 0.0001f)) { _photonView.RPC("SetSliderValueRPC", RpcTarget.All, new object[] { AxisSliders.X_AXIS_MIN, minXVal }); }
        if (!NearlyEqual(_lastMaxXSliderValue, maxXVal, 0.0001f)) { _photonView.RPC("SetSliderValueRPC", RpcTarget.All, new object[] { AxisSliders.X_AXIS_MAX, maxXVal }); }
        if (!NearlyEqual(_lastMinYSliderValue, minYVal, 0.0001f)) { _photonView.RPC("SetSliderValueRPC", RpcTarget.All, new object[] { AxisSliders.Y_AXIS_MIN, minYVal }); }
        if (!NearlyEqual(_lastMaxYSliderValue, maxYVal, 0.0001f)) { _photonView.RPC("SetSliderValueRPC", RpcTarget.All, new object[] { AxisSliders.Y_AXIS_MAX, maxYVal }); }
        if (!NearlyEqual(_lastMinZSliderValue, minZVal, 0.0001f)) { _photonView.RPC("SetSliderValueRPC", RpcTarget.All, new object[] { AxisSliders.Z_AXIS_MIN, minZVal }); }
        if (!NearlyEqual(_lastMaxZSliderValue, maxZVal, 0.0001f)) { _photonView.RPC("SetSliderValueRPC", RpcTarget.All, new object[] { AxisSliders.Z_AXIS_MAX, maxZVal }); }
    }

    public void SetSliderFromPhysical(AxisSliders slider, float value)
    {
        switch (slider)
        {
            case AxisSliders.X_AXIS_MIN:
                _minXSliderValue = value;
                _minXSliderValuePhysical = value;
                break;
            case AxisSliders.X_AXIS_MAX:
                _maxXSliderValue = value;
                _maxXSliderValuePhysical = value;
                break;
            case AxisSliders.Y_AXIS_MIN:
                _minYSliderValue = value;
                _minYSliderValuePhysical = value;
                break;
            case AxisSliders.Y_AXIS_MAX:
                _maxYSliderValue = value;
                _maxYSliderValuePhysical = value;
                break;
            case AxisSliders.Z_AXIS_MIN:
                _minZSliderValue = value;
                _minZSliderValuePhysical = value;
                break;
            case AxisSliders.Z_AXIS_MAX:
                _maxZSliderValue = value;
                _maxZSliderValuePhysical = value;
                break;
        }
    }
    

    [PunRPC]
    void SetSliderValueRPC(AxisSliders idx, float value, PhotonMessageInfo info)
    {
        Transform slider = null;
        
        switch (idx){
            case AxisSliders.X_AXIS_MIN:
                _minXSliderValue = value;
                if (_minXSliderValue != _minXSliderValuePhysical)
                {
                    _test.SetSlider(0, value);
                    _minXSliderValuePhysical = _minXSliderValue;
                }
                _lastMinXSliderValue = _minXSliderValue;
                slider = _minXSlider.transform;
                break;
            case AxisSliders.X_AXIS_MAX:
                _maxXSliderValue = value;
                if (_maxXSliderValue != _maxXSliderValuePhysical)
                {
                    _test.SetSlider(1, value);
                    _maxXSliderValuePhysical = _maxXSliderValue;
                }
                _lastMaxXSliderValue = _maxXSliderValue;
                slider = _maxXSlider.transform;
                break;
            case AxisSliders.Y_AXIS_MIN:
                _minYSliderValue = value;
                if (_minYSliderValue != _minYSliderValuePhysical)
                {
                    _test.SetSlider(2, value);
                    _minYSliderValuePhysical = _minYSliderValue;
                }
                _lastMinYSliderValue = _minYSliderValue;
                slider = _minYSlider.transform;
                break;
            case AxisSliders.Y_AXIS_MAX:
                _maxYSliderValue = value;
                if (_maxYSliderValue != _maxYSliderValuePhysical)
                {
                    _test.SetSlider(3, value);
                    _maxYSliderValuePhysical = _maxYSliderValue;
                }
                _lastMaxYSliderValue = _maxYSliderValue;
                slider = _maxYSlider.transform;
                break;
            case AxisSliders.Z_AXIS_MIN:
                _minZSliderValue = value;
                if (_minZSliderValue != _minZSliderValuePhysical)
                {
                    _test.SetSlider(4, value);
                    _minZSliderValuePhysical = _minZSliderValue;
                }
                _lastMinZSliderValue = _minZSliderValue;
                slider = _minZSlider.transform;
                break;
            case AxisSliders.Z_AXIS_MAX:
                _maxZSliderValue = value;
                if (_maxZSliderValue != _maxZSliderValuePhysical)
                {
                    _test.SetSlider(5, value);
                    _maxZSliderValuePhysical = _maxZSliderValue;
                }
                _lastMaxZSliderValue = _maxZSliderValue;
                slider = _maxZSlider.transform;
                break;
        }

        var pos = slider.transform.localPosition;
        pos.y = value * 0.11f;
        slider.transform.localPosition = pos;

        print("setting min value: " + value);
    }


}
