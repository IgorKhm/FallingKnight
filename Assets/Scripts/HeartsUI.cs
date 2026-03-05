using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Wiring — place Image children manually in the editor")]
    public Transform container;        // HeartsContainer

    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    [Header("Units")]
    [Tooltip("How many HP units per one heart. Use 2 for half-heart system.")]
    public int unitsPerHeart = 2;

    [Header("Shake")]
    public int shakeFrames = 15;
    public float shakeMagnitude = 8f;

    private readonly List<Image> hearts = new();
    private Coroutine[] _heartCoroutines;

    public void Init()
    {
        hearts.Clear();

        foreach (Transform child in container)
        {
            var img = child.GetComponent<Image>();
            if (img != null)
                hearts.Add(img);
        }

        _heartCoroutines = new Coroutine[hearts.Count];
        Debug.Log($"HeartsUI.Init found {hearts.Count} pre-placed hearts");
    }

    public void SetHP(int hpUnits)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            int heartStart = i * unitsPerHeart;
            int remaining = hpUnits - heartStart;

            Sprite newSprite;
            if (remaining >= unitsPerHeart) newSprite = fullHeart;
            else if (remaining == 1) newSprite = halfHeart;
            else newSprite = emptyHeart;

            if (hearts[i].sprite != newSprite)
            {
                bool lost = SpriteFill(newSprite) < SpriteFill(hearts[i].sprite);
                hearts[i].sprite = newSprite;
                if (lost) ShakeHeart(i);
            }
        }
    }

    // Returns a number representing how full a sprite is — used to detect HP loss
    private int SpriteFill(Sprite s)
    {
        if (s == fullHeart) return 2;
        if (s == halfHeart) return 1;
        return 0;
    }

    private void ShakeHeart(int index)
    {
        if (_heartCoroutines[index] != null) StopCoroutine(_heartCoroutines[index]);
        _heartCoroutines[index] = StartCoroutine(ShakeCoroutine(hearts[index].rectTransform));
    }

    private IEnumerator ShakeCoroutine(RectTransform target)
    {
        Vector3 origin = target.localPosition;

        for (int i = 0; i < shakeFrames; i++)
        {
            float t = 1f - (float)i / shakeFrames;   // 1 → 0 over the shake duration
            float magnitude = shakeMagnitude * t;
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);
            target.localPosition = origin + new Vector3(x, y, 0f);
            yield return null;
        }

        target.localPosition = origin;
    }
}
