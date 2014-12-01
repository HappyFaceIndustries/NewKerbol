using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	public static class Utils
	{
		//misc.
		public static CelestialBody GetCelestialBody(string name)
		{
			foreach (var body in FlightGlobals.Bodies)
			{
				if (body.bodyName == name)
					return body;
			}
			return null;
		}
		public static CelestialBody GetCelestialBody(int id)
		{
			foreach (var body in FlightGlobals.Bodies)
			{
				if (body.flightGlobalsIndex == id)
					return body;
			}
			return null;
		}
		//from DRE, used to do flightlogs
		public static string FormatTime(double time)
		{
			int iTime = (int) time % 3600;
			int seconds = iTime % 60;
			int minutes = (iTime / 60) % 60;
			int hours = (iTime / 3600);
			return hours.ToString ("D2") 
				+ ":" + minutes.ToString ("D2") + ":" + seconds.ToString ("D2");
		}

		//loading/saving utils
		public static Shader LoadShader(string path)
		{
			string data = System.IO.File.ReadAllText (KSPUtil.ApplicationRootPath + "GameData/NewKerbol/PluginData/" + path);
			return new Material (data).shader;
		}
		public static AudioClip LoadAudio(string path)
		{
			return GameDatabase.Instance.GetAudioClip ("NewKerbol/Audio/" + path);
		}
		public static Texture2D LoadTexture(string path, TextureFormat format = TextureFormat.ARGB32, bool mipmaps = true)
		{
			var data = System.IO.File.ReadAllBytes (KSPUtil.ApplicationRootPath + "GameData/NewKerbol/PluginData/" + path);
			var tex = new Texture2D (4, 4, format, mipmaps);
			tex.LoadImage (data);
			return tex;
		}
		public static void SaveTexturePNG(string path, Texture2D tex)
		{
			var data = tex.EncodeToPNG ();
			System.IO.File.WriteAllBytes (KSPUtil.ApplicationRootPath + "GameData/NewKerbol/PluginData/" + path + ".png", data);
		}
		public static ConfigNode LoadConfig(string path)
		{
			return ConfigNode.Load (KSPUtil.ApplicationRootPath + "GameData/NewKerbol/" + path);
		}
		public static void SaveConfig(ConfigNode node, string path)
		{
			node.Save (KSPUtil.ApplicationRootPath + "GameData/NewKerbol/" + path);
		}
		public static Color Color(int r, int g, int b, int a = 255)
		{
			return new UnityEngine.Color ((((float)r) / 255f), (((float)g) / 255f), (((float)b) / 255f), (((float)a) / 255f) );
		}

		//mesh utils
		public static void ScaleVerts(Mesh mesh, float scaleFactor)
		{
            Vector3[] vertices = new Vector3[mesh.vertexCount];
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 v = mesh.vertices[i];
                v *= scaleFactor;
                vertices[i] = v;
            }
            mesh.vertices = vertices;
        }
		public static void MatchVerts(Mesh mesh, PQS pqs, double oceanHeight, float scaleFactor)
        {
            if (pqs == null)
            {
				Utils.LogError ("pqs is null");
                return;
            }
            pqs.isBuildingMaps = true;

            Vector3[] vertices = new Vector3[mesh.vertexCount];
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 v = mesh.vertices[i];
                double height = pqs.GetSurfaceHeight(v);
                if (height < oceanHeight)
                    height = oceanHeight;

                vertices[i] = v.normalized * (float)(1000.0 * height / pqs.radius * scaleFactor);
            }
            pqs.isBuildingMaps = false;
            mesh.vertices = vertices;
        }
		public static void CopyMesh(Mesh source, Mesh dest)
		{
			Vector3[] verts = new Vector3[source.vertexCount];
			source.vertices.CopyTo (verts, 0);
			dest.vertices = verts;

			int[] tris = new int[source.triangles.Length];
			source.triangles.CopyTo(tris, 0);
			dest.triangles = tris;

			Vector2[] uvs = new Vector2[source.uv.Length];
			source.uv.CopyTo(uvs, 0);
			dest.uv = uvs;

			Vector3[] normals = new Vector3[source.normals.Length];
			source.normals.CopyTo(normals, 0);
			dest.normals = normals;

			Vector4[] tangents = new Vector4[source.tangents.Length];
			source.tangents.CopyTo(tangents, 0);
			dest.tangents = tangents;
		}

		//logging utils
		public static void Log(object message)
		{
			Debug.Log ("[NewKerbol]: " + message);
		}
		public static void LogError(object message)
		{
			Debug.LogError ("[NewKerbol]: " + message);
		}
		public static void LogWarning(object message)
		{
			Debug.LogWarning ("[NewKerbol]: " + message);
		}
	}
}

