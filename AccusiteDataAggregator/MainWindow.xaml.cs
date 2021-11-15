﻿using AccusiteDataAggregator.Objects;
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

namespace AccusiteDataAggregator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string Version = "1.0.0";
        internal List<string> FunctionList { get; } = new() { "Get All Exc Data", "Get Exc By Serial Number" };

        public MainWindow()
        {
            InitializeComponent();
            Title = $"{Title} {Version}";
            Aggregator.Init();
            AddFunctions();
        }

        private void AddFunctions()
        {
            foreach(string function in FunctionList)
            {
                ListViewItem newItem = new() { Content = function };
                FunctionListView.Items.Add(newItem);
            }
            FunctionListView.SelectedItem = FunctionListView.Items.GetItemAt(0);
        }

        private void UpdateDetails(string item)
        {
            /// <summary>Updates Details Groupbox based on the item that was selected</summary>
            if (item.Contains("Get All Exc Data"))
            {
                DetailsBox.Text = "Gather Exc data for Accusite 2.0 trackers where all three of the following are present:" +
                    "\n1. Imager fixture \"No Windows\" (NW) run." +
                    "\n2. Imager fixture normal run." +
                    "\n3. Kronos normal run." +
                    $"\n\nData will be stored in: {Aggregator.Config.OutputFolderPath}";
                SetElementVisibility(false, false, true, false);
                DetailsButton1.Content = "Start";
                DetailsButton2.Content = "";
            }
            else if (item.Contains("Get Exc By Serial Number"))
            {
                DetailsBox.Text = "Gather Exc data for a specified Accusite 2.0 tracker where all three of the following are present:" +
                    "\n1. Imager fixture \"No Windows\" (NW) run." +
                    "\n2. Imager fixture normal run." +
                    "\n3. Kronos normal run." +
                    $"\n\nData will be stored in: {Aggregator.Config.OutputFolderPath}" +
                    "\nEnter the serial number and click \"Start\"";

                SetElementVisibility(true, false, true, false);
                DetailsButton1.Content = "Start";

            }
        }
        private void SetElementVisibility(bool box1 = false, bool box2 = false, bool button1 = false, bool button2 = false)
        {
            DetailsTextBox1.Visibility = box1 ? Visibility.Visible : Visibility.Hidden;
            DetailsTextBox2.Visibility = box2 ? Visibility.Visible : Visibility.Hidden;
            DetailsButton1.Visibility = button1 ? Visibility.Visible : Visibility.Hidden;
            DetailsButton2.Visibility = button2 ? Visibility.Visible : Visibility.Hidden;
        }

        private void DetailsButton1_Click(object sender, RoutedEventArgs e)
        {

        }
        private void DetailsButton2_Click(object sender, RoutedEventArgs e)
        {

        }
        private void FileMenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void FunctionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDetails((string)((ListViewItem)e.AddedItems[0]).Content);
        }
    }
}
