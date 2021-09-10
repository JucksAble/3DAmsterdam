using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text.RegularExpressions;
using ConvertCoordinates;
using Netherlands3D;

public class AutoImportOBJIntoScene : AssetPostprocessor
{
    private const string autoFolder = "ImportIntoScene";
    private static bool continueImport = false;

    private static string currentProcessingAssetPath = "";

    void OnPreprocessModel()
    {
        //Make sure if our preprocessor changes the asset, it is not imported again
        if(currentProcessingAssetPath == assetPath)
        {
            return;
        }

        currentProcessingAssetPath = assetPath;
        if (assetPath.Contains(autoFolder))
        {
            if (continueImport || EditorUtility.DisplayDialog(
                "Auto import OBJ's into scene",
                "These models will be automatically placed into the current scene. Would you like to proceed?",
                "Proceed",
                "Cancel"
            ))
            {
                if(assetPath.Contains(".obj"))
                    CorrectOBJToSceneUnits(assetPath);

                continueImport = true;
                ModelImporter modelImporter = assetImporter as ModelImporter;
                modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            }
        }
    }

    private void CorrectOBJToSceneUnits(string filePath)
    {
        //Load up our application config set in our scene
        Config.activeConfiguration = EditorSceneManager.GetActiveScene().GetRootGameObjects()[0].GetComponent<ApplicationConfiguration>().ConfigurationFile;

        //Replace line vertex positions with corrected ones
        string[] objLines = File.ReadAllLines(filePath);
        for (int i = 0; i < objLines.Length; i++)
        {
            var lineWithSingleSpaces = Regex.Replace(objLines[i], @"\s+", " ");
            if (lineWithSingleSpaces.Contains("v "))
            {
                string[] lineParts = lineWithSingleSpaces.Split(' ');

                Vector3RD doubleCoordinate = new Vector3RD(
                    double.Parse(lineParts[1]),
                    double.Parse(lineParts[2]),
                    double.Parse(lineParts[3])
                );
                Vector3 unityCoordinate = CoordConvert.RDtoUnity(doubleCoordinate);

                var replacedLine = $"v {unityCoordinate.x} {unityCoordinate.y} {unityCoordinate.z}";
                objLines[i] = replacedLine;
            }
            else if (lineWithSingleSpaces.Contains("v "))
            {
                //Unity OBJ importer only splits up groups as seperate objects. So make sure our objects are groups.
                objLines[i] = objLines[i].Replace("o ", "g ");
            }
        }
        Debug.Log("Corrected OBJ file vert positions");

        File.WriteAllLines(filePath,objLines);
    }

    void OnPostprocessModel(GameObject gameObject)
    {
        if (!continueImport) return;

        Debug.Log("Placed object into scene");
        PrefabUtility.InstantiatePrefab(gameObject,EditorSceneManager.GetActiveScene());
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (!continueImport) return;

        foreach (string assetPath in importedAssets)
        {
            if (assetPath.Contains(autoFolder))
            {
                var importedObject = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
                PrefabUtility.InstantiatePrefab(importedObject, EditorSceneManager.GetActiveScene());
            }
        }
    }
}