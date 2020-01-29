using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MahApps.Metro.Controls;
using MahApps.Metro;
using System.Timers;

namespace Calculator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //明暗主题
        private List<BaseColor> DarkLight { get; }
        //运算数据
        private Symbol symbol;
        private bool Point;
        private bool IsCalculated;
        public string FirstNumber
        {
            get { return (string)GetValue(FirstNumberProperty); }
            set { SetValue(FirstNumberProperty, value); }
        }

        public static readonly DependencyProperty FirstNumberProperty =
            DependencyProperty.Register("FirstNumber", typeof(string), typeof(MainWindow));

        public string SecondNumber
        {
            get { return (string)GetValue(SecondNumberProperty); }
            set { SetValue(SecondNumberProperty, value); }
        }

        public static readonly DependencyProperty SecondNumberProperty =
            DependencyProperty.Register("SecondNumber", typeof(string), typeof(MainWindow));
        public MainWindow()
        {
            InitializeComponent();
            //主题和配色设置
            Accents.ItemsSource = ThemeManager.ColorSchemes;
            this.DarkLight = ThemeManager.Themes
                             .GroupBy(x => x.BaseColorScheme)
                             .Select(x => x.First())
                             .Select(a => new BaseColor() { Name = a.BaseColorScheme, BaseColorBrush = a.Resources["MahApps.Brushes.WhiteColor"] as Brush })
                             .ToList();
            baseColors.ItemsSource = DarkLight;
            //状态初始化
            symbol = Symbol.None;
            Point = false;
            IsCalculated = false;
            FirstNumber = null;
            SecondNumber = null;
            Screen.Content = "0";
        }
        private void Number_Click(object sender, RoutedEventArgs e)
        {
            string input = (e.OriginalSource as Button).Content as string;
            //判断是否进行过运算
            if (IsCalculated)
            {
                FirstNumber = "0";
                SecondNumber = null;
                symbol = Symbol.None;
                IsCalculated = false;
                Screen.SetBinding(ContentProperty, new Binding("FirstNumber") { Source = this }) ;
                FirstNumber = ReadKey(FirstNumber, input);
                Console.WriteLine("FirstNumber = " + FirstNumber);
            }
            //判断键入的是第一个还是第二个操作数
            else if (symbol == Symbol.None)
            {
                Screen.SetBinding(ContentProperty, new Binding("FirstNumber") { Source = this });
                FirstNumber = ReadKey(FirstNumber, input);
                Console.WriteLine("FirstNumber = " + FirstNumber);
            }
            else
            {
                Screen.SetBinding(ContentProperty, new Binding("SecondNumber") { Source = this });
                SecondNumber = ReadKey(SecondNumber, input);
                Console.WriteLine("SecondNumber = " + SecondNumber);
            }
        }
        private void Equal_Click(object sender, RoutedEventArgs e)
        {
            IsCalculated = true;
            Point = false;

            //判断是否按下运算符
            if(symbol == Symbol.None)
            {
                return;
            }
            else if(SecondNumber != null)
            {
                Screen.Content = Calculate(FirstNumber, SecondNumber, symbol);
                FirstNumber = Screen.Content as string;
            }
            else if (symbol != Symbol.None && SecondNumber == null)
            {
                Screen.Content = Calculate(FirstNumber, FirstNumber, symbol);
                SecondNumber = FirstNumber;
                FirstNumber = Screen.Content as string;
            }
        }
        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            //判断连续运算的情况
            if (SecondNumber != null && IsCalculated == false)
            {
                Screen.Content = Calculate(FirstNumber, SecondNumber, symbol);
                FirstNumber = Screen.Content as string;
                SecondNumber = null;
            }
            else if(SecondNumber != null && IsCalculated == true)
            {
                FirstNumber = Screen.Content as string;
                SecondNumber = null;
            }
            switch ((e.OriginalSource as Button).Content as string)
            {
                case "+":
                    symbol = Symbol.Add;
                    break;
                case "-":
                    symbol = Symbol.Subtract;
                    break;
                case "×":
                    symbol = Symbol.Multiply;
                    break;
                case "÷":
                    symbol = Symbol.Divide;
                    break;
                default:
                    break;
            }
            //等于键和小数点键状态清除
            IsCalculated = false;
            Point = false;
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            //数据全部初始化
            symbol = Symbol.None;
            Point = false;
            IsCalculated = false;
            FirstNumber = null;
            SecondNumber = null;
            Screen.Content = "0";
        }
        private void ChangeAccentColor(object sender, RoutedEventArgs e)
        {
            var clicked = e.OriginalSource as MenuItem;
            ColorScheme colorScheme = clicked.DataContext as ColorScheme;
            ThemeManager.ChangeThemeColorScheme(Application.Current, colorScheme.Name);
        }
        private void ChangeBaseColor(object sender, RoutedEventArgs e)
        {
            var clicked = e.OriginalSource as MenuItem;
            BaseColor baseColor = clicked.DataContext as BaseColor;
            ThemeManager.ChangeThemeBaseColor(Application.Current, baseColor.Name);
        }
        public static string Calculate(string firstnumber, string secondnumber, Symbol symbol)
        {
            double result = 0;
            switch (symbol)
            {
                case Symbol.None:
                    result = double.NaN;
                    break;
                case Symbol.Add:
                    result = Convert.ToDouble(firstnumber) + Convert.ToDouble(secondnumber);
                    break;
                case Symbol.Subtract:
                    result = Convert.ToDouble(firstnumber) - Convert.ToDouble(secondnumber);
                    break;
                case Symbol.Multiply:
                    result = Convert.ToDouble(firstnumber) * Convert.ToDouble(secondnumber);
                    break;
                case Symbol.Divide:
                    result = Convert.ToDouble(firstnumber) / Convert.ToDouble(secondnumber);
                    break;
                default:
                    break;
            }
            return Convert.ToString(result);
        }
        public string ReadKey(string str, string input) 
        {
            switch (input)
            {
                case "DEL":
                    if (str == null){str = "0";} //没有可删除的字符时置零
                    else if (str.Count<char>() > 1)
                    {
                        if (str.EndsWith(".")){Point = false; } //删除小数点后激活小数点按键
                        str = str.Remove(str.Length - 1, 1);
                        if (str == "-") { str = "0"; } //只剩负号时置零
                    }
                    else if(str.Count<char>() == 1) { str = "0"; } //只剩一位数字时置零
                break;

                case "±":
                    if (str == null || str == "0") { str = "0"; }
                    //如果为负数，则删去负号
                    else if (str.StartsWith("-")) { str = str.Remove(0, 1); }
                    //如果为正，则添加负号
                    else { str = str.Insert(0, "-"); }
                    break;

                case ".":
                    if (str == null || str == "0") { str = "0."; }
                    else if (Point == false) { str += "."; }
                    Point = true;
                    break;
                case "0":
                    if (str == "0") { str = "0"; }
                    else { str += input; }
                    break;
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    if(str == "0") { str = ""; }
                    str += input;
                    break;
                default:
                    break;
            }
            return str;
        }
    }
    public class BaseColor
    {
        public string Name { get; set; }
        public Brush BaseColorBrush { get; set; }
    }
    public enum Symbol
    {
        None,
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
