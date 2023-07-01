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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;
using System.IO;
using System.Diagnostics;

namespace UEM5;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : UiWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Games.AddGames(gameslist, gameoptions, openfolderbtn);
        ToolTip tt = new ToolTip();
        tt.Content = "Support me on Ko-fi";
        kofi.ToolTip = tt;

    }

    private void openfolderbtn_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = Games.GetCurrPath(),
            UseShellExecute = true,
            Verb = "open"
        });
    }
    private void searchbox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            gameslist.Children.Clear();
            foreach (Grid gr in Games.GetBackup())
            {
                foreach (object obj in gr.Children)
                {
                    if (obj.GetType() == typeof(Button))
                    {
                        if (((Button)obj).Content.ToString().Contains(searchbox.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            gameslist.Children.Add((Grid)((Button)obj).Parent);
                        }
                    }
                }
            }
        }
        catch { }
    }

    private void kofi_Click(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", "https://ko-fi.com/ddarknessone");
    }
}
