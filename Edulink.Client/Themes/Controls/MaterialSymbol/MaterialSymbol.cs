using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Edulink.Controls.MaterialSymbol
{
    public class MaterialSymbol : Control
    {
        static MaterialSymbol()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaterialSymbol), new FrameworkPropertyMetadata(typeof(MaterialSymbol)));
        }

        public static readonly DependencyProperty SymbolKindProperty = DependencyProperty.Register(
             nameof(Kind),
             typeof(MaterialSymbolKind),
             typeof(MaterialSymbol),
             new PropertyMetadata(default(MaterialSymbolKind), OnSymbolChanged));

        public MaterialSymbolKind Kind
        {
            get => (MaterialSymbolKind)GetValue(SymbolKindProperty);
            set => SetValue(SymbolKindProperty, value);
        }

        public static readonly DependencyProperty IsFilledProperty = DependencyProperty.Register(
            nameof(IsFilled),
            typeof(bool),
            typeof(MaterialSymbol),
            new PropertyMetadata(false, OnSymbolChanged));

        public bool IsFilled
        {
            get => (bool)GetValue(IsFilledProperty);
            set => SetValue(IsFilledProperty, value);
        }

        public static readonly DependencyProperty SymbolBrushProperty = DependencyProperty.Register(
            nameof(SymbolBrush),
            typeof(Brush),
            typeof(MaterialSymbol),
            new PropertyMetadata(Brushes.Black));

        public Brush SymbolBrush
        {
            get => (Brush)GetValue(SymbolBrushProperty);
            set => SetValue(SymbolBrushProperty, value);
        }

        public static readonly DependencyProperty SymbolGeometryProperty = DependencyProperty.Register(
            nameof(SymbolGeometry),
            typeof(Geometry),
            typeof(MaterialSymbol),
            new PropertyMetadata(null));

        public Geometry SymbolGeometry
        {
            get => (Geometry)GetValue(SymbolGeometryProperty);
            private set => SetValue(SymbolGeometryProperty, value);
        }

        public static readonly DependencyProperty SymbolSizeProperty = DependencyProperty.Register(
            nameof(SymbolSize),
            typeof(double),
            typeof(MaterialSymbol),
            new PropertyMetadata(double.NaN));

        public double SymbolSize
        {
            get => (double)GetValue(SymbolSizeProperty);
            set => SetValue(SymbolSizeProperty, value);
        }


        private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MaterialSymbol materialSymbol)
            {
                materialSymbol.UpdateSymbolPath();
            }
        }

        private void UpdateSymbolPath()
        {
            try
            {
                string pathData = MaterialSymbolPathData.GetPathData(Kind, IsFilled);
                SymbolGeometry = Geometry.Parse(pathData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to parse geometry: {ex.Message}");
                SymbolGeometry = null;
            }
        }
    }
}