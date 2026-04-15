// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokitos       //
// ========================================== //
//
// ExifHelper (Android): Reads and writes JPEG EXIF metadata.
//
// ApplyMetadata — embeds PhotoCaptureMetadata into a JPEG stream.
//   Writes standard EXIF tags (GPS, date, device) plus a JSON
//   UserComment payload for custom fields (temperature, city, etc.).
//
// ReadMetadata — reads a JPEG file and returns a MediaInfo object
//   for display in the gallery info panel.
//
// This file is in Platforms/Android so it is compiled only for Android;
// no #if ANDROID guards are needed.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Android.Media;
using TravelCamApp.Models;
// Alias to resolve ambiguity with Android.Media.Stream / Android.Media.MediaInfo
using MediaInfo = TravelCamApp.Models.MediaInfo;
using Stream = System.IO.Stream;

namespace TravelCamApp.Helpers
{
    internal static class ExifHelper
    {
        // ── Write ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Embeds <see cref="PhotoCaptureMetadata"/> into a JPEG stream as EXIF tags.
        /// The input stream is read from position 0 and is not disposed.
        /// Returns a new MemoryStream (position 0) with EXIF written.
        /// </summary>
        public static MemoryStream ApplyMetadata(Stream inputStream, PhotoCaptureMetadata meta)
        {
            var tmpPath = Path.Combine(
                Android.App.Application.Context.CacheDir!.AbsolutePath,
                $"exif_{Guid.NewGuid():N}.jpg");
            try
            {
                // 1. Write JPEG bytes to a temp file (ExifInterface needs a file path)
                inputStream.Position = 0;
                using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write))
                    inputStream.CopyTo(fs);

                // 2. Open ExifInterface on the file and write all tags
                var exif = new ExifInterface(tmpPath);

                // ── Date/time ──────────────────────────────────────────────────
                var now = DateTime.UtcNow;
                var exifDateTime = now.ToString("yyyy:MM:dd HH:mm:ss");
                exif.SetAttribute(ExifInterface.TagDatetimeOriginal, exifDateTime);
                exif.SetAttribute(ExifInterface.TagDatetime, exifDateTime);

                // ── GPS coordinates ────────────────────────────────────────────
                SetExifGpsLatLon(exif, meta.Latitude, meta.Longitude);

                // ── GPS altitude ───────────────────────────────────────────────
                if (meta.Altitude.HasValue)
                {
                    exif.SetAttribute(ExifInterface.TagGpsAltitude,
                        ToRational(Math.Abs(meta.Altitude.Value)));
                    // 0 = above sea level, 1 = below
                    exif.SetAttribute(ExifInterface.TagGpsAltitudeRef,
                        meta.Altitude.Value < 0 ? "1" : "0");
                }

                // ── GPS timestamp ──────────────────────────────────────────────
                exif.SetAttribute(ExifInterface.TagGpsTimestamp,
                    $"{now.Hour:D2}/1,{now.Minute:D2}/1,{now.Second:D2}/1");
                // GPS date stamp: "GPSDateStamp" tag
                exif.SetAttribute("GPSDateStamp", now.ToString("yyyy:MM:dd"));

                // ── Compass heading (GPSImgDirection) ──────────────────────────
                if (meta.Heading.HasValue)
                {
                    exif.SetAttribute("GPSImgDirection", ToRational(meta.Heading.Value));
                    exif.SetAttribute("GPSImgDirectionRef", "T"); // T = true north
                }

                // ── GPS speed ──────────────────────────────────────────────────
                if (meta.SpeedMps.HasValue)
                {
                    // GPSSpeedRef "K" = km/h
                    exif.SetAttribute("GPSSpeed", ToRational(meta.SpeedMps.Value * 3.6));
                    exif.SetAttribute("GPSSpeedRef", "K");
                }

                // ── Device info ────────────────────────────────────────────────
                exif.SetAttribute(ExifInterface.TagMake, "CekliCam");
                exif.SetAttribute(ExifInterface.TagModel,
                    $"{Android.OS.Build.Manufacturer} {Android.OS.Build.Model}");
                exif.SetAttribute(ExifInterface.TagSoftware, "TravelCam");

                // ── Flash ──────────────────────────────────────────────────────
                // EXIF flash tag: 0x00 = no flash, 0x01 = flash fired
                exif.SetAttribute(ExifInterface.TagFlash, meta.FlashFired ? "1" : "0");

                // ── Image description (human-readable location) ────────────────
                var location = BuildLocationDescription(meta);
                if (!string.IsNullOrEmpty(location))
                    exif.SetAttribute(ExifInterface.TagImageDescription, location);

                // ── UserComment: JSON payload for all custom fields ─────────────
                // Prefixed with "JSON:" so ReadMetadata can identify and parse it.
                exif.SetAttribute(ExifInterface.TagUserComment,
                    "JSON:" + BuildUserCommentJson(meta, now));

                exif.SaveAttributes();

                // 3. Read the modified file back into a MemoryStream
                var ms = new MemoryStream();
                using (var readFs = new FileStream(tmpPath, FileMode.Open, FileAccess.Read))
                    readFs.CopyTo(ms);
                ms.Position = 0;
                return ms;
            }
            finally
            {
                try { File.Delete(tmpPath); } catch { }
            }
        }

        // ── Read ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads EXIF metadata from a JPEG file and returns a <see cref="MediaInfo"/>.
        /// Returns an empty MediaInfo (with FileName set) when the file is a video
        /// or when EXIF cannot be read.
        /// </summary>
        public static MediaInfo ReadMetadata(string filePath)
        {
            var info = new MediaInfo();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return info;

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            info.IsVideo = ext == ".mp4";
            info.FileName = Path.GetFileName(filePath);

            try
            {
                info.FileSizeText = FormatFileSize(new FileInfo(filePath).Length);
            }
            catch { }

            if (info.IsVideo) return info; // MP4 has no EXIF

            try
            {
                var exif = new ExifInterface(filePath);

                // ── Capture date ───────────────────────────────────────────────
                var dateStr = exif.GetAttribute(ExifInterface.TagDatetimeOriginal)
                           ?? exif.GetAttribute(ExifInterface.TagDatetime);
                if (!string.IsNullOrEmpty(dateStr) &&
                    DateTime.TryParseExact(dateStr, "yyyy:MM:dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var captureDate))
                {
                    info.CaptureDateText = captureDate.ToString("MMM dd, yyyy  HH:mm:ss");
                }

                // ── Device ─────────────────────────────────────────────────────
                info.DeviceMake  = exif.GetAttribute(ExifInterface.TagMake)  ?? string.Empty;
                info.DeviceModel = exif.GetAttribute(ExifInterface.TagModel) ?? string.Empty;
                info.FlashText   = (exif.GetAttributeInt(ExifInterface.TagFlash, 0) & 0x01) == 1
                    ? "Fired" : "Off";

                // ── GPS coordinates ────────────────────────────────────────────
                var latDms = exif.GetAttribute(ExifInterface.TagGpsLatitude);
                var latRef = exif.GetAttribute(ExifInterface.TagGpsLatitudeRef);
                var lonDms = exif.GetAttribute(ExifInterface.TagGpsLongitude);
                var lonRef = exif.GetAttribute(ExifInterface.TagGpsLongitudeRef);

                if (latDms != null && latRef != null && lonDms != null && lonRef != null)
                {
                    var lat = ParseDms(latDms);
                    var lon = ParseDms(lonDms);
                    if (latRef == "S") lat = -lat;
                    if (lonRef == "W") lon = -lon;
                    info.GpsCoordsText = $"{lat:F6}°, {lon:F6}°";
                }

                // ── GPS altitude ───────────────────────────────────────────────
                var altStr = exif.GetAttribute(ExifInterface.TagGpsAltitude);
                var altRef = exif.GetAttribute(ExifInterface.TagGpsAltitudeRef);
                if (altStr != null)
                {
                    var altVal = ParseRational(altStr);
                    if (altRef == "1") altVal = -altVal;
                    info.AltitudeText = $"{altVal:F0} m";
                }

                // ── Pixel resolution from EXIF ─────────────────────────────────
                var imgW = exif.GetAttributeInt(ExifInterface.TagImageWidth, 0);
                var imgH = exif.GetAttributeInt(ExifInterface.TagImageLength, 0);
                if (imgW > 0 && imgH > 0)
                    info.ResolutionText = $"{imgW} × {imgH}";

                // ── UserComment JSON payload ────────────────────────────────────
                var userComment = exif.GetAttribute(ExifInterface.TagUserComment) ?? string.Empty;
                if (userComment.StartsWith("JSON:", StringComparison.Ordinal))
                    ParseUserCommentJson(userComment.Substring(5), info);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ExifHelper] ReadMetadata error: {ex.Message}");
            }

            return info;
        }

        // ── GPS helpers ───────────────────────────────────────────────────────

        private static void SetExifGpsLatLon(ExifInterface exif, double lat, double lon)
        {
            exif.SetAttribute(ExifInterface.TagGpsLatitudeRef, lat >= 0 ? "N" : "S");
            exif.SetAttribute(ExifInterface.TagGpsLatitude, ToDms(Math.Abs(lat)));
            exif.SetAttribute(ExifInterface.TagGpsLongitudeRef, lon >= 0 ? "E" : "W");
            exif.SetAttribute(ExifInterface.TagGpsLongitude, ToDms(Math.Abs(lon)));
        }

        /// <summary>
        /// Converts decimal degrees to EXIF DMS rational string:
        /// "degrees/1,minutes/1,seconds_thousandths/1000"
        /// </summary>
        private static string ToDms(double degrees)
        {
            var d = (int)degrees;
            var mRem = (degrees - d) * 60.0;
            var m = (int)mRem;
            var sNumer = (long)Math.Round((mRem - m) * 60.0 * 1000.0);
            return $"{d}/1,{m}/1,{sNumer}/1000";
        }

        /// <summary>
        /// Parses EXIF DMS rational string "dd/1,mm/1,ss*1000/1000" back to decimal degrees.
        /// </summary>
        private static double ParseDms(string dms)
        {
            try
            {
                var parts = dms.Split(',');
                if (parts.Length != 3) return 0;
                var d = ParseRational(parts[0].Trim());
                var m = ParseRational(parts[1].Trim());
                var s = ParseRational(parts[2].Trim());
                return d + m / 60.0 + s / 3600.0;
            }
            catch { return 0; }
        }

        /// <summary>Parses "numerator/denominator" rational string to double.</summary>
        private static double ParseRational(string rat)
        {
            var idx = rat.IndexOf('/');
            if (idx < 0)
            {
                return double.TryParse(rat,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
            }
            var num = double.Parse(rat.Substring(0, idx).Trim(),
                System.Globalization.CultureInfo.InvariantCulture);
            var den = double.Parse(rat.Substring(idx + 1).Trim(),
                System.Globalization.CultureInfo.InvariantCulture);
            return den == 0 ? 0 : num / den;
        }

        /// <summary>Converts a double to "numer/1000" EXIF rational string.</summary>
        private static string ToRational(double value)
        {
            var numer = (long)Math.Round(value * 1000.0);
            return $"{numer}/1000";
        }

        // ── JSON helpers ──────────────────────────────────────────────────────

        private static string BuildUserCommentJson(PhotoCaptureMetadata meta, DateTime captureTime)
        {
            // Use Utf8JsonWriter for a compact, allocation-light write
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms);
            writer.WriteStartObject();
            writer.WriteString("ts",      captureTime.ToString("o"));
            if (meta.Temperature.HasValue) writer.WriteNumber("temp",    meta.Temperature.Value);
            if (!string.IsNullOrEmpty(meta.City))    writer.WriteString("city",    meta.City!);
            if (!string.IsNullOrEmpty(meta.Country)) writer.WriteString("country", meta.Country!);
            if (meta.Heading.HasValue)  writer.WriteNumber("heading", meta.Heading.Value);
            if (meta.SpeedMps.HasValue) writer.WriteNumber("speed",   meta.SpeedMps.Value);
            writer.WriteBoolean("flash",  meta.FlashFired);
            writer.WriteString("aspect",  meta.AspectRatioLabel);
            writer.WriteString("res",     meta.ResolutionLabel);
            writer.WriteEndObject();
            writer.Flush();
            return System.Text.Encoding.UTF8.GetString(ms.ToArray());
        }

        private static void ParseUserCommentJson(string json, MediaInfo info)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("temp", out var el) && el.ValueKind == JsonValueKind.Number)
                    info.TemperatureText = $"{el.GetDouble():F1}°C";

                if (root.TryGetProperty("city", out el) && el.ValueKind == JsonValueKind.String)
                    info.CityText = el.GetString() ?? string.Empty;

                if (root.TryGetProperty("country", out el) && el.ValueKind == JsonValueKind.String)
                    info.CountryText = el.GetString() ?? string.Empty;

                if (root.TryGetProperty("heading", out el) && el.ValueKind == JsonValueKind.Number)
                    info.HeadingText = $"{el.GetDouble():F0}°";

                if (root.TryGetProperty("speed", out el) && el.ValueKind == JsonValueKind.Number)
                    info.SpeedText = $"{el.GetDouble() * 3.6:F1} km/h";

                if (root.TryGetProperty("aspect", out el) && el.ValueKind == JsonValueKind.String)
                    info.AspectRatioText = el.GetString() ?? string.Empty;

                // Only set resolution from JSON if EXIF pixel dimensions were not available
                if (string.IsNullOrEmpty(info.ResolutionText) &&
                    root.TryGetProperty("res", out el) && el.ValueKind == JsonValueKind.String)
                    info.ResolutionText = el.GetString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ExifHelper] ParseUserCommentJson error: {ex.Message}");
            }
        }

        // ── String helpers ────────────────────────────────────────────────────

        private static string BuildLocationDescription(PhotoCaptureMetadata meta)
        {
            var parts = new List<string>(2);
            if (!string.IsNullOrEmpty(meta.City))    parts.Add(meta.City!);
            if (!string.IsNullOrEmpty(meta.Country)) parts.Add(meta.Country!);
            return string.Join(", ", parts);
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024L)             return $"{bytes} B";
            if (bytes < 1024L * 1024)      return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }
}
