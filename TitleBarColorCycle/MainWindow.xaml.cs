using Microsoft.Win32;
using System.Runtime.InteropServices;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;

namespace TitleBarColorCycle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref uint attrValue, int attrSize);

        Timer t = new Timer();
        float timerValue = 0f;
        float timerSpeed = 0.5f;
        public MainWindow()
        {
            InitializeComponent();
            t.Tick += Timer_Tick;
            t.Interval = 10;
            t.Start();
        }

        private void SetColor(uint windowColor)
        {
            DwmSetWindowAttribute(new System.Windows.Interop.WindowInteropHelper(this).Handle, 35, ref windowColor, System.Runtime.InteropServices.Marshal.SizeOf(windowColor));
            DwmSetWindowAttribute(new System.Windows.Interop.WindowInteropHelper(this).Handle, 34, ref windowColor, System.Runtime.InteropServices.Marshal.SizeOf(windowColor));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timerValue += timerSpeed;
            if (timerValue > 360f) timerValue = 0f;
            SetColor(Convert.ToUInt32(FromAhsb(0, timerValue, 1f, 0.8f).ToArgb()));
        }

        public static System.Drawing.Color FromAhsb(int alpha, float hue, float saturation, float brightness)
        {
            if (0 > alpha
                || 255 < alpha)
            {
                throw new ArgumentOutOfRangeException(
                    "alpha",
                    alpha,
                    "Value must be within a range of 0 - 255.");
            }

            if (0f > hue
                || 360f < hue)
            {
                throw new ArgumentOutOfRangeException(
                    "hue",
                    hue,
                    "Value must be within a range of 0 - 360.");
            }

            if (0f > saturation
                || 1f < saturation)
            {
                throw new ArgumentOutOfRangeException(
                    "saturation",
                    saturation,
                    "Value must be within a range of 0 - 1.");
            }

            if (0f > brightness
                || 1f < brightness)
            {
                throw new ArgumentOutOfRangeException(
                    "brightness",
                    brightness,
                    "Value must be within a range of 0 - 1.");
            }

            if (0 == saturation)
            {
                return System.Drawing.Color.FromArgb(
                                    alpha,
                                    Convert.ToInt32(brightness * 255),
                                    Convert.ToInt32(brightness * 255),
                                    Convert.ToInt32(brightness * 255));
            }

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < brightness)
            {
                fMax = brightness - (brightness * saturation) + saturation;
                fMin = brightness + (brightness * saturation) - saturation;
            }
            else
            {
                fMax = brightness + (brightness * saturation);
                fMin = brightness - (brightness * saturation);
            }

            iSextant = (int)Math.Floor(hue / 60f);
            if (300f <= hue)
            {
                hue -= 360f;
            }

            hue /= 60f;
            hue -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = (hue * (fMax - fMin)) + fMin;
            }
            else
            {
                fMid = fMin - (hue * (fMax - fMin));
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return System.Drawing.Color.FromArgb(alpha, iMid, iMax, iMin);
                case 2:
                    return System.Drawing.Color.FromArgb(alpha, iMin, iMax, iMid);
                case 3:
                    return System.Drawing.Color.FromArgb(alpha, iMin, iMid, iMax);
                case 4:
                    return System.Drawing.Color.FromArgb(alpha, iMid, iMin, iMax);
                case 5:
                    return System.Drawing.Color.FromArgb(alpha, iMax, iMin, iMid);
                default:
                    return System.Drawing.Color.FromArgb(alpha, iMax, iMid, iMin);
            }
        }

        public static bool isHex(string hc)
        {

            return Regex.IsMatch(hc, @"[#][0-9A-Fa-f]{6}\b");
        }

        private void tbCustomColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isHex(tbCustomColor.Text))
            {
                ((Border)tbCustomColor.Parent).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tbCustomColor.Text));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            t.Enabled = !t.Enabled;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                timerSpeed = float.Parse(tbCycleSpeed.Text) / 100f;
            } catch (System.Exception) { }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            t.Enabled = false;
            try
            {
                string hexColor = tbCustomColor.Text.Substring(1);
                uint color = Convert.ToUInt32(hexColor.Substring(4, 2) + hexColor.Substring(2, 2) + hexColor.Substring(0, 2), 16);
                Console.Out.WriteLine(color + "");
                SetColor(color);
            }
            catch (Exception) { }
        }

        public static uint ColorToUint(Color c)
        {
            uint u = (UInt32)c.A << 24;
            u += (UInt32)c.R << 16;
            u += (UInt32)c.G << 8;
            u += c.B;
            return u;
        }
    }
}
