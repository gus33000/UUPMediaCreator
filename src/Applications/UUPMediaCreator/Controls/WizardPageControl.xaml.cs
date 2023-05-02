/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace UUPMediaCreator
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
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public bool BackEnabled
        {
            get => (bool)GetValue(BackEnabledProperty);
            set => SetValue(BackEnabledProperty, value);
        }

        public bool NextEnabled
        {
            get => (bool)GetValue(NextEnabledProperty);
            set => SetValue(NextEnabledProperty, value);
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public WizardPageControl()
        {
            InitializeComponent();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextClicked?.Invoke(sender, e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackClicked?.Invoke(sender, e);
        }
    }
}