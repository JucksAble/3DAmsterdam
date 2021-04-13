﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using System.IO;
using SimpleJSON;
using System.Threading.Tasks;
using System.Linq;

namespace Netherlands3D.AssetGeneration.CityJSON
{
    public enum terrainType
    {
        voetpad = 0,
        fietspad = 1,
        parkeervakken = 2,
        wegen = 3,
        begroeid=4,
        erven=5,
        onbegroeid = 6,
        spoorbanen=7,
        woonerven=8,
        constructies =9,
        bruggen=10,
        water = 11,
        anders=99
    }

    public class CityObject
    {
        public List<Vector3RD> vertices;
        public terrainType type;
        public List<int> indices;
        public bool placed = false;
        public Dictionary<Vector2, List<Vector3RD>> triangleLists;
        public void GenerateTriangleLists( int tileSize)
        {
           triangleLists = new Dictionary<Vector2, List<Vector3RD>>();

            //read all the vertices
            //vertices = new List<Vector3RD>();
            //for (int i = 0; i < indices.Count; i++)
            //{
            //    vertices.Add(allvertices[indices[i]]);
            //}

            Vector2 tileId;
            //check if entire object is inside one tile
           
                if (IsBoundingBoxInSingleTile(vertices, tileSize, out tileId))
                {

                        triangleLists.Add(tileId, vertices);
                    
                }
                else
                {
                    List<Vector2> overlappingTiles = getOverlappingTiles(vertices, tileSize);
                    foreach (var item in overlappingTiles)
                    {
                    if (!triangleLists.ContainsKey(item))
                    {
                        triangleLists.Add(item, vertices);
                    }
                            
                        
                    }

                }
            
        }

        private List<Vector2> getOverlappingTiles(List<Vector3RD> bboxVertices, int tileSize)
        {
            List<Vector2> overlappingTiles = new List<Vector2>();
            // get boundingbox of total cityObject
            double Xmin = bboxVertices.Min(x => x.x);
            double Xmax = bboxVertices.Max(x => x.x);
            double Ymin = bboxVertices.Min(x => x.y);
            double Ymax = bboxVertices.Max(x => x.y);
            Vector2 tile = vertexTile(Xmin, Ymin, tileSize);
            overlappingTiles.Add(tile);
            tile = vertexTile(Xmax, Ymin, tileSize);
            if (!overlappingTiles.Contains(tile))
            {
                overlappingTiles.Add(tile);
            }
            tile = vertexTile(Xmax, Ymax, tileSize);
            if (!overlappingTiles.Contains(tile))
            {
                overlappingTiles.Add(tile);
            }
            tile = vertexTile(Xmin, Ymax, tileSize);
            if (!overlappingTiles.Contains(tile))
            {
                overlappingTiles.Add(tile);
            }
            return overlappingTiles;
        }
        public bool IsBoundingBoxInSingleTile(List<Vector3RD> bboxVertices, int tileSize, out Vector2 tileID)
        {
            
            // get boundingbox of total cityObject
            double Xmin = bboxVertices.Min(x => x.x);
            double Xmax = bboxVertices.Max(x => x.x);
            double Ymin = bboxVertices.Min(x => x.y);
            double Ymax = bboxVertices.Max(x => x.y);
            Vector2 tile = vertexTile(Xmin, Ymin, tileSize);
            tileID = tile;
            Vector2 tile2 = vertexTile(Xmax, Ymin, tileSize);
            if (tile2 == tile){ return false;}
            tile2 = vertexTile(Xmax, Ymax, tileSize);
            if (tile2 == tile) { return false; }
            tile2 = vertexTile(Xmin, Ymax, tileSize);
            if (tile2 == tile) { return false; }
            return true;
        }

        private Vector2 vertexTile(double x, double y, int tileSize)
        {
            Vector2 tileIndex = new Vector2
                (
                (float)(x - (x % tileSize)),
                (float)(y - (y % tileSize))
                ) ;
            return tileIndex;
        }


    }



    public class importTerrain : MonoBehaviour
    {
        [Header("Bounding box in RD coordinates")]
        [SerializeField]
        private Vector2 boundingBoxBottomLeft;
        [SerializeField]
        private Vector2 boundingBoxTopRight;
        private Vector3RD center = new Vector3RD();

        [Tooltip("Width and height in meters")]
        [SerializeField]
        private int tileSize = 1000; //1x1 km

        [SerializeField]
        private string geoJsonSourceFilesFolder = "D:/3DRotterdam/Terrain/cityjson";
        //private string unityMeshAssetFolder = "Assets/3DAmsterdam/GeneratedTileAssets/";

        [SerializeField]
        Netherlands3D.AssetGeneration.CityJSON.ImportCityJsonTerrain importCityjsonterrainScript;
        private TerrainFilter terrainFilter = new TerrainFilter();
        private bool bewerkingGereed = true;

        // Start is called before the first frame update
        void Start()
        {
            
            List<string> fileNames = GetFileList();
            Debug.Log(fileNames.Count + " files");

            StartCoroutine(loopthrougFiles(fileNames));
        }

        IEnumerator loopthrougFiles(List<string> fileNames)
        {
            int counter = 0;
            foreach (var item in fileNames)
            {
                counter++;
                
                yield return new WaitWhile(() => bewerkingGereed ==false);
                Debug.Log("file " + counter + " van " + fileNames.Count);
                bewerkingGereed = false;
                StartCoroutine(ReadJSONFile(item));
            }

        }

        private void moveFile(string fileName)
        {

        }
        List<string> GetFileList()
        {
            var info = new DirectoryInfo(geoJsonSourceFilesFolder);
            var fileInfo = info.GetFiles();

            int numberofFilestoRead = fileInfo.Length;
            //for testing
            //numberofFilestoRead = 1;
            List<string> filenames = new List<string>();
            for (int i = 0; i < numberofFilestoRead; i++)
            {
                filenames.Add(fileInfo[i].FullName);
            }
            return filenames;
        }

        IEnumerator ReadJSONFile(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                Debug.Log("parsing file: " + filename);
                yield return null;
                JSONNode cityModel = JSON.Parse(sr.ReadToEnd());

                Debug.Log("reading vertices");
                yield return null;
                Vector3RD[] vertices = readVertices(cityModel, center);



                //loop through cityobjects
                Debug.Log("collecting cityobjects");
                yield return null;
                CityObject[] cityObjects = GetCityObjects(cityModel["CityObjects"],vertices);

                Dictionary<Vector2, List<CityObject>> Tiles = new Dictionary<Vector2, List<CityObject>>();

                Debug.Log("sorting cityobjects("+cityObjects.Length+")");
                yield return null;
                int total = cityObjects.Count();
                int counter = 0;
                Parallel.ForEach(cityObjects, cityObject => { cityObject.GenerateTriangleLists( tileSize); });
                //foreach (var cityObject in cityObjects)
                //{
                //    counter++;
                //    if (counter % 1000 == 1)
                //    {
                //        Debug.Log("sorting cityobjects " + counter + "of" + total);
                //        yield return null;
                //    }


                //    cityObject.GenerateTriangleLists(vertices, tileSize);
                //}
                Debug.Log("collecting cityobjects");
                yield return null;
                foreach (var cityObject in cityObjects)
                {
                    Dictionary<Vector2, List<Vector3RD>> coTriangleList = new Dictionary<Vector2, List<Vector3RD>>();
                    terrainType coterraintype = cityObject.type;
                    foreach (var tile in cityObject.triangleLists)
                    {
                        if (!Tiles.ContainsKey(tile.Key))
                        {
                            Tiles.Add(tile.Key, new List<CityObject>());
                        }

                        bool found = false;
                        for (int i = 0; i < Tiles[tile.Key].Count; i++)
                        {
                            if (Tiles[tile.Key][i].type == coterraintype)
                            {
                                Tiles[tile.Key][i].vertices.AddRange(tile.Value);
                                found = true;
                                i = Tiles[tile.Key].Count + 2;
                            }
                        }
                        if (found == false)
                        {
                            CityObject newCityObject = new CityObject();
                            newCityObject.type = coterraintype;
                            newCityObject.vertices = tile.Value;
                            Tiles[tile.Key].Add(newCityObject);
                        }
                    }
                }

                Debug.Log("creating submeshes");

                yield return null;
                foreach (var tile in Tiles)
                {
                    Debug.Log("creating submeshes for tile " + tile.Key.x + "-" + tile.Key.y);
                    yield return null;
                    Dictionary<terrainType, Mesh> meshes = new Dictionary<terrainType, Mesh>();
                    foreach (var item in tile.Value)
                    {
                        Mesh mesh = importCityjsonterrainScript.CreateCityObjectMesh(item.vertices, tile.Key.x, tile.Key.y, tileSize);
                        meshes.Add(item.type, mesh);
                    }
                    importCityjsonterrainScript.CreateCombinedMeshes(meshes, tile.Key, tileSize);
                }
                cityModel = null;
            }
            bewerkingGereed = true;
        }

        Vector3RD[] readVertices(JSONNode citymodel, Vector3RD centerpoint)
        {
            //needs to be sequential
            JSONNode verticesNode = citymodel["vertices"];
            Vector3 transformScale = Vector3.one;
            Vector3RD transformTranslate = new Vector3RD(0, 0, 0);


            if (citymodel["transform"] != null)
            {
                if (citymodel["transform"]["scale"] != null)
                {
                    transformScale = new Vector3
                    (
                        citymodel["transform"]["scale"][0].AsFloat,
                        citymodel["transform"]["scale"][1].AsFloat,
                        citymodel["transform"]["scale"][2].AsFloat
                   );
                }
                if (citymodel["transform"]["translate"] != null)
                {
                    transformTranslate = new Vector3RD
                    (
                        citymodel["transform"]["translate"][0].AsDouble,
                        citymodel["transform"]["translate"][1].AsDouble,
                        citymodel["transform"]["translate"][2].AsDouble
                   );
                }
            }
            
            long vertcount = verticesNode.Count;
            Vector3RD[] vertices = new Vector3RD[vertcount];
            for (int i = 0; i < vertcount; i++)
            {
                Vector3RD vert = new Vector3RD();
                vert.x = (verticesNode[i][0].AsDouble*transformScale.x) + transformTranslate.x;
                vert.y = (verticesNode[i][1].AsDouble * transformScale.y) + transformTranslate.y;
                vert.z = (verticesNode[i][2].AsDouble * transformScale.z) + transformTranslate.z;
                vertices[i]=vert;
            }
            return vertices;
        }

        private CityObject[] GetCityObjects(JSONNode cityobjects, Vector3RD[] vertices)
        {
            CityObject[] cityObjects = new CityObject[cityobjects.Count];
            for (int i = 0; i < cityobjects.Count; i++)
            {
                CityObject cityObject = new CityObject();
                List<int> indices = ReadTriangles(cityobjects[i]);
                List<Vector3RD> coVerts = new List<Vector3RD>();
                for (int index = 0; index < indices.Count; index++)
                {
                    coVerts.Add(vertices[indices[index]]);
                }
                cityObject.vertices = coVerts;
                cityObject.type = getTerrainType(cityobjects[i]);
                cityObjects[i] = cityObject;
            }
            return cityObjects;
        }

        private terrainType getTerrainType(JSONNode cityObject)
        {

            string cityObjectType = cityObject["type"];

            if (cityObjectType == "Road")
            {
                if (terrainFilter.RoadsVoetpad.Contains(cityObject["attributes"][terrainFilter.RoadsVoetpadPropertyName]))
                {
                    return terrainType.voetpad;
                }
                if (terrainFilter.RoadsFietspad.Contains(cityObject["attributes"][terrainFilter.RoadsFietsPropertyName]))
                {
                    return terrainType.fietspad;
                }
                if (terrainFilter.RoadsParkeervak.Contains(cityObject["attributes"][terrainFilter.RoadsParkeervakPropertyName]))
                {
                    return terrainType.parkeervakken;
                }
                if (terrainFilter.RoadsSpoorbaan.Contains(cityObject["attributes"][terrainFilter.RoadsSpoorbaanPropertyName]))
                {
                    return terrainType.spoorbanen;
                }
                if (terrainFilter.RoadsWoonerf.Contains(cityObject["attributes"][terrainFilter.RoadsWoonerfPropertyName]))
                {
                    return terrainType.woonerven;
                }
                return terrainFilter.roadsOverig;
            }
            if (cityObjectType == "LandUse")
            {
                if (terrainFilter.LandUseVoetpad.Contains(cityObject["attributes"][terrainFilter.LandUseVoetpadPropertyName]))
                {
                    return terrainType.voetpad;
                }
                if (terrainFilter.LandUseRoads.Contains(cityObject["attributes"][terrainFilter.LandUseRoadsPropertyName]))
                {
                    return terrainType.wegen;
                }
                if (terrainFilter.LandUseGroen.Contains(cityObject["attributes"][terrainFilter.LandUseGroenPropertyName]))
                {
                    return terrainType.begroeid;
                }
                if (terrainFilter.LandUseErf.Contains(cityObject["attributes"][terrainFilter.LandUseErfPropertyName]))
                {
                    return terrainType.erven;
                }
                return terrainFilter.landUseOverig;
            }
            if (cityObjectType == "PlantCover")
            {
                return terrainType.begroeid;
            }
            if (cityObjectType == "GenericCityObject")
            {
                return terrainType.constructies;
            }
            if (cityObjectType == "WaterBody")
            {
                return terrainType.water;
            }
            if (cityObjectType == "Bridge")
            {
                return terrainType.bruggen;
            }

            return terrainType.anders;
        }

        private List<int> ReadTriangles(JSONNode cityObject)
        {
            List<int> triangles = new List<int>();
            JSONNode boundariesNode = cityObject["geometry"][0]["boundaries"];
            // End if no BoundariesNode
            if (boundariesNode is null)
            {
                return triangles;
            }
            foreach (JSONNode boundary in boundariesNode)
            {
                JSONNode outerRing = boundary[0];
                triangles.Add(outerRing[2].AsInt);
                triangles.Add(outerRing[1].AsInt);
                triangles.Add(outerRing[0].AsInt);
            }

            return triangles;
        }


    }
    public class TerrainFilter
    {
        public List<string> RoadsVoetpad = new List<string> { "voetpad", "voetgangersgebied", "ruiterpad", "voetpad op trap" };
        public string RoadsVoetpadPropertyName = "bgt_functie";

        public List<string> RoadsFietspad = new List<string> { "fietspad" };
        public string RoadsFietsPropertyName = "bgt_functie";

        public List<string> RoadsParkeervak = new List<string> { "parkeervlak" };
        public string RoadsParkeervakPropertyName = "bgt_functie";

        public List<string> RoadsSpoorbaan = new List<string> { "spoorbaan" };
        public string RoadsSpoorbaanPropertyName = "bgt_functie";

        public List<string> RoadsWoonerf = new List<string> { "transitie", "woonerf" };
        public string RoadsWoonerfPropertyName = "bgt_functie";
 
        public terrainType roadsOverig = terrainType.wegen;

        //                Mesh LandUseVoetpadMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "open verharding" }, true);
        public List<string> LandUseVoetpad = new List<string> { "open verharding" };
        public string LandUseVoetpadPropertyName = "bgt_fysiekvoorkomen";

        public List<string> LandUseRoads = new List<string> { "gesloten verharding" };
        public string LandUseRoadsPropertyName = "bgt_fysiekvoorkomen";

        public List<string> LandUseGroen = new List<string> { "groenvoorziening" };
        public string LandUseGroenPropertyName = "bgt_fysiekvoorkomen";

        public List<string> LandUseErf = new List<string> { "erf" };
        public string LandUseErfPropertyName = "bgt_fysiekvoorkomen";

        public terrainType landUseOverig = terrainType.onbegroeid;
    }
}

