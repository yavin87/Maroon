﻿using System.Collections.Generic;
using UnityEngine;

namespace Maroon.Assessment
{
    public enum AssessmentClass
    {
        StopWatch,
        Calculator,
        Pendulum,
        Voltmeter,
        Ruler,
        Charge,
        Display,
        Slide
    }

    public class AssessmentObject : MonoBehaviour
    {
        [SerializeField]
        protected AssessmentClass classType;

        private readonly SortedList<string, AssessmentWatchValue> _watchValues = new SortedList<string, AssessmentWatchValue>();

        public AssessmentClass ClassType => classType;

        public IList<AssessmentWatchValue> WatchValues => _watchValues.Values;

        public string ObjectID { get; protected set; }

        protected virtual void Start()
        {
            ObjectID = $"{gameObject.name}{gameObject.GetInstanceID()}";

            foreach (var watchValue in GetComponents<AssessmentWatchValue>())
                _watchValues.Add(watchValue.PropertyName, watchValue);

            AssessmentManager.Instance?.RegisterAssessmentObject(this);
        }

        protected virtual void OnDestroy()
        {
            AssessmentManager.Instance?.DeregisterAssessmentObject(this);
        }

        public void OnAttributeValueChanged(string propertyName)
        {
            var watchValue = _watchValues[propertyName];
            if (watchValue.IsDynamic)
                return;

            AssessmentManager.Instance?.SendDataUpdate(ObjectID, propertyName, watchValue.GetValue());
        }
    }
}