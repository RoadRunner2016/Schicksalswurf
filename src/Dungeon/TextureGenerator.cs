using Godot;

namespace Schicksalswurf.Dungeon
{
    /// <summary>
    /// Generates procedural textures at runtime using noise and gradients.
    /// No external image files needed.
    /// </summary>
    public static class TextureGenerator
    {
        private const int TexSize = 256;

        /// <summary>
        /// Stone wall texture with noise-based variation.
        /// </summary>
        public static ImageTexture CreateStoneWallTexture()
        {
            var image = Image.CreateEmpty(TexSize, TexSize, false, Image.Format.Rgba8);
            var noise = new FastNoiseLite
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
                Frequency = 0.08f,
                Seed = 42
            };

            for (int x = 0; x < TexSize; x++)
            {
                for (int y = 0; y < TexSize; y++)
                {
                    float n = noise.GetNoise2D(x, y);
                    float n2 = noise.GetNoise2D(x * 2, y * 2) * 0.3f;
                    float v = (n + n2 + 1.0f) * 0.5f;

                    // Brick pattern lines
                    int brickRow = y / 32;
                    int brickCol = (x + (brickRow % 2) * 16) / 32;
                    bool onBrickLine = (y % 32 < 2) || (x % 32 < 2);

                    float r, g, b;
                    if (onBrickLine)
                    {
                        r = 0.15f; g = 0.13f; b = 0.1f;
                    }
                    else
                    {
                        // Vary brick color slightly per brick
                        float brickVar = ((brickRow * 7 + brickCol * 13) % 20) / 20.0f * 0.1f;
                        r = 0.35f + v * 0.15f + brickVar;
                        g = 0.32f + v * 0.13f + brickVar;
                        b = 0.28f + v * 0.1f + brickVar;
                    }

                    image.SetPixel(x, y, new Color(r, g, b));
                }
            }

            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Stone floor texture with cobblestone pattern.
        /// </summary>
        public static ImageTexture CreateStoneFloorTexture()
        {
            var image = Image.CreateEmpty(TexSize, TexSize, false, Image.Format.Rgba8);
            var noise = new FastNoiseLite
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
                Frequency = 0.1f,
                Seed = 100
            };

            for (int x = 0; x < TexSize; x++)
            {
                for (int y = 0; y < TexSize; y++)
                {
                    float n = noise.GetNoise2D(x, y);
                    float v = (n + 1.0f) * 0.5f;

                    // Darker stone floor
                    float r = 0.18f + v * 0.1f;
                    float g = 0.16f + v * 0.09f;
                    float b = 0.13f + v * 0.07f;

                    image.SetPixel(x, y, new Color(r, g, b));
                }
            }

            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Dark ceiling texture.
        /// </summary>
        public static ImageTexture CreateCeilingTexture()
        {
            var image = Image.CreateEmpty(TexSize, TexSize, false, Image.Format.Rgba8);
            var noise = new FastNoiseLite
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
                Frequency = 0.15f,
                Seed = 200
            };

            for (int x = 0; x < TexSize; x++)
            {
                for (int y = 0; y < TexSize; y++)
                {
                    float n = noise.GetNoise2D(x, y);
                    float v = (n + 1.0f) * 0.5f;

                    float r = 0.08f + v * 0.05f;
                    float g = 0.07f + v * 0.04f;
                    float b = 0.06f + v * 0.03f;

                    image.SetPixel(x, y, new Color(r, g, b));
                }
            }

            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Wooden door texture with planks.
        /// </summary>
        public static ImageTexture CreateDoorTexture()
        {
            var image = Image.CreateEmpty(TexSize, TexSize, false, Image.Format.Rgba8);
            var noise = new FastNoiseLite
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
                Frequency = 0.2f,
                Seed = 300
            };

            for (int x = 0; x < TexSize; x++)
            {
                for (int y = 0; y < TexSize; y++)
                {
                    float n = noise.GetNoise2D(x, y);
                    float v = (n + 1.0f) * 0.5f;

                    // Wood plank lines
                    bool onPlankLine = (y % 64 < 2);

                    float r, g, b;
                    if (onPlankLine)
                    {
                        r = 0.2f; g = 0.1f; b = 0.05f;
                    }
                    else
                    {
                        // Wood grain
                        float grain = noise.GetNoise2D(x * 3, y * 0.5f) * 0.15f;
                        r = 0.45f + v * 0.1f + grain;
                        g = 0.25f + v * 0.06f + grain * 0.5f;
                        b = 0.1f + v * 0.03f;
                    }

                    image.SetPixel(x, y, new Color(r, g, b));
                }
            }

            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Chest wood texture.
        /// </summary>
        public static ImageTexture CreateChestTexture()
        {
            var image = Image.CreateEmpty(TexSize, TexSize, false, Image.Format.Rgba8);
            var noise = new FastNoiseLite
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
                Frequency = 0.25f,
                Seed = 400
            };

            for (int x = 0; x < TexSize; x++)
            {
                for (int y = 0; y < TexSize; y++)
                {
                    float n = noise.GetNoise2D(x, y);
                    float v = (n + 1.0f) * 0.5f;
                    float grain = noise.GetNoise2D(x * 4, y * 0.3f) * 0.12f;

                    float r = 0.5f + v * 0.08f + grain;
                    float g = 0.3f + v * 0.05f + grain * 0.5f;
                    float b = 0.12f + v * 0.02f;

                    image.SetPixel(x, y, new Color(r, g, b));
                }
            }

            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Stone stairs texture.
        /// </summary>
        public static ImageTexture CreateStairsTexture()
        {
            var image = Image.CreateEmpty(TexSize, TexSize, false, Image.Format.Rgba8);
            var noise = new FastNoiseLite
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
                Frequency = 0.12f,
                Seed = 500
            };

            for (int x = 0; x < TexSize; x++)
            {
                for (int y = 0; y < TexSize; y++)
                {
                    float n = noise.GetNoise2D(x, y);
                    float v = (n + 1.0f) * 0.5f;

                    float r = 0.4f + v * 0.12f;
                    float g = 0.38f + v * 0.1f;
                    float b = 0.35f + v * 0.08f;

                    image.SetPixel(x, y, new Color(r, g, b));
                }
            }

            return ImageTexture.CreateFromImage(image);
        }
    }
}
