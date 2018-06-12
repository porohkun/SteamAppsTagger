using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SteamAppsTagger
{
    /// <summary>
    /// Interaction logic for SelectUserWindow.xaml
    /// </summary>
    public partial class SelectUserWindow : Window
    {
        public string SelectedUser;

        public SelectUserWindow()
        {
            InitializeComponent();
        }

        public SelectUserWindow(Dictionary<string, string> users)
        {
            InitializeComponent();

            foreach (var user in users)
            {
                var button = new Button()
                {
                    Content = string.Format("{0} ({1})", user.Value, user.Key),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(2),
                    Tag = user.Key
                };
                button.Click += Button_Click;
                DockPanel.SetDock(button, Dock.Top);
                dock.Children.Add(button);
            }
            dock.Children.Add(new Grid());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedUser = (sender as Button).Tag as string;
            Close();
        }
    }
}
