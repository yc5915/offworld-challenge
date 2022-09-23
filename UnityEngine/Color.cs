namespace UnityEngine
{
    public struct Color
    {
        public float r;

        public float g;

        public float b;

        public float a;

        public static Color red => new Color(1f, 0f, 0f, 1f);

        public static Color green => new Color(0f, 1f, 0f, 1f);

        public static Color blue => new Color(0f, 0f, 1f, 1f);

        public static Color white => new Color(1f, 1f, 1f, 1f);

        public static Color black => new Color(0f, 0f, 0f, 1f);

        public static Color yellow => new Color(1f, 47f / 51f, 4f / 255f, 1f);

        public static Color cyan => new Color(0f, 1f, 1f, 1f);

        public static Color magenta => new Color(1f, 0f, 1f, 1f);

        public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);

        public static Color grey => new Color(0.5f, 0.5f, 0.5f, 1f);

        public static Color clear => new Color(0f, 0f, 0f, 0f);

        public float grayscale => 0.299f * r + 0.587f * g + 0.114f * b;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            a = 1f;
        }
    }
}
