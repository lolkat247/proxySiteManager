﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Security.Principal;
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
using Microsoft.Win32;

namespace Site_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        storage holder = new storage();
        public MainWindow()
        {
            InitializeComponent();
            loadSerializedStorage();
            loadRadio();
            loadCurrentWhitelist();
        }

        private void loadCurrentWhitelist()
        {
            whitelistBox.ItemsSource = holder.whitelist;
        }

        private void loadRadio()
        {
            if (holder.proxyIsEnabled == true) { enableRadio.IsChecked = true; } else { enableRadio.IsChecked = false; }
            if (holder.proxyIsEnabled == false) { disableRadio.IsChecked = true; } else { disableRadio.IsChecked = false; }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (whitelistBox.SelectedIndex == -1)
            {
                // do nothing
            }
            else
            {
                holder.whitelist = holder.whitelist.Where(val => val != whitelistBox.SelectedItem.ToString()).ToArray();
            }
            loadCurrentWhitelist();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            string siteToAdd_String = siteToAdd_TextBox.Text;
            if (siteToAdd_String == "") { return; }
            string[] siteToAdd_StringArr = {siteToAdd_String};
            holder.whitelist = holder.whitelist.Concat(siteToAdd_StringArr).ToArray();
            loadCurrentWhitelist();
        }

        private void loadSerializedStorage()
        {
            FileStream s = new FileStream("whitelist.dat", FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                holder = (storage)formatter.Deserialize(s);
            }
            catch (System.Runtime.Serialization.SerializationException exception)
            {
                //nothing loaded
                System.Console.WriteLine(exception);
            }

            s.Close();
        }

        private void serializeStorage()
        {
            FileStream s = new FileStream("whitelist.dat", FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(s, holder);

            s.Close();
        }

        
        private void editIEProxyReg(string exclude, bool isEnabled)
        {
            System.AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            if (isEnabled)
            {
                RegistryKey key = Registry.Users.OpenSubKey(@"S-1-5-21-3794728147-2870682560-2744185785-1001\SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings", true);
                key.SetValue("MigrateProxy", 0x00000001);
                key.SetValue("ProxyEnable", 0x00000001);
                key.SetValue("ProxyServer", "127.0.0.1:80");
                key.SetValue("ProxyOverride", exclude);
                key.Close();
                Console.WriteLine("shoudl be on");
            }
            else
            {
                RegistryKey key = Registry.Users.OpenSubKey(@"S-1-5-21-3794728147-2870682560-2744185785-1001\SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings", true);
                key.SetValue("ProxyEnable", 0x00000000);
                key.Close();
                Console.WriteLine("should be off");
            }
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            // save state of radio buttons
            if (enableRadio.IsChecked == true)
            {
                holder.proxyIsEnabled = true;
            }
            else
            {
                if (disableRadio.IsChecked == true)
                {
                    holder.proxyIsEnabled = false;
                }
            }

            serializeStorage();

            string finalOut = "";
            foreach (var x in holder.whitelist)
            {
                finalOut += x + ";";
            }
            finalOut = finalOut.Remove(finalOut.Length - 1, 1);

            editIEProxyReg(finalOut, holder.proxyIsEnabled);
        }
    }

    [Serializable]
    public class storage
    {
        public string[] whitelist = { };
        public bool proxyIsEnabled;
    }
}
