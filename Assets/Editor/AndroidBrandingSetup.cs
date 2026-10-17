using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Keeps Android branding assets valid and reproducible without changing the source artwork.
/// Adaptive icon layers must be Texture2D assets; Unity splash backgrounds remain Sprites.
/// </summary>
[InitializeOnLoad]
internal sealed class AndroidBrandingSetup : IPreprocessBuildWithReport
{
    private const string SourceIconPath = "Assets/Sprites/App/APP Icon.png";
    private const string GeneratedDirectory = "Assets/Sprites/App/Generated";
    private const string ForegroundPath = GeneratedDirectory + "/APP Icon Adaptive Foreground.png";
    private const string BackgroundPath = GeneratedDirectory + "/APP Icon Adaptive Background.png";

    private const int AdaptiveTextureSize = 1024;

    // Android's guaranteed safe zone is circular. A 46% square fits inside that circle,
    // keeping the complete source artwork visible under circular and rounded-square masks.
    private const float ForegroundScale = 0.46f;

    static AndroidBrandingSetup()
    {
        EditorApplication.delayCall += Apply;
    }

    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
        {
            Apply();
        }
    }

    [MenuItem("Sudoku DASH/Apply Android Branding")]
    private static void Apply()
    {
        Texture2D source = LoadSourceTexture();
        if (source == null)
        {
            Debug.LogError($"Android branding source icon was not found at '{SourceIconPath}'.");
            return;
        }

        Directory.CreateDirectory(GeneratedDirectory);

        Color32 backgroundColor = SampleCornerColor(source);
        Texture2D foreground = CreateForeground(source, backgroundColor);
        Texture2D background = CreateSolidTexture(backgroundColor);

        WritePng(ForegroundPath, foreground);
        WritePng(BackgroundPath, background);

        Object.DestroyImmediate(source);
        Object.DestroyImmediate(foreground);
        Object.DestroyImmediate(background);

        AssetDatabase.ImportAsset(ForegroundPath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(BackgroundPath, ImportAssetOptions.ForceSynchronousImport);
        ConfigureIconImporter(ForegroundPath, true);
        ConfigureIconImporter(BackgroundPath, false);

        Texture2D foregroundAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(ForegroundPath);
        Texture2D backgroundAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
        ApplyAdaptiveIcons(foregroundAsset, backgroundAsset);
    }

    private static Texture2D LoadSourceTexture()
    {
        if (!File.Exists(SourceIconPath))
        {
            return null;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        return texture.LoadImage(File.ReadAllBytes(SourceIconPath), false) ? texture : null;
    }

    private static Texture2D CreateForeground(Texture2D source, Color32 backgroundColor)
    {
        var result = NewTexture();
        Color32[] transparent = new Color32[AdaptiveTextureSize * AdaptiveTextureSize];
        result.SetPixels32(transparent);

        int contentSize = Mathf.RoundToInt(AdaptiveTextureSize * ForegroundScale);
        int offset = (AdaptiveTextureSize - contentSize) / 2;
        Color[] scaled = Resize(source, contentSize, contentSize);

        // Pixels matching the artwork's edge color become transparent so the independently
        // supplied adaptive background extends cleanly beyond the foreground safe zone.
        for (int i = 0; i < scaled.Length; i++)
        {
            Color pixel = scaled[i];
            float distance = Mathf.Max(
                Mathf.Abs(pixel.r - backgroundColor.r / 255f),
                Mathf.Max(
                    Mathf.Abs(pixel.g - backgroundColor.g / 255f),
                    Mathf.Abs(pixel.b - backgroundColor.b / 255f)));

            if (distance < 0.025f)
            {
                pixel.a = 0f;
                scaled[i] = pixel;
            }
        }

        result.SetPixels(offset, offset, contentSize, contentSize, scaled);
        result.Apply(false, false);
        return result;
    }

    private static Texture2D CreateSolidTexture(Color32 color)
    {
        var result = NewTexture();
        var pixels = new Color32[AdaptiveTextureSize * AdaptiveTextureSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        result.SetPixels32(pixels);
        result.Apply(false, false);
        return result;
    }

    private static Texture2D NewTexture()
    {
        return new Texture2D(AdaptiveTextureSize, AdaptiveTextureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
    }

    private static Color[] Resize(Texture2D source, int width, int height)
    {
        var result = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            float v = (y + 0.5f) / height;
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width;
                result[y * width + x] = source.GetPixelBilinear(u, v);
            }
        }

        return result;
    }

    private static Color32 SampleCornerColor(Texture2D source)
    {
        Color32[] corners =
        {
            source.GetPixel(0, 0),
            source.GetPixel(source.width - 1, 0),
            source.GetPixel(0, source.height - 1),
            source.GetPixel(source.width - 1, source.height - 1)
        };

        int red = 0;
        int green = 0;
        int blue = 0;
        foreach (Color32 corner in corners)
        {
            red += corner.r;
            green += corner.g;
            blue += corner.b;
        }

        return new Color32((byte)(red / 4), (byte)(green / 4), (byte)(blue / 4), 255);
    }

    private static void WritePng(string path, Texture2D texture)
    {
        byte[] png = texture.EncodeToPNG();
        if (!File.Exists(path) || !BytesEqual(File.ReadAllBytes(path), png))
        {
            File.WriteAllBytes(path, png);
        }
    }

    private static bool BytesEqual(byte[] first, byte[] second)
    {
        if (first.Length != second.Length)
        {
            return false;
        }

        for (int i = 0; i < first.Length; i++)
        {
            if (first[i] != second[i])
            {
                return false;
            }
        }

        return true;
    }

    private static void ConfigureIconImporter(string path, bool hasAlpha)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        bool changed = importer.textureType != TextureImporterType.Default
            || importer.alphaIsTransparency != hasAlpha
            || importer.mipmapEnabled;

        if (!changed)
        {
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = hasAlpha;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }

    private static void ApplyAdaptiveIcons(Texture2D foreground, Texture2D background)
    {
        if (foreground == null || background == null)
        {
            Debug.LogError("Generated Android adaptive icon textures could not be imported.");
            return;
        }

        PlatformIconKind kind = AndroidPlatformIconKind.Adaptive;
        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
        bool changed = false;

        foreach (PlatformIcon icon in icons)
        {
            // Unity's Android adaptive icon layers are background first, foreground second.
            // Putting the opaque background in layer 1 covers the complete icon artwork.
            if (icon.GetTexture(0) != background)
            {
                icon.SetTexture(background, 0);
                changed = true;
            }

            if (icon.GetTexture(1) != foreground)
            {
                icon.SetTexture(foreground, 1);
                changed = true;
            }
        }

        if (changed)
        {
            PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, kind, icons);
            AssetDatabase.SaveAssets();
            Debug.Log("Applied Android adaptive icon safe-zone foreground and solid background.");
        }
    }
}
