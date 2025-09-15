using System.Windows;
using System.Windows.Controls;

namespace Bloc4.Infrastructure
{
    // Public + static, dans le namespace EXACT Bloc4.Infrastructure
    public static class PasswordBoxBinder
    {
        // Permet d’activer/désactiver le binding (évite d’accrocher l’event partout)
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxBinder),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static void SetBindPassword(DependencyObject dp, bool value) => dp.SetValue(BindPasswordProperty, value);
        public static bool GetBindPassword(DependencyObject dp) => (bool)dp.GetValue(BindPasswordProperty);

        // La vraie valeur bindée (TwoWay par défaut)
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxBinder),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        public static void SetBoundPassword(DependencyObject dp, string value) => dp.SetValue(BoundPasswordProperty, value);
        public static string GetBoundPassword(DependencyObject dp) => (string)dp.GetValue(BoundPasswordProperty);

        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box)
            {
                if ((bool)e.NewValue)
                    box.PasswordChanged += HandlePasswordChanged;
                else
                    box.PasswordChanged -= HandlePasswordChanged;
            }
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box)
            {
                var newPassword = e.NewValue as string ?? string.Empty;
                if (box.Password != newPassword)
                    box.Password = newPassword;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                // Réécrit la DP sans reboucler (grâce au check dans OnBoundPasswordChanged)
                SetBoundPassword(box, box.Password);
            }
        }
    }
}