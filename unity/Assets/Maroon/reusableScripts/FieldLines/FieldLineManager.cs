﻿//-----------------------------------------------------------------------------
// FieldLineManager.cs
//
// Controller class to manage the field lines
//
//
// Authors: Michael Stefan Holly
//          Michael Schiller
//          Christopher Schinnerl
//-----------------------------------------------------------------------------
//

using System.Collections.Generic;
using Maroon.Physics.Electromagnetism;
using UnityEngine;

/// <summary>
/// Controller class to manage the field lines
/// </summary>
public class FieldLineManager : MonoBehaviour
{
    /// <summary>
    /// Lists of field lines
    /// </summary>
    protected HashSet<FieldLine> fieldLines = new HashSet<FieldLine>();

    public bool getSensed = false;

    [SerializeField]
    protected int updateRate = 1;

    private int updateCount = 0;

    /// <summary>
    /// Initialization
    /// </summary>
    protected virtual void Start()
    {
        var sensedObjects = GameObject.FindGameObjectsWithTag("FieldLine");

        foreach (var sensedTag in sensedObjects)
        {
            var parent = sensedTag.transform.parent.gameObject;
            AddFieldLine(parent.GetComponent<FieldLine>());
        }
        DrawFieldLines();
    }

    private void Update()
    {
        if (getSensed)
        {
            var sensedObjects = GameObject.FindGameObjectsWithTag("FieldLine");

            foreach (var sensedTag in sensedObjects)
            {
                var parent = sensedTag.transform.parent.gameObject;
                AddFieldLine(parent.GetComponent<FieldLine>());
            }
            Debug.Log("FieldLines: " + sensedObjects.Length);
            getSensed = false;
        }
    }

    /// <summary>
    /// Sets the field line visibility
    /// </summary>
    /// <param name="visibility">The visibility value</param>
    public void SetFieldLinesVisible(bool visibility)
    {
        foreach (var fL in fieldLines)
            fL.SetVisibility(visibility);
    }

    /// <summary>
    /// Adds the given field line to list
    /// </summary>
    /// <param name="fL">The field line</param>
    public void AddFieldLine(FieldLine fL)
    {
        fieldLines.Add(fL);
    }

    /// <summary>
    /// Removes the given field line from list
    /// </summary>
    /// <param name="fL">The field line</param>
    public void RemoveFieldLine(FieldLine fL)
    {
        fieldLines.Remove(fL);
    }

    /// <summary>
    /// This function is called every frame after all Update functions have been called and draws field lines
    /// </summary>
    public void LateUpdate()
    {
        if (SimulationController.Instance == null || !SimulationController.Instance.SimulationRunning)
            return;

        if (++updateCount % updateRate == 0)
            DrawFieldLines();
    }

    protected virtual void DrawFieldLines()
    {
        foreach (var fieldLine in fieldLines)
            fieldLine.Draw();
    }
}
