// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace AdventureCamCore.Controls
{
    public class ResizeableFontLabel : Label
    {
        #region Fields

        public static readonly BindableProperty ContainerSizeProperty = BindableProperty.Create(
                    propertyName: nameof(ContainerSize),
                    returnType: typeof(Size),
                    declaringType: typeof(ResizeableFontLabel),
                    defaultValue: new Size(0, 0),
                    defaultBindingMode: BindingMode.TwoWay,
                    propertyChanged: OnContainerSizeChanged);

        #endregion Fields

        #region Properties

        public Size ContainerSize
        {
            get => (Size)GetValue(ContainerSizeProperty);
            set => SetValue(ContainerSizeProperty, value);
        }

        #endregion Properties

        #region Methods

        private static void OnContainerSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ResizeableFontLabel)bindable;
            // Handle the property changed logic here
        }

        #endregion Methods
    }
}