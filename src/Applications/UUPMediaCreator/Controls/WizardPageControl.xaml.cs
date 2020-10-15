using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace UUPMediaCreator.UWP
{
    [ContentProperty(Name = nameof(CastingElement))]
    public sealed partial class WizardPageControl : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
          "Title",
          typeof(string),
          typeof(WizardPageControl),
          new PropertyMetadata(null, new PropertyChangedCallback(OnChanged))
        );

        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
          "Subtitle",
          typeof(string),
          typeof(WizardPageControl),
          new PropertyMetadata("", new PropertyChangedCallback(OnChanged))
        );

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
          "Glyph",
          typeof(string),
          typeof(WizardPageControl),
          new PropertyMetadata("", new PropertyChangedCallback(OnChanged))
        );

        public static readonly DependencyProperty BackEnabledProperty = DependencyProperty.Register(
          "BackEnabled",
          typeof(bool),
          typeof(WizardPageControl),
          new PropertyMetadata(false, new PropertyChangedCallback(OnChanged))
        );

        public static readonly DependencyProperty NextEnabledProperty = DependencyProperty.Register(
          "NextEnabled",
          typeof(bool),
          typeof(WizardPageControl),
          new PropertyMetadata(false, new PropertyChangedCallback(OnChanged))
        );

        public FrameworkElement CastingElement { get; set; }

        public event EventHandler<RoutedEventArgs> NextClicked;

        public event EventHandler<RoutedEventArgs> BackClicked;

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        public string Glyph
        {
            get { return (string)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }

        public bool BackEnabled
        {
            get { return (bool)GetValue(BackEnabledProperty); }
            set { SetValue(BackEnabledProperty, value); }
        }

        public bool NextEnabled
        {
            get { return (bool)GetValue(NextEnabledProperty); }
            set { SetValue(NextEnabledProperty, value); }
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public WizardPageControl()
        {
            this.InitializeComponent();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (NextClicked != null)
            {
                NextClicked.Invoke(sender, e);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackClicked != null)
            {
                BackClicked.Invoke(sender, e);
            }
        }
    }
}