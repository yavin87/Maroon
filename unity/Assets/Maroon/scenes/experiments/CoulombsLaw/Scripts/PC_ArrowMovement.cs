﻿using System;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;

public class PC_ArrowMovement : MonoBehaviour, IResetWholeObject
{
    [Header("General Input Objects")]
    [Tooltip("The object that will be moved with the arrows. If null then the object with the script on it will be moved.")]
    public GameObject movingObject = null;
    public bool useMovementOffset = true;

    [Header("Restrictions")]
    public bool restrictXMovement = false;
    public bool restrictYMovement = false;
    public bool restrictZMovement = false;

    public Transform minimumBoundary = null;
    public Transform maximumBoundary = null;
    
    [Header("Visualization Specific Properties")] 
    [Tooltip("When clicking on an arrow, one can see how far the object can be moved. This only worked when Minimum and Maximum Boundary are set.")]
    public bool showMovingLines = false;
    [Tooltip("Hides the arrows when the simulation is running.")]
    public bool hideWhileInRunMode = true;
    
    [Header("Reset Settings")] 
    public bool resetOnReset = false;
    public bool resetOnWholeReset = false;

    public UnityEvent OnMovementStart;
    public UnityEvent OnMove;
    public UnityEvent OnMovementFinish;
    
    private Vector3 _localMinBoundary;
    private Vector3 _localMaxBoundary;
    
    private Vector3 _movingDirection =  Vector3.zero;
    private bool _moving = false;
    private Vector3 _movingOffset = Vector3.zero;
    private float _distance;
    
    private LineRenderer _lineRenderer;
    private bool _lastUpdateInRunMode = false;

    private Vector3 _originalPosition;
    
    public GameObject _arrowXPositive;
    public GameObject _arrowXNegative;
    public GameObject _arrowYPositive;
    public GameObject _arrowYNegative;
    public GameObject _arrowZPositive;
    public GameObject _arrowZNegative;

    // Start is called before the first frame update
    private void Start()
    {
        _lastUpdateInRunMode = SimulationController.Instance.SimulationRunning;
        
        //Check if we have a moving object or we should just move our own transform
        if (movingObject == null) movingObject = gameObject;

        if (maximumBoundary != null && minimumBoundary != null)
        {
            //Create a linerenderer if none exists and we need one
            if (showMovingLines)
            {
                _lineRenderer = GetComponent<LineRenderer>();
//                if (!_lineRenderer) _lineRenderer = gameObject.AddComponent<LineRenderer>();
                if(_lineRenderer != null) _lineRenderer.enabled = false;
            }
            
            _localMinBoundary = movingObject.transform.parent.transform.InverseTransformPoint(minimumBoundary.position);
            _localMaxBoundary = movingObject.transform.parent.transform.InverseTransformPoint(maximumBoundary.position);
        }
        
        //Needed for resetting later
        _originalPosition = movingObject.transform.position;
        UpdateMovementRestriction();
    }

    // Update is called once per frame
    private void Update()
    {
        if (SimulationController.Instance.SimulationRunning == _lastUpdateInRunMode) return;
        _lastUpdateInRunMode = SimulationController.Instance.SimulationRunning;
        ChangeRunMode();
    }

    public void UpdateMovementRestriction(bool forbidXMovement, bool forbidYMovement, bool forbidZMovement)
    {
        restrictXMovement = forbidXMovement;
        restrictYMovement = forbidYMovement;
        restrictZMovement = forbidZMovement;
        UpdateMovementRestriction();
    }

    private void UpdateMovementRestriction()
    {
        //Movement Restrictions -> hide arrows that are not allowed to move
        if(_arrowXPositive) _arrowXPositive.SetActive(!restrictXMovement); 
        if(_arrowXNegative) _arrowXNegative.SetActive(!restrictXMovement);
        if(_arrowYPositive) _arrowYPositive.SetActive(!restrictYMovement);
        if(_arrowYNegative) _arrowYNegative.SetActive(!restrictYMovement);
        if(_arrowZPositive) _arrowZPositive.SetActive(!restrictZMovement);
        if(_arrowZNegative) _arrowZNegative.SetActive(!restrictZMovement);
    }
    
    private void ChangeRunMode()
    {
        _arrowXPositive.SetActive((!SimulationController.Instance.SimulationRunning || !hideWhileInRunMode) && !restrictXMovement); 
        _arrowXNegative.SetActive((!SimulationController.Instance.SimulationRunning || !hideWhileInRunMode) && !restrictXMovement);
        _arrowYPositive.SetActive((!SimulationController.Instance.SimulationRunning || !hideWhileInRunMode) && !restrictYMovement);
        _arrowYNegative.SetActive((!SimulationController.Instance.SimulationRunning || !hideWhileInRunMode) && !restrictYMovement);
        _arrowZPositive.SetActive((!SimulationController.Instance.SimulationRunning || !hideWhileInRunMode) && !restrictZMovement);
        _arrowZNegative.SetActive((!SimulationController.Instance.SimulationRunning || !hideWhileInRunMode) && !restrictZMovement);
        
        _arrowXPositive.GetComponent<Collider>().enabled = _arrowXNegative.GetComponent<Collider>().enabled = 
            _arrowYPositive.GetComponent<Collider>().enabled = _arrowYNegative.GetComponent<Collider>().enabled = 
                _arrowZPositive.GetComponent<Collider>().enabled = _arrowZNegative.GetComponent<Collider>().enabled = 
                    !SimulationController.Instance.SimulationRunning || hideWhileInRunMode && _arrowXPositive.GetComponent<Collider>().enabled;
    }
    
    public void OnChildMouseDown(GameObject child)
    {
        if (_moving) return;
        
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (!Physics.Raycast(ray, out hitInfo) || !Input.GetMouseButtonDown(0)) return;
        
        var hitCollider = hitInfo.collider;
        if ((hitCollider == _arrowXPositive.GetComponent<Collider>() || _arrowXNegative && hitCollider == _arrowXNegative.GetComponent<Collider>()) && !restrictXMovement)
            _movingDirection = Vector3.right;
        else if ((hitCollider == _arrowYPositive.GetComponent<Collider>() || _arrowYNegative && hitCollider == _arrowYNegative.GetComponent<Collider>()) && !restrictYMovement)
            _movingDirection = Vector3.up;
        else if ((hitCollider == _arrowZPositive.GetComponent<Collider>() || _arrowZNegative && hitCollider == _arrowZNegative.GetComponent<Collider>()) && !restrictZMovement)
            _movingDirection = Vector3.forward;
        else return;
        
        DrawMovingLines(_movingDirection);
        
        _moving = true;
        _distance = Vector3.Distance(movingObject.transform.position, Camera.main.transform.position);
        var pt = movingObject.transform.parent.transform.InverseTransformPoint(ray.GetPoint(_distance));
//        var pt = movingObject.transform.InverseTransformPoint(ray.GetPoint(_distance));
        
        if(useMovementOffset)
            _movingOffset = pt - movingObject.transform.localPosition;
        
        OnMovementStart.Invoke();
    }

    private void DrawMovingLines(Vector3 drawingMask)
    {
        if(_lineRenderer == null && maximumBoundary != null && minimumBoundary != null && showMovingLines)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (!_lineRenderer) return;
            _lineRenderer.enabled = false;
        }
        
        if (!showMovingLines || maximumBoundary == null || minimumBoundary == null || _lineRenderer == null) return;
        var lineStart = movingObject.transform.localPosition; 
        var lineEnd = movingObject.transform.localPosition;

        if (Math.Abs(drawingMask.x) > 0.0001)
        {
            lineStart.x = Mathf.Min(_localMinBoundary.x, _localMaxBoundary.x);
            lineEnd.x = Mathf.Max(_localMinBoundary.x, _localMaxBoundary.x); 
        }
        if (Math.Abs(drawingMask.y) > 0.0001)
        {
            lineStart.y = Mathf.Min(_localMinBoundary.y, _localMaxBoundary.y);
            lineEnd.y = Mathf.Max(_localMinBoundary.y, _localMaxBoundary.y); 
        }
        if (Math.Abs(drawingMask.z) > 0.0001)
        {
            lineStart.z = Mathf.Min(_localMinBoundary.z, _localMaxBoundary.z);
            lineEnd.z = Mathf.Max(_localMinBoundary.z, _localMaxBoundary.z); 
        }
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPositions(new Vector3[]
        {
            movingObject.transform.parent.transform.TransformPoint(lineStart), 
            movingObject.transform.parent.transform.TransformPoint(lineEnd)
        });
        _lineRenderer.enabled = true;
    }

    public void OnChildMouseDrag()
    {
        if (!_moving) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var pt = movingObject.transform.parent.transform.InverseTransformPoint(ray.GetPoint(_distance));
        var pos = movingObject.transform.localPosition;
        pt -= _movingOffset;

        if (Math.Abs(_movingDirection.x) < 0.001) pt.x = pos.x;
        if (Math.Abs(_movingDirection.y) < 0.001) pt.y = pos.y;
        if (Math.Abs(_movingDirection.z) < 0.001) pt.z = pos.z;
        
        if (minimumBoundary != null && maximumBoundary != null)
        {
            if (Math.Abs(_movingDirection.x) > 0.001)
                pt.x = Mathf.Clamp(pt.x, _localMinBoundary.x, _localMaxBoundary.x);
            if (Math.Abs(_movingDirection.y) > 0.001)
                pt.y = Mathf.Clamp(pt.y, Mathf.Min(_localMinBoundary.y, _localMaxBoundary.y), 
                    Mathf.Max(_localMinBoundary.y, _localMaxBoundary.y));
            if (Math.Abs(_movingDirection.z) > 0.001) 
                pt.z = Mathf.Clamp(pt.z, _localMinBoundary.z, _localMaxBoundary.z);
        }

        movingObject.transform.localPosition =  pt;
        OnMove.Invoke();
    }

    public void OnChildMouseUp()
    {
        if (!_moving) return;
        _moving = false;
        _movingDirection = Vector3.zero;
        if (_lineRenderer && _lineRenderer.enabled) _lineRenderer.enabled = false;
        
        OnMovementFinish.Invoke();
    }

    public void SetBoundaries(Transform min, Transform max)
    {
        minimumBoundary = min;
        maximumBoundary = max;
        _localMinBoundary = movingObject.transform.parent.transform.InverseTransformPoint(minimumBoundary.position);
        _localMaxBoundary = movingObject.transform.parent.transform.InverseTransformPoint(maximumBoundary.position);
    }
    
    public void ResetObject()
    {
        if (resetOnReset) movingObject.transform.position = _originalPosition;
    }

    public void ResetWholeObject()
    {
        if (resetOnWholeReset) movingObject.transform.position = _originalPosition;
    }
}
