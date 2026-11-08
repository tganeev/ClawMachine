using UnityEngine;

public static class SimpleIconGenerator
{
    // Вспомогательный метод для создания прозрачной текстуры
    private static Color[] CreateTransparentCanvas(int size)
    {
        Color[] px = new Color[size * size];
        for (int i = 0; i < px.Length; i++)
            px[i] = new Color(0, 0, 0, 0);
        return px;
    }

    // Рисование эллипса
    private static void DrawEllipse(Color[] px, int size, float cx, float cy, float rx, float ry, Color color)
    {
        int minX = Mathf.FloorToInt(cx - rx);
        int maxX = Mathf.CeilToInt(cx + rx);
        int minY = Mathf.FloorToInt(cy - ry);
        int maxY = Mathf.CeilToInt(cy + ry);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (x < 0 || y < 0 || x >= size || y >= size) continue;
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                if (dx * dx + dy * dy <= 1.0f)
                    px[y * size + x] = color;
            }
        }
    }

    // Рисование прямоугольника
    private static void DrawRect(Color[] px, int size, int x, int y, int w, int h, Color color)
    {
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
            {
                int pxX = x + i;
                int pxY = y + j;
                if (pxX >= 0 && pxY >= 0 && pxX < size && pxY < size)
                    px[pxY * size + pxX] = color;
            }
    }

    // Рисование линии (для галочек и деталей)
    private static void DrawLine(Color[] px, int size, Vector2 start, Vector2 end, int thickness, Color color)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance * 2);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            for (int x = -thickness; x <= thickness; x++)
            {
                for (int y = -thickness; y <= thickness; y++)
                {
                    int pxX = Mathf.RoundToInt(point.x) + x;
                    int pxY = Mathf.RoundToInt(point.y) + y;

                    if (pxX >= 0 && pxX < size && pxY >= 0 && pxY < size)
                    {
                        px[pxY * size + pxX] = color;
                    }
                }
            }
        }
    }

    // Вспомогательный метод для заливки полигона (силуэта)
    private static void DrawPolygon(Color[] px, int size, Vector2[] points, Color color)
    {
        int minY = size, maxY = 0;
        foreach (var p in points)
        {
            if (p.y < minY) minY = Mathf.FloorToInt(p.y);
            if (p.y > maxY) maxY = Mathf.CeilToInt(p.y);
        }

        for (int y = minY; y <= maxY; y++)
        {
            var nodes = new System.Collections.Generic.List<float>();
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 p1 = points[i];
                Vector2 p2 = points[(i + 1) % points.Length];

                if ((p1.y < y && p2.y >= y) || (p2.y < y && p1.y >= y))
                {
                    float x = p1.x + (y - p1.y) / (p2.y - p1.y) * (p2.x - p1.x);
                    nodes.Add(x);
                }
            }

            nodes.Sort();
            for (int i = 0; i < nodes.Count; i += 2)
            {
                int startX = Mathf.RoundToInt(nodes[i]);
                int endX = Mathf.RoundToInt(nodes[i + 1]);
                for (int x = startX; x <= endX; x++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                        px[y * size + x] = color;
                }
            }
        }
    }

    // Создание спрайта из массива пикселей
    private static Sprite CreateSprite(Color[] px, int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // --- ИКОНКА МАРИО ---
    public static Sprite CreateMarioIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f;
        float cy = size * 0.45f;

        Color skin = new Color(0.98f, 0.82f, 0.65f);
        Color red = new Color(0.85f, 0.1f, 0.1f);
        Color darkRed = new Color(0.6f, 0.05f, 0.05f);
        Color white = Color.white;
        Color black = Color.black;
        Color brown = new Color(0.5f, 0.25f, 0.1f);

        DrawEllipse(px, size, cx - size * 0.32f, cy - size * 0.05f, size * 0.10f, size * 0.12f, skin);
        DrawEllipse(px, size, cx + size * 0.32f, cy - size * 0.05f, size * 0.10f, size * 0.12f, skin);
        DrawEllipse(px, size, cx, cy - size * 0.08f, size * 0.30f, size * 0.32f, skin);
        DrawEllipse(px, size, cx - size * 0.22f, cy - size * 0.1f, size * 0.06f, size * 0.15f, brown);
        DrawEllipse(px, size, cx + size * 0.22f, cy - size * 0.1f, size * 0.06f, size * 0.15f, brown);

        DrawEllipse(px, size, cx, cy + size * 0.22f, size * 0.36f, size * 0.24f, red);
        DrawEllipse(px, size, cx, cy + size * 0.12f, size * 0.38f, size * 0.10f, darkRed);
        DrawRect(px, size, (int)(cx - size * 0.4f), (int)(cy + size * 0.02f), (int)(size * 0.8f), (int)(size * 0.12f), new Color(0,0,0,0));
        DrawEllipse(px, size, cx, cy - size * 0.08f, size * 0.30f, size * 0.32f, skin);

        int mThick = (int)(size * 0.04f);
        Vector2 mCenter = new Vector2(cx, cy + size * 0.28f);
        Vector2 mLeftBase = new Vector2(mCenter.x - size * 0.1f, mCenter.y - size * 0.08f);
        Vector2 mRightBase = new Vector2(mCenter.x + size * 0.1f, mCenter.y - size * 0.08f);
        Vector2 mTopLeft = new Vector2(mCenter.x - size * 0.1f, mCenter.y + size * 0.08f);
        Vector2 mTopRight = new Vector2(mCenter.x + size * 0.1f, mCenter.y + size * 0.08f);
        Vector2 mMidBottom = new Vector2(mCenter.x, mCenter.y - size * 0.02f);

        DrawLine(px, size, mLeftBase, mTopLeft, mThick, white);
        DrawLine(px, size, mRightBase, mTopRight, mThick, white);
        DrawLine(px, size, mTopLeft, mMidBottom, mThick, white);
        DrawLine(px, size, mTopRight, mMidBottom, mThick, white);

        DrawEllipse(px, size, cx - size * 0.10f, cy + size * 0.02f, size * 0.08f, size * 0.12f, white);
        DrawEllipse(px, size, cx + size * 0.10f, cy + size * 0.02f, size * 0.08f, size * 0.12f, white);
        DrawEllipse(px, size, cx - size * 0.08f, cy - size * 0.01f, size * 0.04f, size * 0.06f, black);
        DrawEllipse(px, size, cx + size * 0.08f, cy - size * 0.01f, size * 0.04f, size * 0.06f, black);

        DrawEllipse(px, size, cx - size * 0.10f, cy + size * 0.16f, size * 0.07f, size * 0.03f, black);
        DrawEllipse(px, size, cx + size * 0.10f, cy + size * 0.16f, size * 0.07f, size * 0.03f, black);
        DrawEllipse(px, size, cx, cy - size * 0.08f, size * 0.12f, size * 0.10f, skin);
        DrawEllipse(px, size, cx - size * 0.10f, cy - size * 0.18f, size * 0.13f, size * 0.07f, black);
        DrawEllipse(px, size, cx + size * 0.10f, cy - size * 0.18f, size * 0.13f, size * 0.07f, black);
        DrawEllipse(px, size, cx, cy - size * 0.22f, size * 0.05f, size * 0.04f, skin);

        return CreateSprite(px, size);
    }

    // --- ИКОНКА МИШКИ ---
    public static Sprite CreateBearIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f, cy = size * 0.45f;
        Color brown = new Color(0.6f, 0.4f, 0.2f);
        Color lightBrown = new Color(0.8f, 0.6f, 0.4f);

        DrawEllipse(px, size, cx - size * 0.25f, cy + size * 0.25f, size * 0.12f, size * 0.12f, brown);
        DrawEllipse(px, size, cx + size * 0.25f, cy + size * 0.25f, size * 0.12f, size * 0.12f, brown);
        DrawEllipse(px, size, cx, cy, size * 0.35f, size * 0.32f, brown);
        DrawEllipse(px, size, cx, cy - size * 0.1f, size * 0.15f, size * 0.12f, lightBrown);
        DrawEllipse(px, size, cx, cy - size * 0.05f, size * 0.05f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx - size * 0.12f, cy + size * 0.1f, size * 0.04f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx + size * 0.12f, cy + size * 0.1f, size * 0.04f, size * 0.04f, Color.black);

        return CreateSprite(px, size);
    }

    // --- ИКОНКА ЛАБУБУ (Коричневое пятно / Винни-Пух) ---
    public static Sprite CreateLabubuIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f, cy = size * 0.45f;
        Color darkBrown = new Color(0.4f, 0.2f, 0.1f);
        Color lightFace = new Color(0.9f, 0.7f, 0.5f);

        DrawEllipse(px, size, cx - size * 0.3f, cy, size * 0.12f, size * 0.15f, darkBrown);
        DrawEllipse(px, size, cx + size * 0.3f, cy, size * 0.12f, size * 0.15f, darkBrown);
        DrawEllipse(px, size, cx, cy, size * 0.25f, size * 0.25f, darkBrown);
        DrawEllipse(px, size, cx, cy - size * 0.05f, size * 0.18f, size * 0.18f, lightFace);
        DrawEllipse(px, size, cx - size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx + size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx, cy - size * 0.12f, size * 0.08f, size * 0.03f, Color.black);

        return CreateSprite(px, size);
    }

    // --- ИКОНКА ОБЕЗЬЯНЫ (Анфас с улыбкой и хохолком) ---
    public static Sprite CreateMonkeyIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f, cy = size * 0.45f;

        Color brown = new Color(0.45f, 0.25f, 0.15f);
        Color lightFace = new Color(0.95f, 0.9f, 0.75f);
        Color innerEar = new Color(0.8f, 0.6f, 0.5f);
        Color black = Color.black;
        Color white = Color.white;
        Color mouthDark = new Color(0.3f, 0.1f, 0.1f);
        Color tongue = new Color(0.9f, 0.4f, 0.4f);

        DrawEllipse(px, size, cx - size * 0.35f, cy + size * 0.05f, size * 0.12f, size * 0.15f, brown);
        DrawEllipse(px, size, cx + size * 0.35f, cy + size * 0.05f, size * 0.12f, size * 0.15f, brown);
        DrawEllipse(px, size, cx - size * 0.35f, cy + size * 0.05f, size * 0.08f, size * 0.1f, innerEar);
        DrawEllipse(px, size, cx + size * 0.35f, cy + size * 0.05f, size * 0.08f, size * 0.1f, innerEar);

        DrawEllipse(px, size, cx - size * 0.05f, cy + size * 0.32f, size * 0.06f, size * 0.1f, brown);
        DrawEllipse(px, size, cx + size * 0.05f, cy + size * 0.32f, size * 0.06f, size * 0.1f, brown);
        DrawEllipse(px, size, cx, cy + size * 0.35f, size * 0.05f, size * 0.08f, brown);

        DrawEllipse(px, size, cx, cy, size * 0.35f, size * 0.32f, brown);
        DrawEllipse(px, size, cx, cy - size * 0.05f, size * 0.26f, size * 0.22f, lightFace);

        DrawEllipse(px, size, cx - size * 0.12f, cy + size * 0.08f, size * 0.07f, size * 0.08f, black);
        DrawEllipse(px, size, cx + size * 0.12f, cy + size * 0.08f, size * 0.07f, size * 0.08f, black);
        DrawEllipse(px, size, cx - size * 0.10f, cy + size * 0.1f, size * 0.02f, size * 0.02f, white);
        DrawEllipse(px, size, cx + size * 0.14f, cy + size * 0.1f, size * 0.02f, size * 0.02f, white);

        DrawEllipse(px, size, cx - size * 0.05f, cy - size * 0.02f, size * 0.015f, size * 0.01f, black);
        DrawEllipse(px, size, cx + size * 0.05f, cy - size * 0.02f, size * 0.015f, size * 0.01f, black);

        DrawEllipse(px, size, cx, cy - size * 0.15f, size * 0.15f, size * 0.1f, black);
        DrawEllipse(px, size, cx, cy - size * 0.1f, size * 0.13f, size * 0.08f, lightFace);

        DrawEllipse(px, size, cx, cy - size * 0.2f, size * 0.06f, size * 0.04f, tongue);
        DrawLine(px, size, new Vector2(cx, cy - size * 0.2f), new Vector2(cx, cy - size * 0.24f), 1, black);

        return CreateSprite(px, size);
    }

    // --- ИКОНКА КЕНГУРУ (Голова анфас, как на скрине) ---
    public static Sprite CreateKangarooIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f, cy = size * 0.45f;

        Color darkBrown = new Color(0.35f, 0.2f, 0.1f);
        Color tan = new Color(0.85f, 0.6f, 0.2f);
        Color lightTan = new Color(0.95f, 0.8f, 0.55f);
        Color noseColor = new Color(0.35f, 0.15f, 0.1f);
        Color eyeColor = new Color(0.15f, 0.1f, 0.05f);

        // Уши
        DrawEllipse(px, size, cx - size * 0.22f, cy + size * 0.25f, size * 0.18f, size * 0.28f, darkBrown);
        DrawEllipse(px, size, cx + size * 0.22f, cy + size * 0.25f, size * 0.18f, size * 0.28f, darkBrown);

        // Внутренняя часть ушей
        DrawEllipse(px, size, cx - size * 0.22f, cy + size * 0.25f, size * 0.12f, size * 0.22f, tan);
        DrawEllipse(px, size, cx + size * 0.22f, cy + size * 0.25f, size * 0.12f, size * 0.22f, tan);

        // Голова
        DrawEllipse(px, size, cx, cy - size * 0.02f, size * 0.26f, size * 0.30f, tan);

        // Морда
        DrawEllipse(px, size, cx, cy - size * 0.1f, size * 0.10f, size * 0.24f, lightTan);
        DrawEllipse(px, size, cx - size * 0.15f, cy - size * 0.12f, size * 0.08f, size * 0.18f, lightTan);
        DrawEllipse(px, size, cx + size * 0.15f, cy - size * 0.12f, size * 0.08f, size * 0.18f, lightTan);

        // Глаза
        DrawEllipse(px, size, cx - size * 0.13f, cy + size * 0.02f, size * 0.05f, size * 0.02f, eyeColor);
        DrawEllipse(px, size, cx + size * 0.13f, cy + size * 0.02f, size * 0.05f, size * 0.02f, eyeColor);

        // Нос
        DrawEllipse(px, size, cx, cy - size * 0.22f, size * 0.08f, size * 0.06f, noseColor);

        // Ноздри
        Color nostrilColor = new Color(0.2f, 0.1f, 0.05f);
        DrawEllipse(px, size, cx - size * 0.03f, cy - size * 0.21f, size * 0.015f, size * 0.02f, nostrilColor);
        DrawEllipse(px, size, cx + size * 0.03f, cy - size * 0.21f, size * 0.015f, size * 0.02f, nostrilColor);

        // Рот
        DrawEllipse(px, size, cx, cy - size * 0.32f, size * 0.04f, size * 0.01f, tan);

        return CreateSprite(px, size);
    }
}
