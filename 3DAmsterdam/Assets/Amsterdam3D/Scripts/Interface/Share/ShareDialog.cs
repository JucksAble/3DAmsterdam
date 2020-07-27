﻿using Amsterdam3D.Sharing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Amsterdam3D.Interface.Sharing
{
	public class ShareDialog : MonoBehaviour
	{
		public enum SharingState
		{
			SHARING_OPTIONS,
			SHARING_SCENE,
			SHOW_URL
		}

		[SerializeField]
		private RectTransform shareOptions;

		[SerializeField]
		private RectTransform progressFeedback;
		[SerializeField]
		private ProgressBar progressBar;

		[SerializeField]
		private RectTransform generatedURL;
		[SerializeField]
		private InputField generatedURLInputField;
		private string generatedURLAddress = "";

		[SerializeField]
		private SceneSerializer sceneSerializer;

		private SharingState state = SharingState.SHARING_OPTIONS;

		void OnEnable()
		{
			ChangeState(SharingState.SHARING_OPTIONS);
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		public void GenerateURL()
		{
			//Start uploading scene settings, and models to unique URL/session ID
			StartCoroutine(Share());
		}

		private IEnumerator Share()
		{
			progressBar.SetMessage("Instellingen opslaan..");
			progressBar.Percentage(0.2f);
			ChangeState(SharingState.SHARING_SCENE);
			yield return new WaitForEndOfFrame(); 

			var jsonScene = JsonUtility.ToJson(sceneSerializer.ToDataStructure(), true);
			//Post basic scene, and optionaly get unique tokens in return
			UnityWebRequest sceneSaveRequest = UnityWebRequest.Put(Constants.SHARE_URL, jsonScene);
			sceneSaveRequest.SetRequestHeader("Content-Type", "application/json");
			yield return sceneSaveRequest.SendWebRequest();
			Debug.Log(sceneSaveRequest.downloadHandler.text);
			//Check if we got some tokens for model uploads
			ServerReturn serverReturn = JsonUtility.FromJson<ServerReturn>(sceneSaveRequest.downloadHandler.text);

			if (serverReturn.modelUploadTokens.Length > 0)
			{
				progressBar.SetMessage("Objecten opslaan..");
				var currentModel = 0;
				while (currentModel < serverReturn.modelUploadTokens.Length)
				{
					progressBar.SetMessage("Objecten opslaan.. " + (currentModel + 1) + "/" + serverReturn.modelUploadTokens.Length);
					var jsonCustomObject = JsonUtility.ToJson(sceneSerializer.SerializeCustomObject(currentModel,serverReturn.sceneId, serverReturn.modelUploadTokens[currentModel].token),false);						
					Debug.Log("Trying to upload model " + currentModel);
					UnityWebRequest modelSaveRequest = UnityWebRequest.Put(Constants.SHARE_URL, jsonCustomObject);
					modelSaveRequest.SetRequestHeader("Content-Type", "application/json");
					yield return modelSaveRequest.SendWebRequest();
					Debug.Log(jsonCustomObject);
					currentModel++;

					var currentModelLoadPercentage = (float)currentModel / (float)serverReturn.modelUploadTokens.Length - 1;
					progressBar.Percentage(0.3f + (0.7f * currentModelLoadPercentage));
					yield return new WaitForSeconds(0.2f);
				}
			}
			//SERVER: Finalize and place json file
			progressBar.Percentage(1.0f);

			yield return new WaitForSeconds(0.1f);

			Debug.Log(jsonScene);

			//Temp fake random URL
			generatedURLAddress = "https://3d.amsterdam.nl/?view=" + Path.GetRandomFileName().Split('.')[0];
			generatedURLInputField.text = generatedURLAddress;

			ChangeState(SharingState.SHOW_URL);
			yield return null;
		}

		public void ChangeState(SharingState newState)
		{
			switch (newState)
			{
				case SharingState.SHARING_OPTIONS:
					shareOptions.gameObject.SetActive(true);

					progressFeedback.gameObject.SetActive(false);
					generatedURL.gameObject.SetActive(false);
					break;
				case SharingState.SHARING_SCENE:
					progressFeedback.gameObject.SetActive(true);

					shareOptions.gameObject.SetActive(false);
					generatedURL.gameObject.SetActive(false);
					break;
				case SharingState.SHOW_URL:
					generatedURL.gameObject.SetActive(true);

					shareOptions.gameObject.SetActive(false);
					progressFeedback.gameObject.SetActive(false);
					break;
				default:
					break;
			}
		}
	}
}