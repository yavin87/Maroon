﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Maroon.Physics.CoordinateSystem
{
    public class CoordSystem : MonoBehaviour
    {
        [SerializeField] private Transform origin;
        [SerializeField] private AxisController axisController;

        private Dictionary<Axis, CoordAxis> _axisDictionary;

        private static CoordSystem _instance;

        public Dictionary<Axis, CoordAxis> AxisDictionary => _axisDictionary;

        public static CoordSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<CoordSystem>();
                return _instance;
            }
        }

        private void Start()
        {
            _ = origin ?? throw new NullReferenceException();
            _ = axisController ?? throw new NullReferenceException();

            _axisDictionary = axisController.GetAxisDictionary();
        }

        public List<Unit> GetAxisUnits() => axisController.GetAxisUnits();
        public List<Unit> GetAxisSubDivisionUnits() => axisController.GetAxisSubdivisionUnits();

        public Vector3 GetPositionInAxisUnits(Vector3 objectTransform, Unit targetUnit = Unit.respective)
        {
            var systemSpaceCoordinates = WorldCoordinatesToSystemSpace(objectTransform);

            var xValue = _axisDictionary[Axis.X].GetValueFromAxisPoint(systemSpaceCoordinates.x, targetUnit);
            var yValue = _axisDictionary[Axis.Y].GetValueFromAxisPoint(systemSpaceCoordinates.y, targetUnit);
            var zValue = _axisDictionary[Axis.Z].GetValueFromAxisPoint(systemSpaceCoordinates.z, targetUnit);

            return new Vector3(xValue, yValue, zValue);
        }

        public Vector3 GetPositionInWorldSpace(Vector3 localSpacePosition, Unit[] respectiveUnits = null)
        {
            var xAxis = _axisDictionary[Axis.X];
            var yAxis = _axisDictionary[Axis.Y];
            var zAxis = _axisDictionary[Axis.Z];

            respectiveUnits = respectiveUnits ?? new[] { Unit.m, Unit.m, Unit.m };

            var xValue = xAxis.GetAxisPointFromValue(localSpacePosition.x, respectiveUnits[0]) * xAxis.AxisWorldLength;
            var yValue = yAxis.GetAxisPointFromValue(localSpacePosition.y, respectiveUnits[1]) * yAxis.AxisWorldLength;
            var zValue = zAxis.GetAxisPointFromValue(localSpacePosition.z, respectiveUnits[2]) * zAxis.AxisWorldLength;

            var localSpaceVector = new Vector3(xValue, yValue, zValue);
            return origin.TransformDirection(localSpaceVector);
        }

        public Vector3 WorldCoordinatesToSystemSpace(Vector3 objectTransform)
        {
            var axisLengths = axisController.PositiveWorldLengths;
            var objectPosition = origin.InverseTransformDirection(objectTransform);

            var objectPositionRelativeToAxisLength = new Vector3(objectPosition.x / axisLengths.x,
                objectPosition.y / axisLengths.y,
                objectPosition.z / axisLengths.z);

            return objectPositionRelativeToAxisLength;
        }
    }
}
