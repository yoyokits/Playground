using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WorldMapControls.Controls
{
    public partial class ColorPickerDialog : Window
    {
        public Color SelectedColor { get; private set; } = Colors.Gray;
        
        private bool _updatingSliders = false;

        public ColorPickerDialog()
        {
            InitializeComponent();
            InitializeColorPalette();
            SetColor(Colors.Gray);
        }

        public ColorPickerDialog(Color initialColor) : this()
        {
            SetColor(initialColor);
        }

        private void InitializeColorPalette()
        {
            // Create a comprehensive palette of colors useful for outline colors
            var colors = new Color[]
            {
                // Row 1 - Pure colors and common outline colors
                Colors.Black, Colors.White, Color.FromRgb(128, 128, 128), Color.FromRgb(64, 64, 64),
                Color.FromRgb(192, 192, 192), Color.FromRgb(140, 140, 140), Color.FromRgb(96, 96, 96), Color.FromRgb(32, 32, 32),
                Color.FromRgb(255, 0, 0), Color.FromRgb(0, 255, 0), Color.FromRgb(0, 0, 255), Color.FromRgb(255, 255, 0),
                
                // Row 2 - Reds and oranges
                Color.FromRgb(255, 51, 51), Color.FromRgb(255, 102, 102), Color.FromRgb(204, 0, 0), Color.FromRgb(153, 0, 0),
                Color.FromRgb(255, 69, 0), Color.FromRgb(255, 140, 0), Color.FromRgb(255, 165, 0), Color.FromRgb(255, 215, 0),
                Color.FromRgb(255, 160, 122), Color.FromRgb(205, 92, 92), Color.FromRgb(178, 34, 34), Color.FromRgb(139, 69, 19),
                
                // Row 3 - Yellows and greens
                Color.FromRgb(255, 255, 224), Color.FromRgb(255, 228, 196), Color.FromRgb(218, 165, 32), Color.FromRgb(184, 134, 11),
                Color.FromRgb(127, 255, 0), Color.FromRgb(50, 205, 50), Color.FromRgb(0, 128, 0), Color.FromRgb(34, 139, 34),
                Color.FromRgb(144, 238, 144), Color.FromRgb(152, 251, 152), Color.FromRgb(46, 125, 50), Color.FromRgb(76, 175, 80),
                
                // Row 4 - Cyans and teals
                Color.FromRgb(0, 255, 255), Color.FromRgb(64, 224, 208), Color.FromRgb(72, 209, 204), Color.FromRgb(0, 139, 139),
                Color.FromRgb(0, 128, 128), Color.FromRgb(95, 158, 160), Color.FromRgb(102, 205, 170), Color.FromRgb(175, 238, 238),
                Color.FromRgb(0, 191, 255), Color.FromRgb(30, 144, 255), Color.FromRgb(135, 206, 235), Color.FromRgb(173, 216, 230),
                
                // Row 5 - Blues
                Color.FromRgb(65, 105, 225), Color.FromRgb(100, 149, 237), Color.FromRgb(0, 0, 139), Color.FromRgb(25, 25, 112),
                Color.FromRgb(72, 61, 139), Color.FromRgb(106, 90, 205), Color.FromRgb(123, 104, 238), Color.FromRgb(147, 112, 219),
                Color.FromRgb(138, 43, 226), Color.FromRgb(75, 0, 130), Color.FromRgb(176, 196, 222), Color.FromRgb(119, 136, 153),
                
                // Row 6 - Purples and magentas
                Color.FromRgb(255, 0, 255), Color.FromRgb(218, 112, 214), Color.FromRgb(221, 160, 221), Color.FromRgb(238, 130, 238),
                Color.FromRgb(255, 20, 147), Color.FromRgb(199, 21, 133), Color.FromRgb(219, 112, 147), Color.FromRgb(255, 105, 180),
                Color.FromRgb(255, 182, 193), Color.FromRgb(186, 85, 211), Color.FromRgb(148, 0, 211), Color.FromRgb(128, 0, 128),
                
                // Row 7 - Browns and earth tones
                Color.FromRgb(160, 82, 45), Color.FromRgb(210, 180, 140), Color.FromRgb(222, 184, 135), Color.FromRgb(245, 222, 179),
                Color.FromRgb(139, 69, 19), Color.FromRgb(205, 133, 63), Color.FromRgb(244, 164, 96), Color.FromRgb(210, 105, 30),
                Color.FromRgb(255, 228, 196), Color.FromRgb(255, 218, 185), Color.FromRgb(188, 143, 143), Color.FromRgb(205, 183, 158),
                
                // Row 8 - Additional grays and dark colors for outlines
                Color.FromRgb(47, 79, 79), Color.FromRgb(105, 105, 105), Color.FromRgb(169, 169, 169), Color.FromRgb(211, 211, 211),
                Color.FromRgb(220, 220, 220), Color.FromRgb(245, 245, 245), Color.FromRgb(248, 248, 255), Color.FromRgb(240, 248, 255),
                Color.FromRgb(85, 85, 85), Color.FromRgb(68, 68, 68), Color.FromRgb(51, 51, 51), Color.FromRgb(34, 34, 34)
            };

            foreach (var color in colors)
            {
                var rect = new Rectangle
                {
                    Fill = new SolidColorBrush(color),
                    Width = 28,
                    Height = 28,
                    Margin = new Thickness(1),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Stroke = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                    StrokeThickness = 1
                };

                rect.MouseLeftButtonUp += (s, e) => SetColor(color);
                
                // Add tooltip showing color values
                rect.ToolTip = $"RGB({color.R}, {color.G}, {color.B})\n#{color.R:X2}{color.G:X2}{color.B:X2}";
                
                ColorGrid.Children.Add(rect);
            }
        }

        private void SetColor(Color color)
        {
            SelectedColor = color;
            PreviewRectangle.Fill = new SolidColorBrush(color);
            
            _updatingSliders = true;
            RedSlider.Value = color.R;
            GreenSlider.Value = color.G;
            BlueSlider.Value = color.B;
            _updatingSliders = false;
            
            UpdateValueLabels();
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingSliders) return;

            var red = (byte)RedSlider.Value;
            var green = (byte)GreenSlider.Value;
            var blue = (byte)BlueSlider.Value;
            
            var color = Color.FromRgb(red, green, blue);
            SelectedColor = color;
            PreviewRectangle.Fill = new SolidColorBrush(color);
            
            UpdateValueLabels();
        }

        private void UpdateValueLabels()
        {
            RedValue.Text = ((int)RedSlider.Value).ToString();
            GreenValue.Text = ((int)GreenSlider.Value).ToString();
            BlueValue.Text = ((int)BlueSlider.Value).ToString();
            
            // Update hex value
            var r = (int)RedSlider.Value;
            var g = (int)GreenSlider.Value;
            var b = (int)BlueSlider.Value;
            
            if (HexValue != null)
            {
                HexValue.Text = $"#{r:X2}{g:X2}{b:X2}";
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}