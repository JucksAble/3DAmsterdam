﻿#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;

namespace TileBakeLibrary
{
	class Tile
	{
		public Vector2Double position = new Vector2Double(); //Bottom left (RD coordinates)
		public Vector2 size = new Vector2(); //Width and height (RD coordinates)

		public List<Vector3> vertices = new List<Vector3>();
		public List<Vector3> normals = new List<Vector3>();
		public List<Vector2> uvs = new List<Vector2>();

		public List<Submesh> submeshes = new List<Submesh>();

		public string filePath = "";
		public class Submesh
		{
			public List<int> triangleIndices = new List<int>();
			public int baseVertex = 0;
		}

		private List<SubObject> subObjects = new List<SubObject>();
		public List<SubObject> SubObjects { get => subObjects; }

		/// <summary>
		/// Add a subobject to this tile and append its geometry to the tile geometry data
		/// </summary>
		public void AppendSubObject(SubObject subObject, int targetSubMeshIndex = 0)
		{
			SwapSubObjectWithSameID(subObject);

			AddSubObject(subObject, targetSubMeshIndex);
			AppendMeshData(subObject, targetSubMeshIndex, true);
		}

		private void SwapSubObjectWithSameID(SubObject subObject)
		{
			for (int i = 0; i < SubObjects.Count; i++)
			{
				if (SubObjects[i].id == subObject.id)
				{
#if DEBUG
					Console.WriteLine($"Replacing existing subobject {subObject.id}");
#endif
					SubObjects[i] = subObject;
				}
			}
		}

		/// <summary>
		/// Add a subobject to this tile
		/// </summary>
		public void AddSubObject(SubObject subObject, int targetSubMeshIndex = 0)
		{
			CreateSubMesh(targetSubMeshIndex);
			subObjects.Add(subObject);
		}

		private void CreateSubMesh(int targetSubMeshIndex)
		{
			Submesh subMesh;
			if (submeshes.Count == 0)
			{
				subMesh = new Submesh();
				submeshes.Add(subMesh);
			}
			else if (targetSubMeshIndex > submeshes.Count - 1)
			{
				var addSubMeshes = targetSubMeshIndex - (submeshes.Count - 1);
				for (int i = 0; i < addSubMeshes; i++)
				{
					subMesh = new Submesh();
					submeshes.Add(subMesh);
				}
			}
		}

		/// <summary>
		/// Append the subobject data to the tile geometry
		/// </summary>
		/// <param name="subObject">Target SubObject data</param>
		/// <param name="targetSubMeshIndex">Submesh index</param>
		/// <param name="purgeDataAfterAppending">Clears the big arrays of data from subobject after copying to tile</param>
		private void AppendMeshData(SubObject subObject, int targetSubMeshIndex = 0 , bool purgeDataAfterAppending = true)
		{
			var indexOffset = vertices.Count;
			for (int i = 0; i < subObject.vertices.Count; i++)
			{
				var doubleVertex = subObject.vertices[i];
				//Here we convert to single precision, and switch the tile coordinate system
				var vertex = new Vector3((float)(doubleVertex.X - position.X - (size.X/2)), (float)doubleVertex.Z, (float)(doubleVertex.Y - position.Y - (size.Y / 2)));
				var normal = new Vector3(subObject.normals[i].X, subObject.normals[i].Z, subObject.normals[i].Y);
				//var uv = subObject.uvs[i];

				vertices.Add(vertex);
				normals.Add(normal);
				//uvs.Add(uv); //Uv's need to be properly generated from Poly2Mesh first
			}

			var targetSubMesh = submeshes[targetSubMeshIndex];
			for (int i = 0; i < subObject.triangleIndices.Count; i++)
			{
				targetSubMesh.triangleIndices.Add(indexOffset + subObject.triangleIndices[i]);
			}

			//To reduce the memory footprint, we can choose to remove the SubObject data after it has been baked into the tile
			if(purgeDataAfterAppending)
			{
				subObject.triangleIndices.Clear();
				subObject.vertices.Clear();
				subObject.normals.Clear();
				subObject.uvs.Clear();
			}
		}
	}
}
