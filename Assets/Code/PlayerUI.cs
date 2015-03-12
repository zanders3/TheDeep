

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Sprite HeartSprite;
    public int HeartSpriteSize = 64;
    
    List<GameObject> healthSprites = new List<GameObject>();
    
    public int Health
    {
        set
        {
            while (healthSprites.Count > value && healthSprites.Count > 0)
            {
                GameObject.Destroy(healthSprites[healthSprites.Count-1]);
                healthSprites.RemoveAt(healthSprites.Count-1);
            }
            while (healthSprites.Count < value)
            {
                GameObject healthGO = new GameObject("Health", typeof(Image));
                healthGO.transform.SetParent(transform, true);
                healthGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(healthSprites.Count * HeartSpriteSize, 0.0f);
                var image = healthGO.GetComponent<Image>();
                image.sprite = HeartSprite;
                image.SetNativeSize();

                healthSprites.Add(healthGO);
            }
        }
    }
}