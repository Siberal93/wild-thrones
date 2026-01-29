using UnityEngine;
using TMPro;

public partial class BoardBootstrap
{
    TextMeshPro CreateLabelTMP(string text, Transform parent, Vector2 localOffset, float fontSize)
    {
        var go = new GameObject("LabelTMP");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(localOffset.x, localOffset.y, -0.2f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.color = Color.white;
        tmp.extraPadding = true;
        tmp.fontStyle = FontStyles.Bold;

        tmp.rectTransform.sizeDelta = new Vector2(2.5f, 1.5f);

        // IMPORTANTISSIMO: porta il testo sopra gli sprite
        var mr = tmp.GetComponent<MeshRenderer>();
        mr.sortingOrder = 200;

        return tmp;
    }


    GameObject CreateSlotOutline(string name, Vector2 center, float size)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(center.x, center.y, 0f);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 4;
        lr.startWidth = 0.06f;
        lr.endWidth = 0.06f;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        float h = size * 0.5f;
        lr.SetPosition(0, new Vector3(-h, -h, 0));
        lr.SetPosition(1, new Vector3(-h, h, 0));
        lr.SetPosition(2, new Vector3(h, h, 0));
        lr.SetPosition(3, new Vector3(h, -h, 0));

        return go;
    }

    GameObject CreateSlotFill(Transform slot, float size, Color fill)
    {
        var go = new GameObject("Fill");
        go.transform.SetParent(slot, false);
        go.transform.localPosition = new Vector3(0, 0, 0.05f);
        go.transform.localScale = new Vector3(size, size, 1);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        sr.color = fill;
        sr.sortingOrder = -5; // dietro a tutto
        return go;
    }

    GameObject CreateCircleOutline(string name, Vector2 center, float radius, Color color, float width = 0.06f, int segments = 48)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(center.x, center.y, -1f);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = segments;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = color;
        lr.endColor = color;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        // sorting per stare sopra agli slot
        lr.sortingOrder = 10;

        for (int i = 0; i < segments; i++)
        {
            float a = (i / (float)segments) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }

        return go;
    }

    GameObject CreateCircleFill(Transform parent, float radius, Color fill)
    {
        var go = new GameObject("CircleFill");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0, 0, 0.01f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        sr.color = fill;
        sr.sortingOrder = 5;

        float d = radius * 2f;
        go.transform.localScale = new Vector3(d, d, 1f);
        return go;
    }
}
