// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace AdventureCamCore.Behaviors
{
    public class FontSizeToFitHeightBehavior : Behavior<Label>
    {
        #region Methods

        protected override void OnAttachedTo(Label bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.SizeChanged += OnSizeChanged;
        }

        protected override void OnDetachingFrom(Label bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.SizeChanged -= OnSizeChanged;
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (sender is Label label)
            {
                double height = label.Height;
                if (height > 0)
                {
                    // Adjust the font size based on the label's height
                    label.FontSize = height * 0.5; // Adjust the multiplier as needed
                }
            }
        }

        #endregion Methods
    }
}