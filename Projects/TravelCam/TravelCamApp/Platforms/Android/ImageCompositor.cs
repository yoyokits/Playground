// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokitos       //
// ========================================== //

using System;
using System.Collections.Generic;
using AGBitmap  = Android.Graphics.Bitmap;
using AGCanvas  = Android.Graphics.Canvas;
using AGColor   = Android.Graphics.Color;
using AGMatrix  = Android.Graphics.Matrix;
using AGPaint   = Android.Graphics.Paint;
using AGRectF   = Android.Graphics.RectF;
using PaintFlags = Android.Graphics.PaintFlags;

namespace TravelCamApp.Helpers
{
    /// <summary>
    /// Composites the DataOverlay pill onto a copy of an original photo at full resolution.
    /// Mirrors the XAML layout exactly:
    ///   • Background: #99000000, RoundRectangle 10dp, Padding="10,10"
    ///   • Label: #9E9E9E, uppercase, LabelFontSize (= FontSize × 0.70)
    ///   • Value: #FFFFFF, bold, ValueFontSize (= FontSize)
    ///   • VerticalStackLayout Spacing="4" between items, Spacing="0" within each item
    ///   • Pill anchored bottom-right with the same EdgePad used by UpdateOverlayPositionAsync
    ///
    /// All sizes are scaled proportionally from screen dp to image pixels:
    ///   dpScale = imageWidth / 360 — mirrors a 360dp reference screen width.
    /// </summary>
    public static class ImageCompositor
    {
        private const float ReferenceDp = 360f; // dp width of reference phone screen

        /// <summary>
        /// Loads <paramref name="sourcePath"/>, draws the overlay on a mutable copy, and
        /// saves the result as a JPEG at 95% quality to a temp file in CacheDir.
        /// Returns the temp file path, or empty string on failure.
        /// Falls back gracefully: if items list is empty returns empty string (caller shares original).
        /// </summary>
        public static string CompositeAndSave(
            string sourcePath,
            IReadOnlyList<(string Label, string Value)> items,
            float overlayFontSizeSp)
        {
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                return string.Empty;

            if (items == null || items.Count == 0)
                return string.Empty;

            AGBitmap? src = null;
            AGBitmap? mutable = null;

            try
            {
                // ── 1. Load at full resolution with EXIF orientation applied ──────────
                src = LoadOrientedBitmap(sourcePath);
                if (src == null) return string.Empty;

                int imgW = src.Width;
                int imgH = src.Height;

                // ── 2. Create mutable copy ────────────────────────────────────────────
                mutable = src.Copy(AGBitmap.Config.Argb8888!, true)!;
                src.Recycle();
                src = null;

                var canvas = new AGCanvas(mutable);

                // ── 3. Scale dp values to image pixels ───────────────────────────────
                float dpScale   = imgW / ReferenceDp;
                float valueSize = overlayFontSizeSp * dpScale;      // ValueFontSize
                float labelSize = valueSize * 0.70f;                 // LabelFontSize
                float padding   = 10f * dpScale;                     // XAML Padding="10,10"
                float corner    = 10f * dpScale;                     // XAML RoundRectangle 10
                float itemGap   = 4f  * dpScale;                     // XAML Spacing="4" between items
                float edgePad   = 12f * dpScale;                     // matches ImageViewerView EdgePad=12

                // ── 4. Build Paints ───────────────────────────────────────────────────
                using var labelPaint = new AGPaint(PaintFlags.AntiAlias)
                {
                    TextSize = labelSize,
                    Color    = new AGColor(0x9E, 0x9E, 0x9E, 0xFF)
                };
                using var valuePaint = new AGPaint(PaintFlags.AntiAlias)
                {
                    TextSize     = valueSize,
                    FakeBoldText = true,
                    Color        = AGColor.White
                };
                using var bgPaint = new AGPaint(PaintFlags.AntiAlias)
                {
                    Color = new AGColor(0x00, 0x00, 0x00, 0x99)
                };

                var lm = labelPaint.GetFontMetrics()!;
                var vm = valuePaint.GetFontMetrics()!;

                float labelLineH = -lm.Top + lm.Bottom;
                float valueLineH = -vm.Top + vm.Bottom;

                // ── 5. Measure pill ───────────────────────────────────────────────────
                float maxTextW = 0f;
                foreach (var (label, value) in items)
                {
                    maxTextW = Math.Max(maxTextW,
                        Math.Max(labelPaint.MeasureText(label.ToUpperInvariant()),
                                 valuePaint.MeasureText(value)));
                }

                float pillW = maxTextW + padding * 2f;
                float pillH = padding
                              + (labelLineH + valueLineH) * items.Count
                              + itemGap * (items.Count - 1)
                              + padding;

                float pillRight  = imgW - edgePad;
                float pillBottom = imgH - edgePad;
                float pillLeft   = pillRight  - pillW;
                float pillTop    = pillBottom - pillH;

                // ── 6. Draw pill background ───────────────────────────────────────────
                var rect = new AGRectF(pillLeft, pillTop, pillRight, pillBottom);
                canvas.DrawRoundRect(rect, corner, corner, bgPaint);

                // ── 7. Draw each overlay item ─────────────────────────────────────────
                float textX = pillLeft + padding;
                float y     = pillTop  + padding;

                foreach (var (label, value) in items)
                {
                    // Label (baseline = y - lm.Top = y + |ascent|)
                    canvas.DrawText(label.ToUpperInvariant(), textX, y - lm.Top, labelPaint);
                    y += labelLineH;

                    // Value (Spacing="0" within item — no gap between label and value)
                    canvas.DrawText(value, textX, y - vm.Top, valuePaint);
                    y += valueLineH;

                    // Gap between items (Spacing="4")
                    y += itemGap;
                }

                // ── 8. Compress to temp JPEG (95% quality = visually lossless) ───────
                var cacheDir = Android.App.Application.Context.CacheDir!.AbsolutePath;
                var outPath  = System.IO.Path.Combine(cacheDir, "share_composite.jpg");

                using var fs = System.IO.File.Open(outPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                mutable.Compress(AGBitmap.CompressFormat.Jpeg!, 95, fs);
                fs.Flush();

                System.Diagnostics.Debug.WriteLine(
                    $"[ImageCompositor] Composite saved: {imgW}×{imgH}, " +
                    $"{items.Count} items, dpScale={dpScale:F2}, " +
                    $"size={new System.IO.FileInfo(outPath).Length / 1024} KB");

                return outPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCompositor] Error: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                src?.Recycle();
                mutable?.Recycle();
            }
        }

        /// <summary>
        /// Decodes a JPEG from disk and rotates it according to its EXIF orientation tag,
        /// producing an upright bitmap regardless of how the phone was held during capture.
        /// </summary>
        private static AGBitmap? LoadOrientedBitmap(string filePath)
        {
            var bitmap = Android.Graphics.BitmapFactory.DecodeFile(filePath);
            if (bitmap == null) return null;

            int orientation;
            try
            {
                using var exif = new Android.Media.ExifInterface(filePath);
                orientation = exif.GetAttributeInt(
                    Android.Media.ExifInterface.TagOrientation,
                    (int)Android.Media.Orientation.Normal);
            }
            catch
            {
                return bitmap;
            }

            int degrees = orientation switch
            {
                (int)Android.Media.Orientation.Rotate90  => 90,
                (int)Android.Media.Orientation.Rotate180 => 180,
                (int)Android.Media.Orientation.Rotate270 => 270,
                _                                         => 0
            };

            if (degrees == 0) return bitmap;

            var matrix = new AGMatrix();
            matrix.PostRotate(degrees);
            var rotated = AGBitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
            bitmap.Recycle();
            return rotated;
        }
    }
}
