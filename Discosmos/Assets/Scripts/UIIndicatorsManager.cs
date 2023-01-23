using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIIndicatorsManager : MonoBehaviour
{
    public Camera cam;
    public Transform camCenter;
    public List<GameObject> obj;
    public RectTransform rectAdjusted;
    public Rect rectTransformed;
    public Vector2 center;
    public List<Transform> image;
    public List<Image> arrowImage;
    public List<Image> headImage;
    public GameObject indic;
    public Transform uiParent;
    public Sprite[] sprites;

    private void Start()
    {
        center = cam.WorldToScreenPoint(camCenter.position);
        for (int i = 0; i < obj.Count; i++)
        {
            CreateImages(i,i%2+4);
        }

        rectTransformed = new Rect(rectAdjusted.rect.position + (Vector2) rectAdjusted.position, rectAdjusted.rect.size);
    }

    private void Update()
    {
        for (int i = 0; i < obj.Count; i++)
        {
            UpdateIndic(i);
        }
        
    }

    void CreateImages(int sprite,int color)
    {
        image.Add(Instantiate(indic, Vector3.zero, quaternion.identity,uiParent).transform);
        headImage.Add(image[image.Count - 1].GetChild(1).GetComponent<Image>());
        arrowImage.Add(image[image.Count - 1].GetChild(0).GetComponentInChildren<Image>());
        headImage[image.Count - 1].sprite = sprites[sprite];
        arrowImage[image.Count - 1].sprite = sprites[color];
        image[image.Count - 1].gameObject.name = sprite.ToString();
        Debug.Log("IMAGE CREATED");
    }

    public void UpdateIndic(int indexObj)
    {
        Vector2 screenPos = cam.WorldToScreenPoint(obj[indexObj].transform.position);
        
        if (rectTransformed.Contains(screenPos))
        {
            headImage[indexObj].color = new Color(1,1,1,0);
            arrowImage[indexObj].color = new Color(1,1,1,0);
        }
        else
        {
            Vector3 objPosNear = camCenter.position + (obj[indexObj].transform.position - camCenter.position).normalized*3;
            Vector2 pos = cam.WorldToScreenPoint(objPosNear);
            
            pos = FindPointOnRectBorder( pos - center,center,rectTransformed);

            float dist = Vector2.Distance(screenPos, pos) / 150;
            headImage[indexObj].color = Color.Lerp(new Color(1,1,1,0),Color.white,dist);
            arrowImage[indexObj].color = Color.Lerp(new Color(1,1,1,0),Color.white,dist);
            
            image[indexObj].transform.position = pos;
            image[indexObj].GetChild(0).rotation = Quaternion.LookRotation(Vector3.forward,pos - center);
        }
    }

    public void RemoveIndic(int indexObj)
    {
        Destroy(image[indexObj].gameObject);
        image.RemoveAt(indexObj);
        obj.RemoveAt(indexObj);
    }

    public void AddIndic(GameObject newObj,int sprite,int color, out int indicNb)
    {
        obj.Add(newObj);
        CreateImages(sprite,color);
        indicNb = obj.Count - 1;
        Debug.Log("INDIC CREATED "+ indicNb);
    }

    public Vector2 FindPointOnRectBorder(Vector2 dir,Vector2 center,Rect rect)
    {
        float angleSup = Vector2.SignedAngle(Vector2.up, rect.max - center);
        float angleInf = Vector2.SignedAngle(Vector2.up, new Vector2(rect.xMax,rect.yMin) - center);
        float angle = Vector2.SignedAngle(Vector2.up, dir);
        
        if (angle > angleSup && angle < -angleSup)
        {
            Vector2 intersection = Intersection(center, center + dir.normalized * 1000, new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), out bool found);
            return intersection;
        }
        if (angle < angleInf || angle > -angleInf)
        {
            Vector2 intersection = Intersection(center, center + dir.normalized * 1000, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), out bool found);
            return intersection;
        }
        if (angle > 0)
        {
            Vector2 intersection = Intersection(center, center + dir.normalized * 1000, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), out bool found);
            return intersection;
        } 
        if (angle < 0)
        {
            Vector2 intersection = Intersection(center, center + dir.normalized * 1000, new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), out bool found);
            return intersection;
        }
        return Vector2.zero;
    }
    
    
    public Vector2 Intersection(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
    {
       
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
 
        if (tmp == 0)
        {
            // No solution!
            found = false;
            return Vector2.zero;
        }
 
        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;
 
        found = true;
 
        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );
    }
}
