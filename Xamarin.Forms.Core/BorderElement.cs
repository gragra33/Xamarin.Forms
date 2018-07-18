using Xamarin.Forms;
using Xamarin.Forms.StyleSheets;

//[assembly: StyleProperty("-xf-borderelement-border-color", typeof(BorderElement), nameof(BorderElement.BorderColorProperty))]

namespace Xamarin.Forms
{
	static class BorderElement
	{
		public static readonly BindableProperty BorderColorProperty =
			BindableProperty.Create("BorderColor", typeof(Color), typeof(IBorderElement), Color.Default,
									propertyChanged: OnBorderColorPropertyChanged);

		static void OnBorderColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((IBorderElement)bindable).OnBorderColorPropertyChanged((Color)oldValue, (Color)newValue);
		}
	}
}