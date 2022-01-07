using System.Text.RegularExpressions;
using Game.Orbit;
using UnityEngine;

namespace Game.System
{
    public class SatelliteSystem : MonoBehaviour
    {
        public GameObject satPrefab;
        public TextAsset file = null;

        // Start is called before the first frame update
        private void Start()
        {
            var fs = file.text;
            var fLines = Regex.Split(fs, "\n|\r|\r\n");

            for (var i = 0; i < fLines.Length-1; i += 6)
            {
                var valueLine = fLines[i];
                if(valueLine == null)
                    continue;
                var pos = transform;
                var newSat = Instantiate(satPrefab, Vector3.zero, pos.rotation, pos).GetComponent<SatelliteOrbit>();
                newSat.satelliteName = fLines[i];
                newSat.name = fLines[i];
                newSat.tleFirst = fLines[i+2];
                newSat.tleSecond = fLines[i+4];
            }
        }

        // Update is called once per frame
        private void Update()
        {
            
        }
    }
}
