using System;
using System.Collections.Generic;
using System.Windows;
using System.Globalization;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using MathGrapher.Core.Algorithms;

namespace MathGrapher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            string formula = FormulaTextBox.Text.Trim();

            if (string.IsNullOrEmpty(formula))
            {
                ShowError("Введите формулу.");
                return;
            }

            if (!TryParseDouble(XMinTextBox.Text, out double xMin, "XMin")) return;
            if (!TryParseDouble(XMaxTextBox.Text, out double xMax, "XMax")) return;
            if (!TryParseDouble(StepTextBox.Text, out double step, "Шаг")) return;

            if (xMin >= xMax)
            {
                ShowError("XMin должен быть меньше XMax");
                return;
            }
            if (step <= 0)
            {
                ShowError("Шаг должен быть положительным");
                return;
            }

            List<DataPoint> points = new List<DataPoint>();
            for (double x = xMin; x <= xMax; x+= step)
            {
                try
                {
                    double y = ExpressionParser.Evaluate(formula, x);
                    if (!double.IsNaN(y) && !double.IsInfinity(y))
                    {
                        points.Add(new DataPoint(x, y));
                    }
                }
                catch
                {

                }
            }

            if (points.Count == 0)
            {
                ShowError("Нет допустимых точек для построения графика.\nВозможно, функция не определена на всем интервале.");
                return;
            }

            PlotModel model = new PlotModel { Title = $"y = {formula}" };

            LineSeries lineSeries = new LineSeries
            {
                Title = formula,
                Color = OxyColors.DodgerBlue,
                StrokeThickness = 2,
                MarkerType = MarkerType.None
            };
            lineSeries.Points.AddRange(points);
            model.Series.Add(lineSeries);

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X",
                Minimum = xMin,
                Maximum = xMax,
                PositionAtZeroCrossing = true,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                AxislineThickness = 1,
                TitlePosition = 1.0,
                AxisTitleDistance = 10
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Y",
                PositionAtZeroCrossing = true,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                AxislineThickness = 1,
                TitlePosition = 1.0,
                AxisTitleDistance = 10
            });

            model.PlotAreaBorderThickness = new OxyThickness(0);
            PlotView.Model = model;

            double? area = null;
            try
            {
                int n = Math.Max(100, (int)((xMax - xMin)/step));
                Func<double, double> func = x => ExpressionParser.Evaluate(formula, x);
                area = Integrator.Trapezoidal(func, xMin, xMax, n);
                StatusTextBlock.Text = $"Готово. Точек: {points.Count}. Площадь ≈ {area:F4}";
            }
            catch
            {
                StatusTextBlock.Text = $"Готово. Точек: {points.Count}. Площадь не вычислена.";
            }
        }

        private bool TryParseDouble(string text, out double value, string fieldName)
        {
            if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                ShowError($"Неккоректное значение в поле '{fieldName}'. Введите число.");
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            StatusTextBlock.Text = "Ошибка";
        }
    }
}