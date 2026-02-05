using Microsoft.Maui.Graphics;
using System;
using System.ComponentModel;

namespace TravelCamApp.Views
{
    public partial class CompassControl : ContentView
    {
        public static readonly BindableProperty DegreeProperty =
            BindableProperty.Create(
                nameof(Degree),
                typeof(double),
                typeof(CompassControl),
                default(double),
                BindingMode.TwoWay,
                propertyChanged: OnDegreeChanged);

        public double Degree
        {
            get => (double)GetValue(DegreeProperty);
            set => SetValue(DegreeProperty, value);
        }

        private static void OnDegreeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var compassControl = (CompassControl)bindable;
            compassControl.DegreeLabel.Text = $"{Math.Round((double)newValue)}°";
            compassControl.CompassGraphicsView.Invalidate();
        }

        public CompassControl()
        {
            InitializeComponent();
            CompassGraphicsView.Drawable = new CompassDrawable(this);
        }
    }

    internal class CompassDrawable : IDrawable
    {
        private readonly CompassControl _compassControl;

        public CompassDrawable(CompassControl compassControl)
        {
            _compassControl = compassControl;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var centerX = dirtyRect.Width / 2;
            var centerY = dirtyRect.Height / 2;
            var radius = (float)((Math.Min(centerX, centerY) - 8) * 0.88); // Material Design 3 spacing

            // Draw black background circle
            canvas.FillColor = Colors.Black;
            canvas.FillCircle(centerX, centerY, radius);

            // Draw outer white ring
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 3; // Thinner ring
            canvas.DrawCircle(centerX, centerY, radius);

            // Draw major ticks (every 90 degrees - N, E, S, W)
            for (int i = 0; i < 4; i++)
            {
                var angle = i * 90 - 90; // Start from top (0° = North)
                var radian = angle * Math.PI / 180.0;

                var startX = (float)(centerX + (radius - 10) * Math.Cos(radian));
                var startY = (float)(centerY + (radius - 10) * Math.Sin(radian));
                var endX = (float)(centerX + (radius - 2) * Math.Cos(radian));
                var endY = (float)(centerY + (radius - 2) * Math.Sin(radian));

                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = 3; // Thinner lines
                canvas.DrawLine(startX, startY, endX, endY);

                // Draw cardinal letters (N, E, S, W) with proper positioning
                var textRadius = radius - 25;
                var textX = (float)(centerX + textRadius * Math.Cos(radian));
                var textY = (float)(centerY + textRadius * Math.Sin(radian));

                string cardinalText;
                switch (i)
                {
                    case 0: cardinalText = "N"; break;  // North (top)
                    case 1: cardinalText = "E"; break;  // East (right)
                    case 2: cardinalText = "S"; break;  // South (bottom)
                    case 3: cardinalText = "W"; break;  // West (left)
                    default: cardinalText = ""; break;
                }

                canvas.FontColor = Colors.White;
                canvas.FontSize = 18; // Slightly larger and bolder
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                
                // Use a fixed size box for proper text centering
                float boxSize = 24;
                canvas.DrawString(cardinalText, textX - boxSize/2, textY - boxSize/2, boxSize, boxSize, HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            // Draw minor ticks (every 30 degrees, except major ticks)
            for (int i = 0; i < 12; i++)
            {
                if (i % 3 == 0) continue; // Skip major tick positions (every 90°)
                var angle = i * 30 - 90;
                var radian = angle * Math.PI / 180.0;

                var startX = (float)(centerX + (radius - 8) * Math.Cos(radian));
                var startY = (float)(centerY + (radius - 8) * Math.Sin(radian));
                var endX = (float)(centerX + (radius - 2) * Math.Cos(radian));
                var endY = (float)(centerY + (radius - 2) * Math.Sin(radian));

                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(startX, startY, endX, endY);
            }

            // Draw compass needle with improved design
            var needleAngle = _compassControl.Degree - 90; // Adjust for drawing orientation
            var needleRadian = needleAngle * Math.PI / 180.0;

            // North needle (red arrow) - more streamlined and elegant
            var needleLength = radius * 0.75;
            var needleWidth = radius * 0.12;
            
            // North arrow tip
            var northTipX = (float)(centerX + needleLength * Math.Cos(needleRadian));
            var northTipY = (float)(centerY + needleLength * Math.Sin(needleRadian));
            
            // Base points for north arrow
            var baseDistance = radius * 0.08;
            var northLeftX = (float)(centerX + baseDistance * Math.Cos(needleRadian + Math.PI / 2));
            var northLeftY = (float)(centerY + baseDistance * Math.Sin(needleRadian + Math.PI / 2));
            var northRightX = (float)(centerX + baseDistance * Math.Cos(needleRadian - Math.PI / 2));
            var northRightY = (float)(centerY + baseDistance * Math.Sin(needleRadian - Math.PI / 2));

            // Draw north needle with shadow for depth
            var northPath = new PathF();
            northPath.MoveTo(northTipX, northTipY);
            northPath.LineTo(northLeftX, northLeftY);
            northPath.LineTo(centerX, centerY);
            northPath.LineTo(northRightX, northRightY);
            northPath.Close();

            // Shadow effect
            canvas.FillColor = Color.FromRgba(0, 0, 0, 0.3);
            canvas.FillPath(northPath);
            
            // Main north needle (red with gradient effect)
            canvas.FillColor = Color.FromRgb(220, 50, 50); // Brighter red
            canvas.FillPath(northPath);
            
            // Border for definition
            canvas.StrokeColor = Color.FromRgb(180, 30, 30); // Darker red border
            canvas.StrokeSize = 1.5f;
            canvas.DrawPath(northPath);

            // South needle (white/gray arrow) - shorter and more subtle
            var southAngle = needleAngle + 180;
            var southRadian = southAngle * Math.PI / 180.0;
            var southLength = needleLength * 0.65;
            
            var southTipX = (float)(centerX + southLength * Math.Cos(southRadian));
            var southTipY = (float)(centerY + southLength * Math.Sin(southRadian));
            
            // Base points for south arrow
            var southLeftX = (float)(centerX + baseDistance * Math.Cos(southRadian + Math.PI / 2));
            var southLeftY = (float)(centerY + baseDistance * Math.Sin(southRadian + Math.PI / 2));
            var southRightX = (float)(centerX + baseDistance * Math.Cos(southRadian - Math.PI / 2));
            var southRightY = (float)(centerY + baseDistance * Math.Sin(southRadian - Math.PI / 2));

            var southPath = new PathF();
            southPath.MoveTo(southTipX, southTipY);
            southPath.LineTo(southLeftX, southLeftY);
            southPath.LineTo(centerX, centerY);
            southPath.LineTo(southRightX, southRightY);
            southPath.Close();

            // South needle (light gray/white)
            canvas.FillColor = Color.FromRgb(220, 220, 220); // Light gray
            canvas.FillPath(southPath);
            
            canvas.StrokeColor = Color.FromRgb(160, 160, 160); // Gray border
            canvas.StrokeSize = 1.5f;
            canvas.DrawPath(southPath);

            // Draw center pivot (metallic look)
            canvas.FillColor = Color.FromRgb(200, 200, 200);
            canvas.FillEllipse(centerX - 5, centerY - 5, 10, 10);
            
            // Inner center dot for depth
            canvas.FillColor = Color.FromRgb(100, 100, 100);
            canvas.FillEllipse(centerX - 3, centerY - 3, 6, 6);
            
            // Highlight on center
            canvas.FillColor = Color.FromRgba(255, 255, 255, 0.6);
            canvas.FillEllipse(centerX - 2, centerY - 2, 3, 3);
        }
    }
}