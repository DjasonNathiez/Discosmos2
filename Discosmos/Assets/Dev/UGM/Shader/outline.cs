using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class outline : MonoBehaviour
{
	public Shader DrawAsSolidColor;
	public Shader Outline;
	Material _outlineMaterial;
	Camera TempCam;

	void Start()
	{
		_outlineMaterial = new Material(Outline);

		TempCam = new GameObject().AddComponent<Camera>();
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		TempCam.CopyFrom(Camera.current);
		TempCam.backgroundColor = Color.black;
		TempCam.clearFlags = CameraClearFlags.Color;

		TempCam.cullingMask = 1 << LayerMask.NameToLayer("Outline");

		var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.R8);

		TempCam.targetTexture = rt;

		TempCam.RenderWithShader(DrawAsSolidColor, "");

		Graphics.Blit(rt, dst, _outlineMaterial);

		RenderTexture.ReleaseTemporary(rt);
	}
}