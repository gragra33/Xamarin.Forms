using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.StyleSheets.UnitTests
{
	[TestFixture]
	public class StyleTests
	{
		[SetUp]
		public void SetUp()
		{
			Device.PlatformServices = new MockPlatformServices();
			Internals.Registrar.RegisterAll(new Type[0]);
		}

		[TearDown]
		public void TearDown()
		{
			Device.PlatformServices = null;
			Application.ClearCurrent();
		}

		[Test]
		public void PropertiesAreApplied()
		{
			var styleString = @"background-color: #ff0000;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var ve = new VisualElement();
			Assume.That(ve.BackgroundColor, Is.EqualTo(Color.Default));
			style.Apply(ve);
			Assert.That(ve.BackgroundColor, Is.EqualTo(Color.Red));
		}

		[Test]
		public void PropertiesSetByStyleDoesNotOverrideManualOne()
		{
			var styleString = @"background-color: #ff0000;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var ve = new VisualElement() { BackgroundColor = Color.Pink };
			Assume.That(ve.BackgroundColor, Is.EqualTo(Color.Pink));

			style.Apply(ve);
			Assert.That(ve.BackgroundColor, Is.EqualTo(Color.Pink));
		}

		[Test]
		public void StylesAreCascading()
		{
			//color should cascade, background-color should not
			var styleString = @"background-color: #ff0000; color: #00ff00;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var label = new Label();
			var layout = new StackLayout {
				Children = {
					label,
				}
			};

			Assume.That(layout.BackgroundColor, Is.EqualTo(Color.Default));
			Assume.That(label.BackgroundColor, Is.EqualTo(Color.Default));
			Assume.That(label.TextColor, Is.EqualTo(Color.Default));

			style.Apply(layout);
			Assert.That(layout.BackgroundColor, Is.EqualTo(Color.Red));
			Assert.That(label.BackgroundColor, Is.EqualTo(Color.Default));
			Assert.That(label.TextColor, Is.EqualTo(Color.Lime));
		}

		[Test]
		public void PropertiesAreOnlySetOnMatchingElements()
		{
			var styleString = @"background-color: #ff0000; color: #00ff00;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var layout = new StackLayout();
			Assert.That(layout.GetValue(TextElement.TextColorProperty), Is.EqualTo(Color.Default));
		}

		[Test]
		public void StyleSheetsOnAppAreApplied()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("label{ color: red;}"));
			var page = new ContentPage {
				Content = new Label()
			};
			app.MainPage = page;
			Assert.That((page.Content as Label).TextColor, Is.EqualTo(Color.Red));
		}

		public string ToVenderSpecificCss(Type type, string property)
		{
			var sb = new StringBuilder();
			sb.Append(Char.ToLower(property[0]));

			for (var i = 1; i < property.Length; i ++)
			{
				var c = property[i];
				var lower = Char.ToLower(c);
				if (c != lower)
					sb.Append('-');
				sb.Append(lower);
			}

			var cssProperty = sb.ToString();
			return $"-xf-{ type.Name.ToLower()}-{cssProperty}";
		}

		public object ApplyVenderSpecificCssValue(
			Type concreteType, Type type, string property, string value)
		{
			var app = new MockApplication();

			var css = $"{concreteType.Name} {{ {ToVenderSpecificCss(type, property)}: {value}; }}";
			app.Resources.Add(StyleSheet.FromString(css));

			var content = Activator.CreateInstance(concreteType, nonPublic: true);
			var page = content is View ? new ContentPage
			{
				Content = (View)content
			} : (Page)content;
			app.MainPage = page;

			var bf = BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance;
			return type.InvokeMember(property, bf, null, content, null);
		}

		[
			TestCase(typeof(ActivityIndicator), nameof(ActivityIndicator.Color)),
			TestCase(typeof(BoxView), nameof(BoxView.Color)),
			//TestCase(typeof(BorderElement), nameof(BorderElement.BorderColor)), // no BoarderColor poperty
			TestCase(typeof(Button), new[] {
				nameof(Button.BorderWidth),
				nameof(Button.BorderColor),
				nameof(Button.CornerRadius),
			}), 
			TestCase(typeof(Editor), new[] {
				nameof(Editor.TextColor),
				nameof(Editor.PlaceholderColor),
				nameof(Editor.Placeholder)
			}),
			TestCase(typeof(Entry), new[] {
				nameof(Entry.TextColor),
				nameof(Entry.PlaceholderColor),
				nameof(Entry.Placeholder),
				nameof(Entry.IsPassword),
			}),
			TestCase(typeof(Grid), new[] {
				nameof(Grid.RowSpacing),
				nameof(Grid.ColumnSpacing),
			}),
			TestCase(typeof(InputView), new[] {
				nameof(InputView.MaxLength),
				//nameof(InputView.Keyboard), // not an enum
			}),
			TestCase(typeof(Label), nameof(Label.VerticalTextAlignment)),
			TestCase(typeof(ProgressBar), nameof(ProgressBar.ProgressColor)),
			TestCase(typeof(SearchBar), new[] {
				nameof(SearchBar.PlaceholderColor),
				nameof(SearchBar.CancelButtonColor),
			}),
			TestCase(typeof(ScrollView), new[] {
				nameof(ScrollView.Orientation),
				nameof(ScrollView.HorizontalScrollBarVisibility),
				nameof(ScrollView.VerticalScrollBarVisibility),
			}),
			//TestCase(typeof(Span), new[] { // not IStylable
			//	nameof(Span.BackgroundColor),
			//}),
			TestCase(typeof(StackLayout), new[] {
				nameof(StackLayout.Spacing),
				nameof(StackLayout.Orientation),
			}),
			TestCase(typeof(Switch), nameof(Switch.OnColor)),
			TestCase(typeof(TabbedPage), new[] {
				nameof(TabbedPage.BarBackgroundColor),
				nameof(TabbedPage.BarTextColor),
			}),
			TestCase(typeof(TableView), nameof(TableView.RowHeight)),
			//TestCase(typeof(View), new[] { // no property View.MarginLeft etc
			//	nameof(View.MarginLeft),
			//	nameof(View.MarginRight),
			//	nameof(View.MarginTop),
			//	nameof(View.MarginBottom),
			//}),
			TestCase(typeof(VisualElement), new[] {
				nameof(VisualElement.AnchorX),
				nameof(VisualElement.AnchorY),
				nameof(VisualElement.TranslationX),
				nameof(VisualElement.TranslationY),
				nameof(VisualElement.Rotation),
				nameof(VisualElement.RotationX),
				nameof(VisualElement.RotationY),
				nameof(VisualElement.Scale),
				nameof(VisualElement.ScaleX),
				nameof(VisualElement.ScaleY),
			}),
		]
		public void GreenVenderSpecificStyleSheetsAreApplied(Type type, object propertyOrArray)
		{
			if (propertyOrArray is string)
				propertyOrArray = new[] { propertyOrArray };

			var values = new[] {
				new { type = typeof(Color), css = "limegreen", result = (object)Color.LimeGreen },
				new { type = typeof(string), css = "your name here", result = (object)"your name here" },
				new { type = typeof(bool), css = "true", result = (object)true },
				new { type = typeof(int), css = "16", result = (object)16 },
				new { type = typeof(double), css = "4.2", result = (object)4.2d },
				//new { type = typeof(Keyboard), css = "Keyboard.Telephone", result = (object)Keyboard.Telephone },
				new { type = typeof(TextAlignment), css = "center", result = (object)TextAlignment.Center },
				new { type = typeof(ScrollOrientation), css = "horizontal", result = (object)ScrollOrientation.Horizontal },
				new { type = typeof(ScrollBarVisibility), css = "never", result = (object)ScrollBarVisibility.Never },
				new { type = typeof(StackOrientation), css = "vertical", result = (object)StackOrientation.Vertical },
			}.ToDictionary(o => o.type);

			foreach (string property in (object[])propertyOrArray)
			{
				var concretType = type;
				if (concretType == typeof(VisualElement))
					concretType = typeof(Entry);

				var element = Activator.CreateInstance(concretType, nonPublic: true);
				var bp = ((IStylable)element).GetProperty(ToVenderSpecificCss(type, property), false);
				var value = values[bp.ReturnType];

				var result = ApplyVenderSpecificCssValue(concretType, type, property, value.css);
				Assert.That(result, Is.EqualTo(value.result));
			}
		}
	}
}