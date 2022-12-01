namespace SolidUtilities
{
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>Different useful extensions for <see cref="Texture2D"/>.</summary>
    public static class Texture2DExtensions
    {
        /// <summary>Rotates the input texture by 90 degrees and returns the new rotated texture.</summary>
        /// <param name="texture">Texture to rotate.</param>
        /// <param name="clockwise">Whether to rotate the texture clockwise.</param>
        /// <returns>The rotated texture.</returns>
        /// <example><code>EditorIcon TriangleDown = new EditorIcon(Database.TriangleRight.Rotate());</code></example>
        [PublicAPI] public static Texture2D Rotate(this Texture2D texture, bool clockwise = true)
        {
            var original = texture.GetPixels32();
            var rotated = new Color32[original.Length];
            int textureWidth = texture.width;
            int textureHeight = texture.height;
            int origLength = original.Length;

            for (int heightIndex = 0; heightIndex < textureHeight; ++heightIndex)
            {
                for (int widthIndex = 0; widthIndex < textureWidth; ++widthIndex)
                {
                    int rotIndex = (widthIndex + 1) * textureHeight - heightIndex - 1;

                    int origIndex = clockwise
                        ? origLength - 1 - (heightIndex * textureWidth + widthIndex)
                        : heightIndex * textureWidth + widthIndex;

                    rotated[rotIndex] = original[origIndex];
                }
            }

            var rotatedTexture = new Texture2D(textureHeight, textureWidth);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        /// <summary>Draws the texture in a given rect.</summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="rect">Rectangle in which to draw the texture.</param>
        /// <example><code>tintedIcon.Draw(triangleRect);</code></example>
        [PublicAPI] public static void Draw(this Texture2D texture, Rect rect) => GUI.DrawTexture(rect, texture);

        /// <summary>Duplicates a texture2D. The copy is readable.</summary>
        /// <param name="texture">The texture to copy.</param>
        [PublicAPI] public static Texture2D Copy(this Texture2D texture)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary( 
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            // "myTexture2D" now has the same pixels from "texture" and it's readable
            return myTexture2D;
        }
    }
}