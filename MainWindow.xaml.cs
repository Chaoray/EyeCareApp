using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using EyeCareApp.Properties;

namespace EyeCareApp {
    public partial class MainWindow : Window {
        private static Random random = new Random();
        private static readonly double DEFAULT_TIMER_INTERVAL = 20;

        private System.Timers.Timer notificationTimer;
        private NotifyIcon nIcon;
        private bool canSetInterval = false;
        private RegistryManager settingsRM, startUpRM;

        public MainWindow() {
            settingsRM = new RegistryManager(
                Registry.CurrentUser,
                $"SOFTWARE\\{StringConstants.APP_NAME}",
                true);
            startUpRM = new RegistryManager(Registry.CurrentUser,
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                false);
            this.notificationTimer = new System.Timers.Timer();
            this.notificationTimer.Elapsed += new ElapsedEventHandler(NotificationTimer_Elapsed);

            if (!isRegistryInitialized()) {
                initRegistry();
            }
            InitializeComponent();
            initializeControlsFromRegKeys();
            initNotifyIcon();
        }

        private void NotificationSwitch_Toggled(object sender, RoutedEventArgs e) {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch == null) return;

            bool isOn = toggleSwitch.IsOn;
            if (isOn) {
                notificationTimer.Start();
            } else {
                notificationTimer.Stop();
            }
        }

        private void OpenAtStartSwitch_Toggled(object sender, RoutedEventArgs e) {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch == null) return;

            bool isOn = toggleSwitch.IsOn;
            if (isOn) {
                try {
                    startUpRM.set(StringConstants.APP_NAME, System.Reflection.Assembly.GetExecutingAssembly().Location);
                    // this may be blocked by anti-virus and throw exception
                    toggleSwitch.IsOn = true;
                } catch {
                    toggleSwitch.IsOn = false;
                }
            } else {
                startUpRM.delete(StringConstants.APP_NAME);
                toggleSwitch.IsOn = false;
            }
        }

        private void LoopMinuteNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) {
            double minutes = sender.Value;
            if (double.IsNaN(minutes)) {
                System.Windows.MessageBox.Show(
                    "Please Enter a Valid Number",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (minutes <= 1) {
                System.Windows.MessageBox.Show(
                    "Please Enter a Number Greater Than 1",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            this.notificationTimer.Interval = minutes * 60 * 1000;
            if (this.canSetInterval) {
                settingsRM.set("Interval", minutes.ToString());
            }
        }

        private void NotificationTimer_Elapsed(object sender, ElapsedEventArgs e) {
            int index = random.Next(StringConstants.NOTIFICATION_TEXTS.Length);
            new ToastContentBuilder()
                .AddText(StringConstants.NOTIFICATION_TEXTS[index])
                .AddText("Eye Care App")
                .Show();
        }

        private void initializeControlsFromRegKeys() {
            NumberBox numBox = this.FindName("LoopMinuteNumberBox") as NumberBox;
            if (numBox != null) {
                numBox.Value = double.Parse(settingsRM["Interval"]);
            }
            canSetInterval = true;

            ToggleSwitch openAtStartSwitch = this.FindName("OpenAtStartSwitch") as ToggleSwitch;
            if (startUpRM.has(StringConstants.APP_NAME)) {
                if (openAtStartSwitch != null) {
                    openAtStartSwitch.IsOn = true;
                }
            }
        }

        private bool isRegistryInitialized() {
            return settingsRM.has("Interval") && startUpRM.has(StringConstants.APP_NAME);
        }


        private void initNotifyIcon() {
            this.nIcon = new NotifyIcon();
            nIcon.Icon = Properties.Resources.AppIcon;
            nIcon.Text = StringConstants.APP_NAME;
            nIcon.Visible = true;
            nIcon.Click += new EventHandler(this.nIcon_Click);
        }

        private void nIcon_Click(object Sender, EventArgs e) {
            showWindow();
        }

        private void showWindow() {
            if (this.WindowState == WindowState.Minimized) {
                this.Show();
                this.WindowState = WindowState.Normal;
            }
            this.Activate();
            this.Focus();
        }

        private void initRegistry() {
            settingsRM.set("Interval", DEFAULT_TIMER_INTERVAL.ToString());
        }

        private void Window_StateChanged(object sender, EventArgs e) {
            if (this.WindowState == WindowState.Minimized) {
                this.Hide();
            }
        }
    }
}