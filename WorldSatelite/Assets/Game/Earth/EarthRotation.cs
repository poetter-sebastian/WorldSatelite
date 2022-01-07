using System;
using Game.System;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Game.Earth
{
    public class EarthRotation : Timer
    {
        public GameObject target;

        // Start is called before the first frame update
        public void Start()
        {
            //var rotation = UserInput.GetTime().Hour * EarthRotationFactor;
            //transform.RotateAround(Vector3.zero, target.transform.position, rotation);
            StartCoroutine(CheckTime());
        }

        protected override void DoWork()
        {
            transform.RotateAround(Vector3.zero, target.transform.position, EarthRotationFactor * UserInput.GetScaleValue());
        }
    }
}
