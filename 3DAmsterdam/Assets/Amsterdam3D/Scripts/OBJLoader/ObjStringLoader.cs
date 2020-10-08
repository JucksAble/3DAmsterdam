﻿using Amsterdam3D.CameraMotion;
using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Amsterdam3D.JavascriptConnection;
using UnityEngine.Events;

namespace Amsterdam3D.UserLayers
{
	public class ObjStringLoader : MonoBehaviour
	{
		[SerializeField]
		private Material defaultLoadedObjectsMaterial;

		[SerializeField]
		private LoadingScreen loadingObjScreen;

		[SerializeField]
		private UnityEvent doneLoadingModel;

		[SerializeField]
		private PlaceCustomObject customObjectPlacer;

		private string objModelName = "model";

		[SerializeField]
		private int maxLinesPerFrame = 200000; //20000 obj lines are close to a 4mb obj file

		private void Start()
		{
			loadingObjScreen.Hide();
		}

#if UNITY_EDITOR
		private void Update()
		{
			//Only used for in editor testing
			if (Input.GetKeyDown(KeyCode.L))
				StartCoroutine(ParseOBJFromString(
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/Source/KRZNoord_OBJ/Testgebied_3DAmsterdam.obj"),
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/Source/KRZNoord_OBJ/Testgebied_3DAmsterdam.mtl")
				));
			if (Input.GetKeyDown(KeyCode.K))
				StartCoroutine(ParseOBJFromString(
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/Source/SketchUp_OBJexport_triangulated/25052020 MV 3D Model Marineterrein.obj"),
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/Source/SketchUp_OBJexport_triangulated/25052020 MV 3D Model Marineterrein.mtl")
				));
			if (Input.GetKeyDown(KeyCode.H))
				StartCoroutine(ParseOBJFromString(
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/Source/suzanne.obj"),
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/Source/suzanne.mtl")
				));
			if (Input.GetKeyDown(KeyCode.J))
				StartCoroutine(ParseOBJFromString(
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/Source/suzanne.obj"),
				""
				));
		}
#endif
		public void SetOBJFileName(string fileName)
		{
			objModelName = Path.GetFileNameWithoutExtension(fileName);
			loadingObjScreen.ShowMessage("Model wordt geladen: " + objModelName);
			loadingObjScreen.ProgressBar.SetMessage("1%");
			loadingObjScreen.ProgressBar.Percentage(1);
		}
		public void LoadOBJFromJavascript()
		{
			StartCoroutine(ParseOBJFromString(JavascriptMethodCaller.FetchOBJDataAsString(), JavascriptMethodCaller.FetchMTLDataAsString()));
		}
		private IEnumerator ParseOBJFromString(string objText, string mtlText = "")
		{
			//Display loading message covering entire screen
			yield return new WaitForEndOfFrame();
			yield return new WaitForSeconds(0.1f);

			//Create a new gameobject that parses OBJ lines one by one
			var newOBJLoader = new GameObject().AddComponent<ObjLoad>();
			float remainingLinesToParse;
			float totalLines;
			float percentage; 

			//Parse the obj line by line
			newOBJLoader.SetGeometryData(ref objText);
			loadingObjScreen.ShowMessage("Objecten worden geladen...");
			remainingLinesToParse = newOBJLoader.ParseNextObjLines(1);
			totalLines = remainingLinesToParse;
			while (remainingLinesToParse > 0)
			{
				remainingLinesToParse = newOBJLoader.ParseNextObjLines(maxLinesPerFrame);
				percentage = 1.0f - (remainingLinesToParse / totalLines);
				loadingObjScreen.ProgressBar.SetMessage(Mathf.Round(percentage * 100.0f) + "%");
				loadingObjScreen.ProgressBar.Percentage(percentage);
				yield return null;
			}

			//Parse the mtl file, filling our material library
			if (mtlText != "")
			{
				newOBJLoader.SetMaterialData(ref mtlText);
				remainingLinesToParse = newOBJLoader.ParseNextMtlLines(1);
				totalLines = remainingLinesToParse;

				loadingObjScreen.ShowMessage("Materialen worden geladen...");
				while (remainingLinesToParse > 0)
				{
					remainingLinesToParse = newOBJLoader.ParseNextMtlLines(maxLinesPerFrame);
					percentage = 1.0f - (remainingLinesToParse / totalLines);
					//loadingObjScreen.ProgressBar.SetMessage(Mathf.Round(percentage * 100.0f) + "%");
					loadingObjScreen.ProgressBar.Percentage(percentage);
					yield return null;
				}
			}

			newOBJLoader.Build(defaultLoadedObjectsMaterial);
			
			//Make interactable
			newOBJLoader.transform.Rotate(0, 90, 0);
			newOBJLoader.transform.localScale = new Vector3(1.0f, 1.0f, -1.0f);
;			newOBJLoader.name = objModelName;
			newOBJLoader.gameObject.AddComponent<Draggable>();
			newOBJLoader.gameObject.AddComponent<MeshCollider>().sharedMesh = newOBJLoader.GetComponent<MeshFilter>().sharedMesh;

			customObjectPlacer.PlaceExistingObjectAtPointer(newOBJLoader.gameObject);

			//hide panel and loading screen after loading
			loadingObjScreen.Hide();

			//Invoke done event
			doneLoadingModel.Invoke();

			//Remove this loader from finished object
			Destroy(newOBJLoader);
		}
	}
}