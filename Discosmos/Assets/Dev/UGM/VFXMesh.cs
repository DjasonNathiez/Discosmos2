using System.Collections.Generic;
using UnityEngine;


	public class VFXMesh : MonoBehaviour
	{
		[Header("Mesh Regenerate")]
		[Tooltip("Should the mesh be regenerated in playmode (Update() method) ?")]
		public bool updateInPlayMode;

		[Header("Ring")]
		[Tooltip("The number of 'sides' of the circle.")]
		[Range(3f, 50f)]
		public int ringAmount = 20;

		[Tooltip("Remaps the position of the ring vertices, allows to change the distribution of the topology.")]
		public AnimationCurve ringPositionRemap = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

		[Header("Loop")]
		[Tooltip("The number of intermediate vertices between the inner edge and the outer edge.")]
		[Range(2f, 30f)]
		public int loopAmount = 3;

		[Tooltip("Remaps the position of the loop vertices, allows to change the distribution of the topology.")]
		public AnimationCurve loopPositionRemap = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

		[Header("Width")]
		[Tooltip("Distance from center of the inner edge.")]
		[Range(0f, 20f)]
		public float minRadius = 0.5f;

		[Tooltip("Distance from center of the outer edge.")]
		[Range(0f, 20f)]
		public float maxRadius = 1f;

		[Tooltip("Remaps the width of the circle.")]
		public AnimationCurve widthRemap = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));

		[Tooltip("Influence of the width remap curve.")]
		[Range(0f, 1f)]
		public float widthRemapInfluence = 1f;

		[Header("Depth")]
		[Tooltip("Position of the last loop in the Z-Axis.")]
		[Range(-20f, 20f)]
		public float outerLoopDepth;

		[Tooltip("Remaps the position of the loop vertices on their vertical axis, which changes the shape of the cone.")]
		public AnimationCurve loopHeightPositionRemap = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

		[Tooltip("Position of the last edge in the Z-Axis.")]
		[Range(-5f, 5f)]
		public float maxLoopDepth;

		[Tooltip("Remaps the position of the loop vertices on their depth axis, which changes the shape of the cone / circle.")]
		public AnimationCurve loopDepthPositionRemap = new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(0.5f, 1f, 0f, 0f), new Keyframe(1f, 0f, -2f, 0f));

		[Tooltip("The higher the U coordinate, the further it is in the Z-Axis.")]
		[Range(-10f, 10f)]
		public float depthByU;

		[Tooltip("Number of spirals, only relevant if using Depth by U variable.")]
		[Range(1f, 5f)]
		public int spiralAmount = 1;

		[Header("Ring Offset")]
		[Tooltip("Position of the last ring in degree.")]
		[Range(0f, 360f)]
		public float arc = 360f;

		[Tooltip("Offset each loop vertex on the right or the left.")]
		[Range(-360f, 360f)]
		public float twist;

		[Header("UVs")]
		[Tooltip("Invert the U and the V coordinates.")]
		public bool invertUAndV;

		[Tooltip("Change the current shader with a shader displaying the UV coordinates.")]
		public bool displayUVs;

		[Header("Normals")]
		[Tooltip("Flip the normals direction.")]
		public bool flipNormals;

		[Tooltip("Draw the normals. Color is based on their orientation.")]
		public bool drawNormalDebug;

		[Tooltip("Size of the normal debug.")]
		[Range(0f, 2f)]
		public float normalSizeDebug = 0.5f;

		[Header("Vertices")]
		[Tooltip("Draw the vertices.")]
		public bool drawVertexDebug;

		[Tooltip("Size of the debug vertices.")]
		[Range(0f, 0.03f)]
		public float vertexSizeDebug = 0.03f;

		[Tooltip("Color of the debug vertices.")]
		public Color vertexColorDebug = Color.black;

		[Header("Mesh Data")]
		public Mesh mesh;

		public List<Vector3> vertices = new List<Vector3>();

		public List<Vector3> normals = new List<Vector3>();

		public List<int> triangles = new List<int>();

		public List<Vector2> uv = new List<Vector2>();

		public bool initialized;

		public MeshFilter meshFilter;

		public MeshRenderer meshRenderer;

		public Shader lastShader;

		private void OnValidate()
		{
			UpdateMesh();
			GenerateMesh();
		}

		private void OnDrawGizmos()
		{
			DrawNormals();
			DrawVertices();
			AddMeshComponents();
		}

		private void Update()
		{
			if (updateInPlayMode)
			{
				UpdateMesh();
			}
		}

		private void UpdateMesh()
		{
			ShaderUpdate();
			if (mesh == null)
			{
				GenerateMesh();
			}
		}

		public void GenerateMesh()
		{
			ClearAll();
			AddMainVertices();
			GenerateTriangles();
			ApplyMesh();
		}

		private void AddMainVertices()
		{
			float num = 360f / (float)(RingAmountMultipliedByCircle() - 1);
			float num2 = arc / 360f;
			for (int i = 0; i < loopAmount; i++)
			{
				for (int j = 0; j < RingAmountMultipliedByCircle(); j++)
				{
					float time = Interpolate(0f, 1f, 0f, RingAmountMultipliedByCircle() - 1, j);
					float num3 = ringPositionRemap.Evaluate(time);
					float u = Interpolate(0f, 1f, 0f, RingAmountMultipliedByCircle() - 1, j);
					Quaternion q = Quaternion.AngleAxis((float)spiralAmount * num * (num3 * (float)(RingAmountMultipliedByCircle() - 1)) * num2, Vector3.back);
					GenerateVertex(i, q, u, j);
				}
			}
		}

		private void GenerateVertex(int l, Quaternion q, float u, int r)
		{
			float num = Interpolate(0f, 1f, 0f, loopAmount - 1, l);
			float time = Interpolate(0f, 1f, 0f, RingAmountMultipliedByCircle() - 1, r);
			float num2 = Mathf.LerpUnclamped(0f, ClampedOuterLoopDepth(), loopPositionRemap.Evaluate(num));
			float num3 = Mathf.LerpUnclamped(0f, maxLoopDepth, loopDepthPositionRemap.Evaluate(num));
			float time2 = Mathf.InverseLerp(0f, ClampedOuterLoopDepth(), num2);
			float t = Mathf.Lerp(1f, widthRemap.Evaluate(time), widthRemapInfluence);
			float num4 = Mathf.LerpUnclamped(minRadius, Mathf.LerpUnclamped(minRadius, maxRadius, t), loopHeightPositionRemap.Evaluate(time2));
			Quaternion quaternion = Quaternion.AngleAxis(Mathf.LerpUnclamped(0f, twist, num), Vector3.back);
			Vector3 vert = q * quaternion * (Vector3.up * num4) + new Vector3(0f, 0f, num2 + depthByU * u + num3);
			float num5 = 0.5f;
			float z = Random.Range(0f - num5, num5);
			new Vector3(0f, 0f, z);
			AddVertex(vert);
			if (!invertUAndV)
			{
				uv.Add(new Vector2(u, num));
			}
			else
			{
				uv.Add(new Vector2(num, u));
			}
		}

		private void GenerateTriangles()
		{
			for (int i = 0; i < loopAmount - 1; i++)
			{
				for (int j = 0; j < RingAmountMultipliedByCircle() - 1; j++)
				{
					int num = i * RingAmountMultipliedByCircle();
					int a = j + num;
					int b = j + RingAmountMultipliedByCircle() + num;
					int num2 = j + RingAmountMultipliedByCircle() + 1 + num;
					int c = j + 1 + num;
					AddTriangle(a, b, num2);
					AddTriangle(a, num2, c);
				}
			}
		}

		private void AddTriangle(int a, int b, int c)
		{
			if (!flipNormals)
			{
				triangles.Add(a);
				triangles.Add(b);
				triangles.Add(c);
			}
			else
			{
				triangles.Add(c);
				triangles.Add(b);
				triangles.Add(a);
			}
		}

		private void AddVertex(Vector3 vert)
		{
			vertices.Add(vert);
		}

		private void ClearAll()
		{
			vertices.Clear();
			triangles.Clear();
			uv.Clear();
			if ((bool)mesh)
			{
				mesh.triangles = triangles.ToArray();
				mesh.vertices = vertices.ToArray();
				mesh.uv = uv.ToArray();
			}
		}

		private void ApplyMesh()
		{
			if (mesh == null)
			{
				mesh = new Mesh();
			}
			mesh.name = "VFX Mesh";
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.uv = uv.ToArray();
			mesh.RecalculateNormals();
			FixNormals();
			if ((bool)meshFilter && meshFilter.sharedMesh != mesh)
			{
				meshFilter.sharedMesh = mesh;
			}
		}

		private void AddMeshComponents()
		{
			if (meshFilter == null)
			{
				MeshFilter component = GetComponent<MeshFilter>();
				if (component == null)
				{
					meshFilter = base.gameObject.AddComponent<MeshFilter>();
				}
				else
				{
					meshFilter = component;
				}
			}
			if (meshRenderer == null)
			{
				MeshRenderer component2 = GetComponent<MeshRenderer>();
				if (component2 == null)
				{
					meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
				}
				else
				{
					meshRenderer = component2;
				}
			}
			else if (meshRenderer.sharedMaterial == null)
			{
				meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
			}
		}

		private void ShaderUpdate()
		{
			if ((bool)meshRenderer && (bool)meshRenderer.sharedMaterial)
			{
				if (meshRenderer.sharedMaterial.shader != Shader.Find("QuikFX/DisplayUVs"))
				{
					lastShader = meshRenderer.sharedMaterial.shader;
				}
				if (displayUVs)
				{
					meshRenderer.sharedMaterial.shader = Shader.Find("QuikFX/DisplayUVs");
				}
				else if ((bool)lastShader && meshRenderer.sharedMaterial.shader != lastShader)
				{
					meshRenderer.sharedMaterial.shader = lastShader;
				}
			}
		}

		private void FixNormals()
		{
			if (vertices.Count > 0)
			{
				normals = new List<Vector3>();
				for (int i = 0; i < mesh.normals.Length; i++)
				{
					normals.Add(mesh.normals[i]);
				}
				for (int j = 0; j < loopAmount; j++)
				{
					int num = j * (ringAmount + 1);
					Vector3 value = normals[num];
					normals[num + ringAmount] = value;
					mesh.normals = normals.ToArray();
				}
			}
		}

		private void DrawNormals()
		{
			if (drawNormalDebug)
			{
				for (int i = 0; i < normals.Count; i++)
				{
					Vector3 vector = normals[i];
					Gizmos.color = new Color(vector.x, vector.y, vector.z, 1f);
					Vector3 vector2 = base.transform.rotation * vertices[i] + base.transform.position;
					Vector3 to = vector2 + base.transform.rotation * (vector * normalSizeDebug);
					Gizmos.DrawLine(vector2, to);
				}
			}
		}

		private void DrawVertices()
		{
			if (drawVertexDebug)
			{
				for (int i = 0; i < vertices.Count; i++)
				{
					Gizmos.color = vertexColorDebug;
					Gizmos.DrawSphere(base.transform.rotation * vertices[i] + base.transform.position, vertexSizeDebug);
				}
			}
		}

		public int RingAmountMultipliedByCircle()
		{
			return (ringAmount + 1) * spiralAmount;
		}

		private float ClampedOuterLoopDepth()
		{
			if (outerLoopDepth == 0f)
			{
				return 0.0001f;
			}
			return outerLoopDepth;
		}

		public static float Interpolate(float minValueReturn, float maxValueReturn, float minValueToCheck, float maxValueToCheck, float valueToCheck)
		{
			return Mathf.LerpUnclamped(minValueReturn, maxValueReturn, Mathf.InverseLerp(minValueToCheck, maxValueToCheck, valueToCheck));
		}
	}
