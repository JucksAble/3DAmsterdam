/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Netherlands3D.Events;
using Netherlands3D.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Visualisers
{
    public class MapsDataLoader : MonoBehaviour
    {
        [SerializeField]
        private StringEvent tableNameReceiveEvent;

        [SerializeField]
        private List<MapsDataTable> mapsDataTable;

        [System.Serializable]
        public struct MapsDataTable
        {
            public string name;
            public GeoJsonURLS[] geoJsonURLs;
        }
        [System.Serializable]
        public struct GeoJsonURLS
        {
            public string geoJsonURL;
            public Vector3Event drawPointEvent;
        }

        void Awake()
        {
            tableNameReceiveEvent.unityEvent.AddListener(LoadAllDataURLs);
        }

        void LoadAllDataURLs(string tableNames)
        {
            string[] tableNameValues = tableNames.Split(',');
            foreach (var tableName in tableNameValues)
            {
                var targetDataTable = mapsDataTable.Where((item) => tableName == item.name);
                if (targetDataTable.Any())
                {
                    var firstResult = targetDataTable.First();
                    foreach (var geoJsonURL in firstResult.geoJsonURLs)
                    {
                        StartCoroutine(LoadGeoJSON(geoJsonURL.geoJsonURL, geoJsonURL.drawPointEvent));
                    }
                }
            }
        }

        private IEnumerator LoadGeoJSON(string geoJsonURL, Vector3Event drawPointEvent)
        {
            var geoJsonDataRequest = UnityWebRequest.Get(geoJsonURL);
            yield return geoJsonDataRequest.SendWebRequest();

            if (geoJsonDataRequest.result == UnityWebRequest.Result.Success)
            {
                GeoJSON geoJSON = new GeoJSON(geoJsonDataRequest.downloadHandler.text);
                yield return null;

                //We already filtered the request, so we can draw all features
                while (geoJSON.GotoNextFeature())
                {
                    double[] location = geoJSON.getGeometryPoint2DDouble();
                    var unityCoordinates = ConvertCoordinates.CoordConvert.WGS84toUnity(location[0], location[1]);

                    drawPointEvent.unityEvent?.Invoke(unityCoordinates);
                }
            }
        }
    }
}