﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorPicker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	[SerializeField]
	private RectTransform dragDropRegion;

	[SerializeField]
	private Image pointer;

	private Rect dragRegionRectangle;

	public RawImage colorPalette;
	public Color pickedColor;

	public Material[] targetMaterials;
	public Image[] targetImages;

	[SerializeField]
	private bool radialConstraint = false;

	private float intensity = 1.0f;

	public void OnPointerClick(PointerEventData eventData) => OnDrag(eventData);
	public void OnBeginDrag(PointerEventData eventData) => OnDrag(eventData);
	public void OnEndDrag(PointerEventData eventData) => OnDrag(eventData);
	public void OnDrag(PointerEventData eventData = null)
	{
		MovePointer();
		PickColorFromPalette();
	}

	void Start()
	{
		PickColorFromPalette();
	}

	public void SetColorIntensity(float intensityValue){
		intensity = intensityValue;
		PickColorFromPalette();
		colorPalette.color = Color.Lerp(Color.black, Color.white, intensity);
	}

	void MovePointer()
	{
		dragRegionRectangle = RectTransformToScreenSpace(dragDropRegion);

		var newPosition = new Vector2(
			Mathf.Max(Mathf.Min(dragRegionRectangle.max.x, Input.mousePosition.x), dragRegionRectangle.min.x),
			Mathf.Max(Mathf.Min(dragRegionRectangle.max.y, Input.mousePosition.y), dragRegionRectangle.min.y)
		);

		if (radialConstraint)
		{
			var radius = dragRegionRectangle.width * 0.5f;
			var distanceFromCenter = Vector2.Distance(dragRegionRectangle.center, newPosition);
			newPosition = Vector2.Lerp(dragRegionRectangle.center,newPosition,(radius / distanceFromCenter));
		}

		pointer.rectTransform.position = newPosition;
	}
	public void PickColorFromPalette()
	{
		//Lets inverse transform point so we can scale stuff as well
		Vector3 inverseTransform = this.colorPalette.rectTransform.InverseTransformPoint(pointer.rectTransform.position);
		var colorPalette = (Texture2D)this.colorPalette.texture;
		int W = colorPalette.width;
		int H = colorPalette.height;

		var paletteRectangle = this.colorPalette.rectTransform.rect;
		inverseTransform.x -= paletteRectangle.x;
		inverseTransform.y -= paletteRectangle.y;
		inverseTransform.x /= paletteRectangle.width;
		inverseTransform.y /= paletteRectangle.height;

		//Grab the raw texture pixel at the picker coordinates
		pickedColor = colorPalette.GetPixel((int)(inverseTransform.x * W), (int)(inverseTransform.y * H)) * intensity;
		pointer.color = pickedColor;

		//Apply color to selected object/material
		foreach (var material in targetMaterials)
		{
			material.color = pickedColor;
		}
	}

	public Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - (size * 0.5f), size);
	}
}
