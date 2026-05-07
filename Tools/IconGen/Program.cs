// One-shot generator that writes Assets/app.ico (multi-resolution).
// Run via: dotnet run --project Tools/IconGen
//
// The drawing here matches the tray-icon design and is the single source of truth
// for the app icon. Re-run after edits to refresh the .ico.

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

int[] sizes = [16, 24, 32, 48, 64, 128, 256];

// Locate repo root: this file lives at <repo>/Tools/IconGen/Program.cs
var here = AppContext.BaseDirectory;
var repoRoot = Path.GetFullPath(Path.Combine(here, "..", "..", "..", "..", ".."));
// Fall back: walk up until we find WindowsPinboard.csproj
while (!File.Exists(Path.Combine(repoRoot, "WindowsPinboard.csproj")))
{
    var parent = Path.GetDirectoryName(repoRoot);
    if (parent == null || parent == repoRoot)
    {
        Console.Error.WriteLine("Could not locate repo root (WindowsPinboard.csproj).");
        return 1;
    }
    repoRoot = parent;
}

var outDir = Path.Combine(repoRoot, "Assets");
Directory.CreateDirectory(outDir);
var outPath = Path.Combine(outDir, "app.ico");

var pngs = new List<byte[]>();
foreach (var size in sizes)
{
    using var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(bmp))
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.CompositingQuality = CompositingQuality.HighQuality;
        DrawPinboardIcon(g, size);
    }
    using var ms = new MemoryStream();
    bmp.Save(ms, ImageFormat.Png);
    pngs.Add(ms.ToArray());
}

using (var fs = File.Create(outPath))
using (var bw = new BinaryWriter(fs))
{
    // ICONDIR
    bw.Write((short)0);          // reserved
    bw.Write((short)1);          // type: 1 = ICO
    bw.Write((short)sizes.Length); // count

    int dataOffset = 6 + 16 * sizes.Length;
    for (int i = 0; i < sizes.Length; i++)
    {
        int s = sizes[i];
        bw.Write((byte)(s == 256 ? 0 : s)); // width  (0 = 256)
        bw.Write((byte)(s == 256 ? 0 : s)); // height (0 = 256)
        bw.Write((byte)0);                  // color count
        bw.Write((byte)0);                  // reserved
        bw.Write((short)1);                 // planes
        bw.Write((short)32);                // bit count
        bw.Write((int)pngs[i].Length);      // bytes in res
        bw.Write((int)dataOffset);          // image offset
        dataOffset += pngs[i].Length;
    }

    foreach (var png in pngs) bw.Write(png);
}

Console.WriteLine($"Wrote {outPath}");
foreach (var s in sizes) Console.WriteLine($"  {s}x{s}");
return 0;


static void DrawPinboardIcon(Graphics g, int size)
{
    g.Clear(Color.Transparent);

    // Outer rounded panel
    var inset = size * 0.08f;
    var panelRect = new RectangleF(inset, inset, size - inset * 2, size - inset * 2);
    var radius = MathF.Max(2f, size * 0.12f);

    // Subtle shadow for larger sizes
    if (size >= 48)
    {
        using var shadow = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
        var shadowRect = new RectangleF(panelRect.X + size * 0.02f, panelRect.Y + size * 0.03f,
                                         panelRect.Width, panelRect.Height);
        using var sp = RoundedRect(shadowRect, radius);
        g.FillPath(shadow, sp);
    }

    using (var bg = new SolidBrush(Color.FromArgb(255, 31, 31, 35)))
    using (var path = RoundedRect(panelRect, radius))
    {
        g.FillPath(bg, path);
    }

    // Sliver (handle) on right edge of panel
    using (var sliver = new SolidBrush(Color.FromArgb(255, 111, 168, 255)))
    {
        var sliverWidth = MathF.Max(2f, size * 0.07f);
        var sliverRect = new RectangleF(
            panelRect.Right - sliverWidth - size * 0.05f,
            panelRect.Top + size * 0.10f,
            sliverWidth,
            panelRect.Height - size * 0.20f);
        var sliverRadius = MathF.Min(sliverWidth / 2f, size * 0.05f);
        using var sp = RoundedRect(sliverRect, sliverRadius);
        g.FillPath(sliver, sp);
    }

    // "Note lines" — three horizontal pills on the left side of the panel
    using (var line = new SolidBrush(Color.FromArgb(255, 182, 187, 196)))
    {
        float lineHeight = MathF.Max(1f, size * 0.07f);
        float lineX = panelRect.Left + size * 0.14f;
        float lineWidth = panelRect.Width * 0.45f;
        float startY = panelRect.Top + size * 0.18f;
        float gap = size * 0.18f;

        for (int i = 0; i < 3; i++)
        {
            float y = startY + i * gap;
            // Slightly shorter middle / last line for visual interest at large sizes.
            float w = lineWidth * (i == 1 ? 0.85f : i == 2 ? 0.65f : 1.0f);
            var lineRect = new RectangleF(lineX, y, w, lineHeight);
            using var lp = RoundedRect(lineRect, lineHeight / 2f);
            g.FillPath(line, lp);
        }
    }
}

static GraphicsPath RoundedRect(RectangleF rect, float radius)
{
    var path = new GraphicsPath();
    if (radius <= 0)
    {
        path.AddRectangle(rect);
        return path;
    }
    var d = radius * 2f;
    path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
    path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
    path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
    path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
    path.CloseFigure();
    return path;
}
