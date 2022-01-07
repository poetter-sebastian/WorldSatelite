using System;
using System.Collections.Generic;
using Game.Earth;
using Game.System;
using UnityEngine;

namespace Game.Orbit
{
   public class SatelliteOrbit: Timer
   {
      public string satelliteName;
      public string tleFirst;
      public string tleSecond;

      public GameObject orbit;
      public GameObject obj;
      public bool showLine = false;

      private double epoch = 0;
      public Tle tle;
      public Eci eci;
      public Satellite sat;

      private void OnDrawGizmos()
      {
         if (!showLine) return;
         var satellite = new Satellite(tle);

         Gizmos.color = Color.red;
         var eciSat = satellite.PositionEci(0);
         Gizmos.DrawSphere(new Vector3((float)eciSat.Position.X, (float)eciSat.Position.Y, (float)eciSat.Position.Z), 150);
         Gizmos.color = Color.yellow;
         for (double i = 0; i < 93; i += 0.1)
         { 
            eciSat = satellite.PositionEci(i);
            Gizmos.DrawSphere(new Vector3((float)eciSat.Position.X, (float)eciSat.Position.Y, (float)eciSat.Position.Z), 10);
         }
      }

      public void ShowOrbit()
      {
         for (double i = 0; i < 100; i += 0.1)
         { 
            var eciSat = sat.PositionEci(i);
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = orbit.transform;
            sphere.transform.localScale = new Vector3(5, 5, 5);
            sphere.transform.position = new Vector3((float) eciSat.Position.X, (float) eciSat.Position.Y, (float) eciSat.Position.Z);
         }
         orbit.SetActive(true);
      }

      public void HideOrbit()
      {
         for (var i = 0; i < orbit.transform.childCount; i++)
         {
            Destroy(orbit.transform.GetChild(i).gameObject);
         }
         orbit.SetActive(false);
      }

      private void Start()
      {
         if (string.IsNullOrEmpty(satelliteName))
         {
            Debug.LogWarning("Satellite name not set: "+gameObject.name);
            return;
         }
         tle = new Tle(satelliteName, tleFirst, tleSecond);
         if (tle == null)
         {
            return;
         }
         sat = new Satellite(tle);
         eci = sat.PositionEci(epoch);
         obj.transform.position = new Vector3((float)eci.Position.X, (float)eci.Position.Y, (float)eci.Position.Z);

         StartCoroutine(CheckTime());
      }

      protected override void DoWork()
      {
         epoch += SatelliteSpeedFactor * UserInput.GetScaleValue();
         eci = sat.PositionEci(epoch);
         var pos = eci.Position;
         obj.transform.position = new Vector3((float) pos.X, (float) pos.Y, (float) pos.Z);
         obj.transform.LookAt(Vector3.zero);
      }
   }
}