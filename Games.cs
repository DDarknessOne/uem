using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Button = Wpf.Ui.Controls.Button;
using Wpf.Ui.Controls;
using System.Diagnostics.Metrics;

namespace UEM5;
internal class Games
{
    private static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static List<Grid> backup = new List<Grid>();
    private static List<List<string>> games = new List<List<string>>();
    private static string currpath = "";
    public static string GetCurrPath() { return currpath; }
    public static List<Grid> GetBackup() { return backup; }
    public static Color PrimaryAccent
    {
        get
        {
            var resource = Application.Current.Resources["SystemAccentColorPrimary"];

            if (resource is Color color) { return color; }
            else { return Colors.Transparent; }
        }
    }
    private static void GetGames()
    {
        try
        {
            List<List<string>> games2 = new List<List<string>>();
            games.Clear();
            List<string> fs = new List<string>();
            List<string> inifiles = new List<string>();
            fs = Directory.GetDirectories(appdata, "*", SearchOption.TopDirectoryOnly).ToList();
            foreach (string dir in fs)
            {
                try
                {
                    inifiles.AddRange(Directory.GetFiles(dir, "*.ini", SearchOption.AllDirectories).Where(x => x.Contains("GameUserSettings.ini", StringComparison.OrdinalIgnoreCase)));
                    inifiles.AddRange(Directory.GetFiles(dir, "*.ini", SearchOption.AllDirectories).Where(x => x.Contains("Engine.ini", StringComparison.OrdinalIgnoreCase)));
                    inifiles.AddRange(Directory.GetFiles(dir, "*.ini", SearchOption.AllDirectories).Where(x => x.Contains("Input.ini", StringComparison.OrdinalIgnoreCase)));
                }
                catch { }
            }
            inifiles = inifiles.Where(x => x.Contains("Saved\\Config\\", StringComparison.OrdinalIgnoreCase)).ToList();

            int i = 1;
            foreach (string s in inifiles)
            {
                int i1 = s.IndexOf("Saved\\Config") - 1;
                int i2 = 0;
                string name = $"NaN{i}";
                i2 = s.IndexOf("AppData\\Local") + 14;
                name = s.Substring(i2, i1 - i2);
                string path = s;

                if (games2.Where(x => x[0] == name).Count() == 0)
                {
                    List<string> list = new List<string>();
                    list.Add(name);
                    list.Add(path);
                    games2.Add(list);
                }
                else
                {
                    games2.Where(x => x[0] == name).First().Add(path);
                }
                i++;
            }
            foreach (List<string> game in games2)
            {
                if (game.Where(x => x.Contains("Input.ini", StringComparison.OrdinalIgnoreCase)).Count() > 0 &&
                    game.Where(x => x.Contains("Engine.ini", StringComparison.OrdinalIgnoreCase)).Count() > 0 &&
                    game.Where(x => x.Contains("GameUserSettings.ini", StringComparison.OrdinalIgnoreCase)).Count() > 0) 
                {
                    games.Add(game);
                }
            }
        }
        catch { games = null; }
    }
    public static void AddGames(WrapPanel gameslist, WrapPanel gameoptions, Button openfolderbtn)
    {
        GetGames();
        if (games == null) { return; }
        foreach (List<string> game in games)
        {
            Grid grid = new Grid
            {
                Width = 250,
                Height = 35,
            };
            Border mini = new Border
            {
                Width = 3,
                Height = 15,
                CornerRadius = new CornerRadius(1.5),
                Margin = new Thickness(5, 0, 0, 0),
                Background = new SolidColorBrush(PrimaryAccent),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Visibility = Visibility.Hidden,
            };
            Button btn = new Button
            {
                Content = game[0],
                Name = $"Name{games.IndexOf(game)}",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5, 1, 5, 1),
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent
            };
            btn.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (btn.DesiredSize.Width > 250)
            {
                ToolTip tt = new ToolTip
                {
                    Content = game[0]
                };
                btn.ToolTip = tt;
            }
            btn.Click += Click;
            void Click(object sender, RoutedEventArgs e)
            {
                foreach (Grid gr in gameslist.Children)
                {
                    foreach (object obj in gr.Children)
                    {
                        if (obj.GetType() == typeof(Button))
                        {
                            ((Button)obj).Background = Brushes.Transparent;
                            ((Button)obj).BorderBrush = Brushes.Transparent;
                        }
                        if (obj.GetType() == typeof(Border))
                        {
                            ((Border)obj).Visibility = Visibility.Hidden;
                        }
                    }
                }

                btn.Background = btn.MouseOverBackground;
                btn.BorderBrush = btn.MouseOverBorderBrush;
                mini.Visibility = Visibility.Visible;
                UpdatePage(gameslist, gameoptions, openfolderbtn, games.IndexOf(game));
            }
            grid.Children.Add(mini);
            grid.Children.Add(btn);
            gameslist.Children.Add(grid);
            backup.Add(grid);
        }
    }

    public static void UpdatePage(WrapPanel gameslist, WrapPanel gameoptions, Button folderbtn, int index)
    {
        try
        {
            gameoptions.Children.Clear();
            List<string> game = games[index];
            List<string> inputinifile = new List<string>();
            List<string> engineinifile = new List<string>();
            List<string> gameusersettingsinifile = new List<string>();
            string inputinipath = "";
            string engineinipath = "";
            string gameusersettingsinipath = "";
            int i = 0;
            List<object[]> lst = new List<object[]>();
            foreach (string s in game)
            {
                if (i > 0)
                {
                    lst.Add(new object[] { s, File.GetLastWriteTime(s) });
                }
                i++;
            }
            lst = lst.OrderByDescending(x => (DateTime)x[1]).ToList();

            //Read Last Modified Input.ini
            inputinipath = lst.Where(x => ((string)x[0]).ToLower().EndsWith("input.ini")).ToList()[0][0].ToString();
            inputinifile = File.ReadAllLines(inputinipath).ToList();
            currpath = inputinipath.Substring(0, inputinipath.LastIndexOf("\\") + 1);
            folderbtn.Visibility = Visibility.Visible;

            //Read Last Modified Engine.ini
            engineinipath = lst.Where(x => ((string)x[0]).ToLower().EndsWith("engine.ini")).ToList()[0][0].ToString();
            engineinifile = File.ReadAllLines(engineinipath).ToList();
            currpath = engineinipath.Substring(0, engineinipath.LastIndexOf("\\") + 1);
            folderbtn.Visibility = Visibility.Visible;

            //Read Last Modified GameUserSettings.ini
            gameusersettingsinipath = lst.Where(x => ((string)x[0]).ToLower().EndsWith("gameusersettings.ini")).ToList()[0][0].ToString();
            gameusersettingsinifile = File.ReadAllLines(gameusersettingsinipath).ToList();
            currpath = gameusersettingsinipath.Substring(0, gameusersettingsinipath.LastIndexOf("\\") + 1);
            folderbtn.Visibility = Visibility.Visible;

            //OPTIONS
            string smoothingstring = "bEnableMouseSmoothing=False";
            string acceleratestring = "bDisableMouseAcceleration=True";
            string smootframerate = "bSmoothFrameRate=False";
            string minsmoothedframerate = "MinSmoothedFrameRate=5";
            string maxsmoothedframerate = "MaxSmoothedFrameRate=500";
            string vsync = "bUseVSync=False";
            string frameratelimit = "FrameRateLimit=0.000000";
            string fullscreenmode = "FullscreenMode=1";
            string fullscreen = "FullScreen=True";
            int frameunlocked = 0;
            //

            //Option Mouse Smoothing
            Border bd1 = new Border
            {
                Margin = new Thickness(10, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24)),
                Height = 50,
                CornerRadius = new CornerRadius(5)
            };
            Grid wp1 = new Grid();
            TextBlock lbl1 = new TextBlock
            {
                Text = "Disable Mouse Smoothing",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };
            ToggleSwitch btn1 = new ToggleSwitch { HorizontalAlignment = HorizontalAlignment.Center };

            //Option Mouse Acceleration
            Border bd2 = new Border
            {
                Margin = new Thickness(10, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24)),
                Height = 50,
                CornerRadius = new CornerRadius(5)
            };
            Grid wp2 = new Grid();
            TextBlock lbl2 = new TextBlock
            {
                Text = "Disable Mouse Acceleration",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };
            ToggleSwitch btn2 = new ToggleSwitch { HorizontalAlignment = HorizontalAlignment.Center };

            //Option Vsync
            Border bd3 = new Border
            {
                Margin = new Thickness(10, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24)),
                Height = 50,
                CornerRadius = new CornerRadius(5)
            };
            Grid wp3 = new Grid();
            TextBlock lbl3 = new TextBlock
            {
                Text = "Disable VSync",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };
            ToggleSwitch btn3 = new ToggleSwitch { HorizontalAlignment = HorizontalAlignment.Center };

            //Option Unlock FPS
            Border bd4 = new Border
            {
                Margin = new Thickness(10, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24)),
                Height = 50,
                CornerRadius = new CornerRadius(5)
            };
            Grid wp4 = new Grid();
            TextBlock lbl4 = new TextBlock
            {
                Text = "Unlock FPS",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };
            ToggleSwitch btn4 = new ToggleSwitch { HorizontalAlignment = HorizontalAlignment.Center };

            //Option FullScreen
            Border bd5 = new Border
            {
                Margin = new Thickness(10, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24)),
                Height = 50,
                CornerRadius = new CornerRadius(5)
            };
            Grid wp5 = new Grid();
            TextBlock lbl5 = new TextBlock
            {
                Text = "Enable FullScreen",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };
            Grid fullsmodecoll = new Grid
            {
                Height = 50,
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Button fullsbtn = new Button
            {
                Content = "Fullscreen",
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Button fullswbtn = new Button
            {
                Content = "Fullscreen Windowed",
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Button windowedbtn = new Button
            {
                Content = "Windowed",
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            ToggleSwitch btn5 = new ToggleSwitch { HorizontalAlignment = HorizontalAlignment.Center };


            foreach (string s in inputinifile)
            {
                if (s.ToLower().Contains("benablemousesmoothing"))
                {
                    if (s.Split('=')[1].Trim().ToLower() == "false")
                    {
                        btn1.IsChecked = true;
                        smoothingstring = "bEnableMouseSmoothing=False";
                    }
                    if (s.Split('=')[1].Trim().ToLower() == "true")
                    {
                        btn1.IsChecked = false;
                        smoothingstring = "bEnableMouseSmoothing=True";
                    }
                }
                if (s.ToLower().Contains("bdisablemouseacceleration"))
                {
                    if (s.Split('=')[1].Trim().ToLower() == "false")
                    {
                        btn2.IsChecked = false;
                        acceleratestring = "bDisableMouseAcceleration=False";
                    }
                    if (s.Split('=')[1].Trim().ToLower() == "true")
                    {
                        btn2.IsChecked = true;
                        acceleratestring = "bDisableMouseAcceleration=True";
                    }
                }
            }
            foreach (string s in engineinifile)
            {
                if (s.ToLower().Contains("bsmoothframerate"))
                {
                    if (s.Split('=')[1].Trim().ToLower() == "false")
                    {
                        smootframerate = "bSmoothFrameRate=False";
                        frameunlocked++;
                    }
                    if (s.Split('=')[1].Trim().ToLower() == "true")
                    {
                        smootframerate = "bSmoothFrameRate=True";
                        frameunlocked--;
                    }
                }
                if (s.ToLower().Contains("minsmoothedframerate"))
                {
                    minsmoothedframerate = $"MinSmoothedFrameRate={s.Split('=')[1].Trim()}";
                    frameunlocked++;
                }
                if (s.ToLower().Contains("maxsmoothedframerate"))
                {
                    maxsmoothedframerate = $"MaxSmoothedFrameRate={s.Split('=')[1].Trim()}";
                    if (Convert.ToInt32(maxsmoothedframerate.Split('=')[1].Trim()) > 60)
                    {
                        frameunlocked++;
                    }
                    else { frameunlocked--; }
                }
                if (s.ToLower().Contains("busevsync"))
                {
                    if (s.Split('=')[1].Trim().ToLower() == "false")
                    {
                        vsync = "bUseVSync=False";
                        btn3.IsChecked = true;
                        frameunlocked++;
                    }
                    if (s.Split('=')[1].Trim().ToLower() == "true")
                    {
                        vsync = "bUseVSync=True";
                        btn3.IsChecked = false;
                        frameunlocked--;
                    }
                }
            }
            foreach (string s in gameusersettingsinifile)
            {
                if (s.ToLower().Contains("busevsync"))
                {
                    if (s.Split('=')[1].Trim().ToLower() == "false")
                    {
                        vsync = "bUseVSync=False";
                        frameunlocked++;
                    }
                    if (s.Split('=')[1].Trim().ToLower() == "true")
                    {
                        vsync = "bUseVSync=True";
                        frameunlocked--;
                    }
                }
                if (s.ToLower().Contains("frameratelimit"))
                {
                    frameratelimit = $"FrameRateLimit={s.Split('=')[1].Trim()}";
                    if (frameratelimit.Split('=')[1].StartsWith("0"))
                    {
                        frameunlocked++;
                    }
                    else { frameunlocked--; }
                }
                if (s.Trim().ToLower().StartsWith("fullscreenmode="))
                {
                    fullscreenmode = $"FullscreenMode={s.Split('=')[1].Trim()}";
                }
                if (s.Replace(" ", "").Trim().StartsWith("FullScreen="))
                {
                    fullscreen = $"FullScreen={s.Split('=')[1].Trim()}";
                }
            }
            if (frameunlocked > 5)
            {
                btn4.IsChecked = true;
            }
            if (fullscreen.Split('=')[1].Trim().ToLower() == "true")
            {
                fullsmodecoll.Visibility = Visibility.Visible;
                btn5.IsChecked = true;
                bd5.Height = 100;
            }
            if (fullscreenmode.Split('=')[1].Trim() == "0")
            {
                fullsbtn.BorderThickness = new Thickness(4);
                fullswbtn.BorderThickness = new Thickness(0);
                windowedbtn.BorderThickness = new Thickness(0);
                fullsbtn.Background = new SolidColorBrush(Color.FromArgb(255, 18, 18, 18));
                fullswbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                windowedbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            if (fullscreenmode.Split('=')[1].Trim() == "1")
            {
                fullsbtn.BorderThickness = new Thickness(0);
                fullswbtn.BorderThickness = new Thickness(4);
                windowedbtn.BorderThickness = new Thickness(0);
                fullsbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                fullswbtn.Background = new SolidColorBrush(Color.FromArgb(255, 18, 18, 18));
                windowedbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            if (fullscreenmode.Split('=')[1].Trim() == "2")
            {
                fullsbtn.BorderThickness = new Thickness(0);
                fullswbtn.BorderThickness = new Thickness(4);
                windowedbtn.BorderThickness = new Thickness(0);
                fullsbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                fullswbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                windowedbtn.Background = new SolidColorBrush(Color.FromArgb(255, 18, 18, 18));
            }
            btn1.Click += Clickbtn1;
            btn2.Click += Clickbtn2;
            btn3.Click += Clickbtn3;
            btn4.Click += Clickbtn4;
            btn5.Click += Clickbtn5;
            fullsbtn.Click += Clickfullsbtn;
            fullswbtn.Click += Clickfullswbtn;
            windowedbtn.Click += Clickwindowedbtn;
            void Clickbtn1(object sender2, RoutedEventArgs e2)
            {
                if ((bool)btn1.IsChecked)
                {
                    smoothingstring = "bEnableMouseSmoothing=False";
                }
                else
                {
                    smoothingstring = "bEnableMouseSmoothing=True";
                }
                UpdateInputFile();
            }
            void Clickbtn2(object sender2, RoutedEventArgs e2)
            {
                if ((bool)btn2.IsChecked)
                {
                    acceleratestring = "bDisableMouseAcceleration=True";
                }
                else
                {
                    acceleratestring = "bDisableMouseAcceleration=False";
                }
                UpdateInputFile();
            }
            void Clickbtn3(object sender2, RoutedEventArgs e2)
            {
                if (!(bool)btn4.IsChecked)
                {
                    if ((bool)btn3.IsChecked)
                    {
                        vsync = "bUseVSync=False";
                    }
                    else
                    {
                        vsync = "bUseVSync=True";
                    }
                    UpdateEngineFile();
                    UpdateGameUserSettingsFile();
                }
            }
            void Clickbtn4(object sender2, RoutedEventArgs e2)
            {
                if ((bool)btn4.IsChecked)
                {
                    smootframerate = "bSmoothFrameRate=False";
                    minsmoothedframerate = "MinSmoothedFrameRate=5";
                    maxsmoothedframerate = "MaxSmoothedFrameRate=500";
                    frameratelimit = "FrameRateLimit=0.000000";
                    btn3.IsChecked = true;
                    btn3.IsEnabled = false;
                    vsync = "bUseVSync=False";
                }
                else
                {
                    btn3.IsEnabled = true;
                }
                UpdateEngineFile();
                UpdateGameUserSettingsFile();
            }
            void Clickbtn5(object sender2, RoutedEventArgs e2)
            {
                if ((bool)btn5.IsChecked)
                {
                    fullscreen = "FullScreen=True";
                    fullsmodecoll.Visibility = Visibility.Visible;
                    bd5.Height = 100;
                }
                else
                {
                    fullscreen = "FullScreen=False";
                    fullsmodecoll.Visibility = Visibility.Collapsed;
                    bd5.Height = 50;
                }
                UpdateGameUserSettingsFile();
            }
            void Clickfullsbtn(object sender2, RoutedEventArgs e2)
            {
                fullscreenmode = "FullscreenMode=0";
                fullsbtn.BorderThickness = new Thickness(4);
                fullswbtn.BorderThickness = new Thickness(0);
                windowedbtn.BorderThickness = new Thickness(0);
                fullsbtn.Background = new SolidColorBrush(Color.FromArgb(255, 18, 18, 18));
                fullswbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                windowedbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                UpdateGameUserSettingsFile();
            }
            void Clickfullswbtn(object sender2, RoutedEventArgs e2)
            {
                fullscreenmode = "FullscreenMode=1";
                fullsbtn.BorderThickness = new Thickness(0);
                fullswbtn.BorderThickness = new Thickness(4);
                windowedbtn.BorderThickness = new Thickness(0);
                fullsbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                fullswbtn.Background = new SolidColorBrush(Color.FromArgb(255, 18, 18, 18));
                windowedbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                UpdateGameUserSettingsFile();
            }
            void Clickwindowedbtn(object sender2, RoutedEventArgs e2)
            {
                fullscreenmode = "FullscreenMode=2";
                fullsbtn.BorderThickness = new Thickness(0);
                fullswbtn.BorderThickness = new Thickness(0);
                windowedbtn.BorderThickness = new Thickness(4);
                fullsbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                fullswbtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                windowedbtn.Background = new SolidColorBrush(Color.FromArgb(255, 18, 18, 18));
                UpdateGameUserSettingsFile();
            }
            void UpdateEngineFile()
            {
                if (engineinifile.Where(x => x.ToLower().Contains("[/script/engine.engine]")).Count() == 0)
                {
                    engineinifile.Add("[/script/engine.engine]");
                }
                if (engineinifile.Where(x => x.ToLower().Contains("bsmoothframerate")).Count() == 0)
                {
                    if ((bool)btn4.IsChecked)
                    {
                        engineinifile.Insert(engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("[/script/engine.engine]")).Last()) + 1, smootframerate);
                    }
                }
                else
                {
                    if ((bool)btn4.IsChecked)
                    {
                        engineinifile[engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("bsmoothframerate")).Last())] = smootframerate;
                    }
                    else
                    {
                        engineinifile.RemoveAt(engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("bsmoothframerate")).Last()));
                    }
                }
                if (engineinifile.Where(x => x.ToLower().Contains("minsmoothedframerate")).Count() == 0)
                {
                    if ((bool)btn4.IsChecked)
                    {
                        engineinifile.Insert(engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("[/script/engine.engine]")).Last()) + 1, minsmoothedframerate);
                    }
                }
                else
                {
                    if ((bool)btn4.IsChecked)
                    {
                        engineinifile[engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("minsmoothedframerate")).Last())] = minsmoothedframerate;
                    }
                    else
                    {
                        engineinifile.RemoveAt(engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("minsmoothedframerate")).Last()));
                    }
                }
                if (engineinifile.Where(x => x.ToLower().Contains("maxsmoothedframerate")).Count() == 0)
                {
                    if ((bool)btn4.IsChecked)
                    {
                        engineinifile.Insert(engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("[/script/engine.engine]")).Last()) + 1, maxsmoothedframerate);
                    }
                }
                else
                {
                    if ((bool)btn4.IsChecked)
                    {
                        engineinifile[engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("maxsmoothedframerate")).Last())] = maxsmoothedframerate;
                    }
                    else
                    {
                        engineinifile.RemoveAt(engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("maxsmoothedframerate")).Last()));
                    }
                }
                if (engineinifile.Where(x => x.ToLower().Contains("busevsync")).Count() == 0)
                {
                    engineinifile.Insert(engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("[/script/engine.engine]")).Last()) + 1, vsync);
                }
                else
                {
                    engineinifile[engineinifile.LastIndexOf(engineinifile.Where(x => x.ToLower().Contains("busevsync")).Last())] = vsync;
                }
                File.WriteAllLines(engineinipath, engineinifile);
            }
            void UpdateGameUserSettingsFile()
            {
                if (gameusersettingsinifile.Where(x => x.ToLower().Contains("gameusersettings]")).Count() > 0)
                {
                    if (gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("fullscreen=")).Count() == 0)
                    {
                        gameusersettingsinifile.Insert(gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.ToLower().Contains("gameusersettings]")).Last()) + 1, fullscreen);
                    }
                    else
                    {
                        gameusersettingsinifile[gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("fullscreen=")).Last())] = fullscreen;
                    }
                    if (gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("fullscreenmode=")).Count() == 0)
                    {
                        gameusersettingsinifile.Insert(gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.ToLower().Contains("gameusersettings]")).Last()) + 1, fullscreenmode);
                    }
                    else
                    {
                        gameusersettingsinifile[gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("fullscreenmode=")).Last())] = fullscreenmode;
                    }
                    if (gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("lastconfirmedfullscreenmode=")).Count() == 0)
                    {
                        gameusersettingsinifile.Insert(gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.ToLower().Contains("gameusersettings]")).Last()) + 1, $"LastConfirmed{fullscreenmode}");
                    }
                    else
                    {
                        gameusersettingsinifile[gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("lastconfirmedfullscreenmode=")).Last())] = $"LastConfirmed{fullscreenmode}";
                    }
                    if (gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("preferredfullscreenmode=")).Count() == 0)
                    {
                        gameusersettingsinifile.Insert(gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.ToLower().Contains("gameusersettings]")).Last()) + 1, $"Preferred{fullscreenmode}");
                    }
                    else
                    {
                        gameusersettingsinifile[gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("preferredfullscreenmode=")).Last())] = $"Preferred{fullscreenmode}";
                    }
                    if (gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("busevsync=")).Count() == 0)
                    {
                        gameusersettingsinifile.Insert(gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.ToLower().Contains("gameusersettings]")).Last()) + 1, vsync);
                    }
                    else
                    {
                        gameusersettingsinifile[gameusersettingsinifile.LastIndexOf(gameusersettingsinifile.Where(x => x.Trim().ToLower().Replace(" ", "").StartsWith("busevsync=")).Last())] = vsync;
                    }
                    File.WriteAllLines(gameusersettingsinipath, gameusersettingsinifile);
                }
            }
            void UpdateInputFile()
            {
                if (inputinifile.Where(x => x.Contains("[/Script/Engine.InputSettings]")).Count() == 0)
                {
                    inputinifile.Add("[/Script/Engine.InputSettings]");
                }
                if (inputinifile.Where(x => x.Contains("bEnableMouseSmoothing")).Count() == 0)
                {
                    inputinifile.Insert(inputinifile.IndexOf(inputinifile.Where(x => x.Contains("[/Script/Engine.InputSettings]")).First()) + 1, smoothingstring);
                }
                else
                {
                    inputinifile[inputinifile.LastIndexOf(inputinifile.Where(x => x.Contains("bEnableMouseSmoothing")).First())] = smoothingstring;
                }
                if (inputinifile.Where(x => x.Contains("bDisableMouseAcceleration")).Count() == 0)
                {
                    inputinifile.Insert(inputinifile.IndexOf(inputinifile.Where(x => x.Contains("[/Script/Engine.InputSettings]")).First()) + 1, acceleratestring);
                }
                else
                {
                    inputinifile[inputinifile.LastIndexOf(inputinifile.Where(x => x.Contains("bDisableMouseAcceleration")).First())] = acceleratestring;
                }
                File.WriteAllLines(inputinipath, inputinifile);
            }
            wp1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            wp1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            wp2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            wp2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            wp3.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            wp3.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            wp4.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            wp4.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            wp5.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            wp5.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            wp5.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            wp5.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            fullsmodecoll.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
            fullsmodecoll.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fullsmodecoll.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
            wp1.Children.Add(lbl1);
            wp1.Children.Add(btn1);
            Grid.SetColumn(btn1, 1);
            wp2.Children.Add(lbl2);
            wp2.Children.Add(btn2);
            Grid.SetColumn(btn2, 1);
            wp3.Children.Add(lbl3);
            wp3.Children.Add(btn3);
            Grid.SetColumn(btn3, 1);
            wp4.Children.Add(lbl4);
            wp4.Children.Add(btn4);
            Grid.SetColumn(btn4, 1);
            fullsmodecoll.Children.Add(fullsbtn);
            fullsmodecoll.Children.Add(fullswbtn);
            fullsmodecoll.Children.Add(windowedbtn);
            wp5.Children.Add(lbl5);
            wp5.Children.Add(btn5);
            wp5.Children.Add(fullsmodecoll);
            Grid.SetColumn(fullswbtn, 1);
            Grid.SetColumn(windowedbtn, 2);
            Grid.SetColumn(btn5, 1);
            Grid.SetRow(fullsmodecoll, 1);
            Grid.SetColumnSpan(fullsmodecoll, 2);
            bd1.Child = wp1;
            bd2.Child = wp2;
            bd3.Child = wp3;
            bd4.Child = wp4;
            bd5.Child = wp5;
            gameoptions.Children.Add(bd1);
            gameoptions.Children.Add(bd2);
            gameoptions.Children.Add(bd3);
            gameoptions.Children.Add(bd4);
            gameoptions.Children.Add(bd5);
        }
        catch { folderbtn.Visibility = Visibility.Collapsed; return; }
    }
}
