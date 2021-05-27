﻿using MediaManager;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Platform.WPF;

namespace Jukebox.WpfCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        public MainWindow()
        {
            InitializeComponent();
            CrossMediaManager.Current.Init();
            Forms.Init();
            LoadApplication(new Jukebox.App());
        }
    }
}
