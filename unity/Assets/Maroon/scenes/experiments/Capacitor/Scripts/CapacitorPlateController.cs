﻿using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CapacitorPlateController : VRTK_InteractableObject, IGenerateE, IResetObject
{
    private GameObject resizeHeightObject1;
    private GameObject resizeHeightObject2;
    private GameObject resizeWidthObject1;
    private GameObject resizeWidthObject2;

    [SerializeField]
    private float resizeObjectSize = 0.02f;

    [SerializeField]
    private float maxHeightSize = 0.5f;

    [SerializeField]
    private float maxWidthSize = 1;

    [SerializeField]
    private bool isNegativePlate = false;

    private Capacitor capacitor;

    private Dictionary<Tuple<float, float, float>, Vector3> eFieldCalculations = new Dictionary<Tuple<float, float, float>, Vector3>();

    private Vector3 oldPosition;

    private Vector3 oldLocalScale;

    private Vector3 startScale;

    public bool Enabled
    {
        get => enabled;
        set => enabled = value;
    }

    private void Start()
    {
        startScale = transform.localScale;

        oldPosition = transform.position;
        oldLocalScale = transform.localScale;

        capacitor =  GetComponentInParent<Capacitor>();

        resizeWidthObject1 = CreateResizeObject(Vector3.right, maxWidthSize);
        resizeWidthObject2 = CreateResizeObject(Vector3.right, maxWidthSize);
        resizeHeightObject1 = CreateResizeObject(Vector3.up, maxHeightSize);
        resizeHeightObject2 = CreateResizeObject(Vector3.up, maxHeightSize);

        EnableResizeObjects(false);

        disableWhenIdle = false;
    }

    protected override void Update()
    {
        base.Update();

        if (oldPosition != transform.position || oldLocalScale != transform.localScale)
        {
            eFieldCalculations.Clear();

            oldPosition = transform.position;
            oldLocalScale = transform.localScale;
        }
           

        Vector3 size = GetComponent<Renderer>().bounds.size;
        Vector3 offset_x = new Vector3(size.x / 2, 0, 0);
        Vector3 offset_y = new Vector3(0, size.y / 2, 0);

        if (resizeWidthObject1 != null)
            resizeWidthObject1.transform.position = transform.position + offset_x;
        if(resizeWidthObject2 != null)
            resizeWidthObject2.transform.position = transform.position - offset_x;
        if (resizeHeightObject1 != null)
            resizeHeightObject1.transform.position = transform.position + offset_y;
        if (resizeHeightObject2 != null)
            resizeHeightObject2.transform.position = transform.position - offset_y;
    }

    private GameObject CreateResizeObject(Vector3 resizeAxis, float maxSize)
    {
        GameObject resizeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        resizeObject.GetComponent<SphereCollider>().radius = 2;
        resizeObject.transform.localScale = new Vector3(resizeObjectSize, resizeObjectSize, resizeObjectSize);
        resizeObject.GetComponent<Renderer>().material.color = Color.black;

        CapacitorPlateResizeController resizeController = resizeObject.AddComponent<CapacitorPlateResizeController>();
        resizeController.isUsable = true;
        resizeController.setCapacitorPlate(this);
        resizeController.setResizeAxsis(resizeAxis);
        resizeController.setMaxSize(maxSize);

        return resizeObject;
    }

    public void EnableResizeObjects(bool value)
    {
        resizeWidthObject1.GetComponent<Renderer>().enabled = value;
        resizeWidthObject2.GetComponent<Renderer>().enabled = value;
        resizeHeightObject1.GetComponent<Renderer>().enabled = value;
        resizeHeightObject2.GetComponent<Renderer>().enabled = value;
    }

    public override void StartTouching(VRTK_InteractTouch currentTouchingObject = null)
    {
        base.StartTouching(currentTouchingObject);

        EnableResizeObjects(true);
    }

    public override void StopTouching(VRTK_InteractTouch previousTouchingObject = null)
    {
        base.StopTouching(previousTouchingObject);

        EnableResizeObjects(false);
    }

    public float GetChargeValue()
    {
        float chargeValue = capacitor.GetChargeValue() / 2;
        if (isNegativePlate)
            chargeValue *= -1;

        return chargeValue; 
    }

    public Vector3 getE(Vector3 position)
    {
        Vector3 eField = Vector3.zero;
        Vector3 size = GetComponent<Renderer>().bounds.size;
        Vector3 offset = new Vector3(-size.x / 2, -size.y / 2, 0);

        float dw = 0.08f;
        float dh = 0.08f;
        float dq = (GetChargeValue() / (size.x * size.y)) * dw * dh;

        Tuple<float, float, float> tPosition = new Tuple<float, float, float>(position.x, position.y, position.z);
        if (eFieldCalculations.ContainsKey(tPosition))
            return dq * eFieldCalculations[tPosition];
      
        for(int i = 0; i <= (int)(size.x / dw); i++)
        {
            for(int j = 0;  j <= (int)(size.y / dh); j++)
            {
                Vector3 s = transform.position + offset;
                s.x += i * dw;
                s.y += j * dh;

                Vector3 direction = position - s;
                float distance = Vector3.Distance(s, position);

                eField += direction / (4 * Mathf.PI * 8.8542e-12f * Mathf.Pow(distance, 3));
            }
        }
        eFieldCalculations.Add(tPosition, eField);

        return dq * eField;
    }

    public float getEFlux(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    public float getEPotential(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    public void ResetObject()
    {
        transform.localScale = startScale;
    }

    public float getFieldStrength()
    {
        return capacitor.GetElectricalFieldStrength();
    }
}
