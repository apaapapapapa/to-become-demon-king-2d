using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 外部アートアセットがない段階でも試作を維持できるよう、単純な図形Spriteを実行時に生成します。
    /// 本番アセットへの移行時は、このFactoryの利用箇所をPrefabやTilemapへ置き換えます。
    /// </summary>
    internal sealed class RuntimeShapeFactory
    {
        private Sprite squareSprite;
        private Sprite circleSprite;
        private Sprite diamondSprite;

        public GameObject CreateDiamond(string name, Vector2 position, Vector2 size, Color color,
            int order, Transform parent)
        {
            return CreatePatch(name, position, size, color, order, parent, DiamondSprite);
        }

        public GameObject CreateEllipse(string name, Vector2 position, Vector2 size, Color color,
            int order, Transform parent)
        {
            return CreatePatch(name, position, size, color, order, parent, CircleSprite);
        }

        public GameObject CreatePatch(string name, Vector2 position, Vector2 size, Color color,
            int order, Transform parent, Sprite sprite = null, float rotation = 0f)
        {
            GameObject patch = new(name);
            patch.transform.SetParent(parent, false);
            patch.transform.localPosition = new Vector3(position.x, position.y, 0f);
            patch.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            patch.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = patch.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite != null ? sprite : SquareSprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return patch;
        }

        private Sprite SquareSprite
        {
            get
            {
                if (squareSprite == null)
                {
                    squareSprite = CreateShapeSprite("四角ピクセル", 0);
                }

                return squareSprite;
            }
        }

        private Sprite CircleSprite
        {
            get
            {
                if (circleSprite == null)
                {
                    circleSprite = CreateShapeSprite("丸ピクセル", 1);
                }

                return circleSprite;
            }
        }

        private Sprite DiamondSprite
        {
            get
            {
                if (diamondSprite == null)
                {
                    diamondSprite = CreateShapeSprite("菱形ピクセル", 2);
                }

                return diamondSprite;
            }
        }

        private static Sprite CreateShapeSprite(string name, int shape)
        {
            const int size = 16;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
                    float ny = (y + 0.5f - size * 0.5f) / (size * 0.5f);
                    bool visible = shape switch
                    {
                        1 => nx * nx + ny * ny <= 1f,
                        2 => Mathf.Abs(nx) + Mathf.Abs(ny) <= 1f,
                        _ => true
                    };
                    texture.SetPixel(x, y, visible ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
