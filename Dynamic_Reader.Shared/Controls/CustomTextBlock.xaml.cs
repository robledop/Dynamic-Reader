using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Dynamic_Reader.Controls
{
	public sealed partial class CustomTextBlock
	{
		public int FontType
		{
			set { SetValue(FontTypeProperty, value); }
		}

		public static readonly DependencyProperty FontTypeProperty =
			DependencyProperty.Register("FontType", typeof(int), typeof(CustomTextBlock), new PropertyMetadata(1, FontTypePropertyChangedCallback));

		private static void FontTypePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var newValue = (int)dependencyPropertyChangedEventArgs.NewValue;
			var textBlock = (CustomTextBlock)dependencyObject;

			var fontName = FontTypeList[newValue];
			var fontType = new FontFamily(fontName);

			textBlock.FontFamily = fontType;
		}

		public new static readonly DependencyProperty FontSizeProperty =
			DependencyProperty.Register("FontSize", typeof(double), typeof(CustomTextBlock), new PropertyMetadata(42.0));

		private new static readonly DependencyProperty FontFamilyProperty =
			DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(CustomTextBlock), new PropertyMetadata(new FontFamily("Lucida Console")));

		public static readonly DependencyProperty DisplayGuidesProperty =
			DependencyProperty.Register("DisplayGuides", typeof(bool), typeof(CustomTextBlock), new PropertyMetadata(false));

		public static readonly DependencyProperty OrpEnabledProperty =
			DependencyProperty.Register("OrpEnabled",
				typeof(bool),
				typeof(CustomTextBlock),
				new PropertyMetadata(true, OrpEnabledPropertyChangedCallback));

		public static readonly DependencyProperty TextValueProperty = DependencyProperty.Register(
			"TextValue",
			typeof(string),
			typeof(CustomTextBlock),
			new PropertyMetadata(string.Empty, TextValuePropertyChangedCallback));

		private static readonly List<string> FontTypeList = new List<string>
		{
			"Courier New",
			"Lucida Console",
			"/Fonts/DroidSansMono.ttf#Droid Sans Mono",
			"/Fonts/Anonymous.ttf#Anonymous"
		};

		public new double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		private new FontFamily FontFamily
		{
			set { SetValue(FontFamilyProperty, value); }
		}
		public bool DisplayGuides
		{
			get { return (bool)GetValue(DisplayGuidesProperty); }
			set { SetValue(DisplayGuidesProperty, value); }
		}

		public bool OrpEnabled
		{
			get { return (bool)GetValue(OrpEnabledProperty); }
			set { SetValue(OrpEnabledProperty, value); }
		}

		public string TextValue
		{
			get { return GetValue(TextValueProperty) as string; }
			set { SetValue(TextValueProperty, value); }
		}

		public CustomTextBlock()
		{
			InitializeComponent();
			LayoutRoot.DataContext = this;
		}

		private static void OrpEnabledPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var customTextBlock = dependencyObject as CustomTextBlock;
			if (customTextBlock == null) return;

			if (!customTextBlock.OrpEnabled)
			{
				customTextBlock.PivotRun.Text = string.Empty;
				customTextBlock.EndRun.Text = string.Empty;

				customTextBlock.StartRun.Text = customTextBlock.TextValue;
				customTextBlock.StartRun.TextAlignment = TextAlignment.Center;
			}
			else
			{
				SetText(customTextBlock.TextValue, customTextBlock);
			}
		}

		private static void TextValuePropertyChangedCallback(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var customTextBlock = dependencyObject as CustomTextBlock;

			if (customTextBlock == null || customTextBlock.TextValue == null) return;
			if (!customTextBlock.OrpEnabled)
			{
				customTextBlock.StartRun.Text = customTextBlock.TextValue;
				return;
			}

			var length = customTextBlock.TextValue.Length;
			var sb = new StringBuilder(customTextBlock.TextValue);


			if (sb.ToString().EndsWith(",")
				|| sb.ToString().EndsWith(".")
				|| sb.ToString().EndsWith("!")
				|| sb.ToString().EndsWith("?")
				|| sb.ToString().EndsWith(":")
				|| sb.ToString().EndsWith(";")
				|| sb.ToString().EndsWith("\""))
			{
				sb.Append(" ");
			}

			if (length < 6)
			{
				var bit = 1;
				while (sb.Length < 22)
				{
					if (bit > 0)
					{
						sb.Append(" ");
					}
					else
					{
						sb.Insert(0, " ");
					}
					bit = bit * -1;
				}
				SetText(sb.ToString(), customTextBlock);
			}
			else
			{
				var tail = 22 - (sb.Length + 7);
				sb.Insert(0, "       ");

				for (int i = 0; i < tail; i++)
				{
					sb.Append(" ");
				}

				SetText(sb.ToString(), customTextBlock);
			}
		}

		private static void SetText(string word, CustomTextBlock customTextBlock)
		{
			var start = word.Substring(0, word.Length / 2);
			var end = word.Substring(word.Length / 2);

			customTextBlock.StartRun.Text = start.Substring(0, start.Length - 1);
			customTextBlock.PivotRun.Text = start.Substring(start.Length - 1);
			customTextBlock.EndRun.Text = end;

			if (customTextBlock.StartRun.Text == "          ")
			{
				customTextBlock.StartRun.Text = "..........";
				customTextBlock.StartRun.Opacity = 0;
			}
			else
			{
				customTextBlock.StartRun.Opacity = 1;
			}
		}
	}
}
