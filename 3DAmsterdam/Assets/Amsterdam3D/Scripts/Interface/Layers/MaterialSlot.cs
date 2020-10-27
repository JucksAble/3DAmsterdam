﻿using Amsterdam3D.JavascriptConnection;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class MaterialSlot : ChangePointerStyleHandler, IPointerClickHandler
	{
		[SerializeField]
		private Material transparentMaterialSource;
		[SerializeField]
		private Material opaqueMaterialSource;

		private Color resetMaterialColor;

		private Material targetMaterial;
		private LayerVisuals layerVisuals;

		private bool selected = false;

		public float materialOpacity = 1.0f;

		private const string EXPLANATION_TEXT = "\nShift+Klik: Multi-select";

		public bool Selected
		{
			get
			{
				return selected;
			}
			set
			{
				selected = value;
				selectedImage.enabled = selected;
			}
		}

		[SerializeField]
		private Image selectedImage;
		[SerializeField]
		private Image colorImage;
		public Color GetMaterialColor => targetMaterial.GetColor("_BaseColor");

		private void Start()
		{
			Selected = selected; //start unselected
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Select();
		}

		/// <summary>
		/// Sets the material target and reference the target LayerVisuals where this slot is in.
		/// </summary>
		/// <param name="target">The Material this slot targets</param>
		/// <param name="targetLayerVisuals">The target LayerVisuals where this slot is in</param>
		public void Init(Material target, Color resetColor, LayerVisuals targetLayerVisuals, Material transparentMaterialSourceOverride = null, Material opaqueMaterialSourceOverride = null)
		{
			targetMaterial = target;

			//Optional non standard shader type overrides ( for layers with custom shaders )
			if(transparentMaterialSourceOverride)
				transparentMaterialSource = transparentMaterialSourceOverride;
			if(opaqueMaterialSourceOverride)
				opaqueMaterialSource = opaqueMaterialSourceOverride;

			//Set tooltip text. Users do not need to know if a material is an instance.
			var materialName = targetMaterial.name.Replace(" (Instance)", "");
			GetComponent<TooltipTrigger>().TooltipText = materialName + EXPLANATION_TEXT;

			var materialColor = GetMaterialColor;
			colorImage.color = new Color(materialColor.r, materialColor.g, materialColor.b, 1.0f);
			materialOpacity = materialColor.a;

			resetMaterialColor = resetColor;

			layerVisuals = targetLayerVisuals;
		}

		/// <summary>
		/// User (multi)selection of the material slot
		/// </summary>
		private void Select()
		{
			var multiSelect = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			Selected = (multiSelect) ? !Selected : true;

			layerVisuals.SelectMaterialSlot(this, multiSelect);

			Debug.Log("Selected material " + targetMaterial.name);
		}

		/// <summary>
		/// Reset the material color back to what it was at initialisation.
		/// </summary>
		public void ResetColor()
		{
			ChangeColor(resetMaterialColor);
		}

		/// <summary>
		/// Changes the color of the Material that is linked to this slot
		/// </summary>
		/// <param name="pickedColor">The new color for the linked Material</param>
		public void ChangeColor(Color pickedColor)
		{
			colorImage.color = pickedColor;
			targetMaterial.SetColor("_BaseColor", new Color(pickedColor.r, pickedColor.g, pickedColor.b, materialOpacity));
		}

		/// <summary>
		/// Changes the opacity of the material, and always swap the shader type to the faster Opaque surface when opacity is 1.
		/// </summary>
		/// <param name="opacity">Opacity value from 0.0 to 1.0</param>
		public void ChangeOpacity(float opacity)
		{
			if(materialOpacity == opacity)
			{
				return;
			}
			else{
				materialOpacity = opacity;
				SwitchShaderAccordingToOpacity();
			}
		}

		private void SwitchShaderAccordingToOpacity()
		{
			if (materialOpacity < 1.0f)
			{
				SwapShaderToTransparent();
			}
			else
			{
				SwapShaderToOpaque();
			}
		}

		private void SwapShaderToOpaque()
		{
			targetMaterial.CopyPropertiesFromMaterial(opaqueMaterialSource);
			targetMaterial.SetFloat("_Surface", 0); //0 Opaque
			targetMaterial.SetColor("_BaseColor", colorImage.color);

			targetMaterial.shader = opaqueMaterialSource.shader;
		}

		private void SwapShaderToTransparent()
		{
			targetMaterial.CopyPropertiesFromMaterial(transparentMaterialSource);
			var color = colorImage.color;
			color.a = materialOpacity;
			targetMaterial.SetFloat("_Surface", 1); //1 Alpha
			targetMaterial.SetColor("_BaseColor", color);

			targetMaterial.shader = transparentMaterialSource.shader;
		}
	}
}