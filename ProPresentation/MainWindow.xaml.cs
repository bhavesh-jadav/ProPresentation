using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Runtime.Serialization;
using System.Windows.Interop;
using Microsoft.WindowsAPICodePack.Shell;
using System.Diagnostics;
using WPFCustomMessageBox;

namespace proPresentation
{
    
    public partial class MainWindow : Window
    {
        private bool mediaIsPlaying = false;
        private bool userIsDraggingSlider = false;
        StoredData data = new StoredData();
        private bool isNewFileCreated = false;
        private string dataFilePath = "C:\\";
        private string mediaFilePath = "C:\\";
        private bool isFullScreen = false;
        private double frameRate = 0.0, t, m, height, width;
        private TimeSpan startTime, endTime, frameTime;
        private DispatcherTimer dispatcherTimer, transitionTimer, reverseTransitionTimer;
        private int counter = 0;
		private bool isStartButtonPressed = false;
        private int type = 1;
        private BrushConverter convert = new BrushConverter();
        private bool canStartPresentation = false;
        private TimeSpan[] slidesStartTimes;
        private TimeSpan pos;

        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            grid_Main.Height = SystemParameters.WorkArea.Height - SystemParameters.WindowCaptionHeight;
            grid_Main.Width = SystemParameters.WorkArea.Width;
            /*dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            transitionTimer = new DispatcherTimer();
            transitionTimer.Tick += new EventHandler(transitionTimer_Tick);*/
            reverseTransitionTimer = new DispatcherTimer();
            reverseTransitionTimer.Tick += new EventHandler(reverseTransitionTimer_Tick);
        }

        private void reverseTransitionTimer_Tick(object sender, EventArgs e)
        {
            if(type == 1)
            {
                if(mePlayer.Position > startTime)
                {
                    t -= m;
                    mePlayer.Position = TimeSpan.FromMilliseconds(t);
                }
                else
                {
                    reverseTransitionTimer.Stop();
                    //dispatcherTimer.Start();
                }
            }
            else if(type == 2)
            {
                if (mePlayer.Position > frameTime)
                {
                    t -= m;
                    mePlayer.Position = TimeSpan.FromMilliseconds(t);
                }
                else
                    reverseTransitionTimer.Stop();
            }
        }

        private void transitionTimer_Tick(object sender, EventArgs e)
        {
            if(type == 1)
            {
                if(counter != 1)
                    mePlayer.Position = startTime;
                dispatcherTimer.Start();
            }
            else if (type == 2)
                mePlayer.Pause();

            transitionTimer.Stop();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //At the end of a Tick period, reset the MediaElement Position and Play again
            mePlayer.Position = startTime;
            mePlayer.Play();

            dispatcherTimer.IsEnabled = true;
        }


        void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                PlayerSlider.Minimum = 0;
                PlayerSlider.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                PlayerSlider.Value = mePlayer.Position.TotalSeconds;
            }
        }

        protected void LoopInput_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !IsNumberKey(e.Key) && !IsActionKey(e.Key) ;
        }

        private bool IsNumberKey(Key inKey)
        {
            if (inKey < Key.D0 || inKey > Key.D9)
            {
                if (inKey < Key.NumPad0 || inKey > Key.NumPad9)
                    return false;
            }
            return true;
        }

        private bool IsActionKey(Key inKey)
        {
            return inKey == Key.Delete || inKey == Key.Tab || inKey == Key.Back || inKey == Key.Return || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
        }

        private void LoopInput_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LoopInput.Text) && Convert.ToInt32(LoopInput.Text) != 0)
            {
                try
                {
                    data.TotalLoops = Convert.ToInt32(LoopInput.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("Please enter a valid data.");
                    LoopInput.Text = "";
                }
                var value = data.TotalLoops;
                try
                {
                    List<string> comboBoxList = new List<string>();
                    for (int i = 1; i <= value; i++)
                        comboBoxList.Add(i.ToString());
                    LoopComboBox.ItemsSource = comboBoxList;
                    LoopTypeStackPanel.Visibility = Visibility.Visible;
                    Separator2.Visibility = Visibility.Visible;
                    atobCheckbox.IsChecked = true;
                    //if (atobCheckbox.IsChecked == true)
                    //    AtoBStack.Visibility = Visibility.Visible;
                    //else
                    //    FrameStackPanel.Visibility = Visibility.Visible;
                    
                }
                catch (Exception)
                {
                    MessageBox.Show("The loop no. is very high to handle by your computer.\n Please try again with small no.");
                }
            }
            else
            {
                ifLoopInputNumberNotValid();
            }
        }

        private void ifLoopInputNumberNotValid()
        {
            AtoBStack.Visibility = Visibility.Hidden;
            FrameStackPanel.Visibility = Visibility.Hidden;
            LoopComboBox.ItemsSource = new List<String>(0);
            LoopTypeStackPanel.Visibility = Visibility.Hidden;
            Separator2.Visibility = Visibility.Hidden;
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files(*.xml)|*.xml";
            openFileDialog.InitialDirectory = @dataFilePath;
            openFileDialog.Title = "Open File";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                dataFilePath = openFileDialog.FileName;
                data.DataFilePath = dataFilePath;
                var result = deSerialized(dataFilePath);
                if(result)
                 constructWindow();
            }
        }

        private bool deSerialized(string filePath)
        {
            DataContractSerializer ser;
            FileStream fs = new FileStream(filePath, FileMode.Open);
            try
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                ser = new DataContractSerializer(typeof(StoredData));
                data = (StoredData)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                //reader.Close();
                fs.Close();
                var title = "Error opening file";
                var msg = "The file you selected is corrupted or does not contain all the information. Please select different file.";
                MessageBox.Show(msg + "   " + e.ToString(), title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }


        void constructWindow()
        {
            if (File.Exists(data.MediaFilePath))
            {
                try
                {
                    mePlayer.Source = new Uri(data.MediaFilePath);
                }
                catch (Exception)
                {
                    MessageBox.Show("The video file you selected is currupted or not a valid file. " +
                        "Please select another video file.", "Video file is not valid video file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                proPresentation.Title = "ProPresentation : " + dataFilePath;
                LoopInput.Text = data.TotalLoops.ToString();
                //List<string> comboBoxList = new List<string>();
                //for (int i = 1; i <= data.TotalLoops; i++)
                //    comboBoxList.Add(i.ToString());
                //LoopComboBox.ItemsSource = comboBoxList;
                SaveProjectButton.IsEnabled = true;
                StartPresentationButton.IsEnabled = true;
                OpenMediaButton.Visibility = System.Windows.Visibility.Hidden;
                mePlayer.Position = TimeSpan.FromMilliseconds(1);
                mePlayer.Pause();
                //mediaIsPlaying = false;
                InfoTextBox1.Visibility = System.Windows.Visibility.Hidden;
                InfoTextBox2.Visibility = System.Windows.Visibility.Hidden;
                if (data.TotalLoops != 0)
                    LoopTypeStackPanel.Visibility = Visibility.Visible;
                else
                    LoopTypeStackPanel.Visibility = Visibility.Hidden;
                OpenAnotherMediaButton.Visibility = System.Windows.Visibility.Visible;
                isNewFileCreated = true;
            }
            else
                videoFileNotFoundError();
        }

        private void videoFileNotFoundError()
        {
            var title = "Video File Not Found";
            var msg = "The video file used in this project is not present at " + data.MediaFilePath +
                " Please select Browse to open the video file or Select Cancel to open " +
                "another project or create a new project";
            var result = CustomMessageBox.ShowOKCancel(msg, title,"Browse", "Cancel", MessageBoxImage.Error);

            switch (result)
            {
                case MessageBoxResult.OK:
                    {
                        OpenMediaButton_Click(this, new RoutedEventArgs());
                        if(File.Exists(data.MediaFilePath))
                            constructWindow();
                    }
                    break;
            }
        }

        private void newFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save New File";
            saveFileDialog.InitialDirectory = @dataFilePath;
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.Filter = "XML files(*.xml)|*.xml";
            saveFileDialog.RestoreDirectory = true;
            //saveFileDialog.CheckFileExists = true;
            saveFileDialog.CheckPathExists = true;
            if(saveFileDialog.ShowDialog() == true)
            {
                string name = saveFileDialog.FileName;
                using (StreamWriter sw = new StreamWriter(name))
                    sw.WriteLine("");
                isNewFileCreated = true;
                dataFilePath = saveFileDialog.FileName;
                data = new StoredData();
                data.loopTypes = new Dictionary<int,LoopType>();
                InfoTextBox1.Foreground = Brushes.Gray;
                OpenMediaButton.IsEnabled = true;
                proPresentation.Title = "ProPresentation " + dataFilePath;
                data.DataFilePath = dataFilePath;
				InfoTextBox2.Foreground = Brushes.Black;
            }
            reset();
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string temp = Serialize(data);
            try
            {
                File.WriteAllText(dataFilePath, temp, Encoding.UTF8);
            }
            catch(Exception)
            {
                var msg = "There is an error saving file. Please make sure that file exists or file is not open in "
                          + "another program.";

                MessageBox.Show(msg, "Error saving file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            newFile_Click(sender, e);
        }
        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (SaveProjectButton.IsEnabled == true || isStartButtonPressed)
            {
                var result = beforeClosingApplication();
                if (result == true)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private bool beforeClosingApplication()
        {
            var msg = "Closing this applcation will loose the unsaved data. " +
                        "Plese select Save & exit to save the data before closing the application ";
            var title = "Save before closing";

            var result = CustomMessageBox.ShowYesNoCancel(msg, title, "Save & exit", "Exit", "Cancel", MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        Serialize(data);
                        Thread.Sleep(200);
                        return true;
                    }

                case MessageBoxResult.No: return true;
            }
            return false;
        }


        private void atobCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if(!isFullScreen)
                AtoBStack.Visibility = System.Windows.Visibility.Visible;
            FrameStackPanel.Visibility = System.Windows.Visibility.Collapsed;
        }


        private void singleFrame_Checked(object sender, RoutedEventArgs e)
        {
            FrameStackPanel.Visibility = System.Windows.Visibility.Visible;
            AtoBStack.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void TimeKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !IsNumberKey(e.Key) && !IsActionKey(e.Key);
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null) && !mediaIsPlaying;
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Play();
            mediaIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaIsPlaying; 
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
            mediaIsPlaying = false;
        }

        private void OpenMediaButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openMedia = new OpenFileDialog();
            openMedia.Title = "Open Media";
            openMedia.Filter = "Video files(*.mp4, *.flv, *.avi)|*.mp4;*.flv;*.avi";
            openMedia.InitialDirectory = @mediaFilePath;
            openMedia.RestoreDirectory = true;
            if (openMedia.ShowDialog() == true)
            {
                try
                {
                    mePlayer.Source = new Uri(openMedia.FileName);
                }
                catch(Exception)
                {
                    MessageBox.Show("The video file you selected is currupted or not a valid file. " +
                        "Please select another video file.", "Video file is not valid video file", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                mePlayer.Pause();
                mediaFilePath = openMedia.FileName;
                OpenAnotherMediaButton.Visibility = System.Windows.Visibility.Visible;
                OpenMediaButton.Visibility = System.Windows.Visibility.Collapsed;
                mePlayer.Visibility = System.Windows.Visibility.Visible;
                InfoTextBox1.Visibility = System.Windows.Visibility.Hidden;
                InfoTextBox2.Visibility = System.Windows.Visibility.Hidden;
                data.MediaFilePath = mediaFilePath;
                SaveProjectButton.IsEnabled = true;
                StartPresentationButton.IsEnabled = true;
                frameRate = calculateFrameRate();
                data.ReverseMediaFilePath = null;
            }

        }

        private double calculateFrameRate()
        {
            ShellFile shellFile = ShellFile.FromFilePath(data.MediaFilePath);
            return (double)(shellFile.Properties.System.Video.FrameRate.Value) / 1000;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (mePlayer.Source != null && mePlayer != null)
                mePlayer.Play();
        }

        private void PlayerSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void PlayerSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(PlayerSlider.Value);
        }

        private void PlayerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ProgressStatus.Text = TimeSpan.FromSeconds(PlayerSlider.Value).ToString(@"hh\:mm\:ss\.fff");
        }

        void fill()
        {
            if(data.TotalLoops != 0)
            {
                for(int i = 0; i < data.TotalLoops; i++)
                {
                    var looptype = new LoopType1();
                    looptype.StartTime = new TimeSpan(0, 1, 0);
                    looptype.EndTime = new TimeSpan(2, 2, 3);
                }
            }
        }

        private void mePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            EnterLoopStackPanel.Visibility = System.Windows.Visibility.Visible;
            checkEnteredTimeOnLostFocus(StartTimeTextBox, StartTimeTextBox.Text);
            checkEnteredTimeOnLostFocus(EndTimeTextBox, EndTimeTextBox.Text);
            frameRate = calculateFrameRate();
            LoopComboBox.SelectedIndex = 0;
        }

        void reset()
        {
            mePlayer.Stop();
            EnterLoopStackPanel.Visibility = System.Windows.Visibility.Hidden;
            LoopTypeStackPanel.Visibility = System.Windows.Visibility.Hidden;
            InfoTextBox1.Visibility = System.Windows.Visibility.Visible;
            InfoTextBox2.Visibility = System.Windows.Visibility.Visible;
            //InfoTextBox2.Foreground = Brushes.Black;
            mePlayer.Visibility = System.Windows.Visibility.Hidden;
            mediaIsPlaying = false;
            mePlayer.Source = null;
            userIsDraggingSlider = true;
            OpenAnotherMediaButton.Visibility = System.Windows.Visibility.Hidden;
            StartPresentationButton.Visibility = System.Windows.Visibility.Visible;
            OpenMediaButton.Visibility = System.Windows.Visibility.Visible;
            PlayerSlider.Minimum = 0;
            PlayerSlider.Maximum = 0;
            ProgressStatus.Text = "00:00:00";
            AtoBStack.Visibility = System.Windows.Visibility.Hidden;
            FrameStackPanel.Visibility = System.Windows.Visibility.Hidden;
            LoopInput.Text = "";
        }

        private void StartPresentationButton_Click(object sender, RoutedEventArgs e)
        {
            //mePlayer.Pause();
            //m = 1 / frameRate;
            //t = 17;
            //mePlayer.Position = TimeSpan.FromMinutes(17);
            //backwardTimer = new DispatcherTimer();
            //backwardTimer.Interval = TimeSpan.FromSeconds(m);
            //backwardTimer.Tick += new EventHandler(backwardTimer_Tick);
            //backwardTimer.Start();
            if (isNewFileCreated && File.Exists(data.MediaFilePath))
            {
                if (data.loopTypes.Count == data.TotalLoops && canStartPresentation)
                {
                    isStartButtonPressed = true;
                    TimeSpan pos = mePlayer.Position;
                    mePlayer.Pause();
                    mePlayer.Position = pos;
                    if (data.loopTypes[1].GetType() == typeof(LoopType1))
                        type = 1;
                    else
                        type = 2;

                    counter = 0;
                    for (int i = 0; i < data.TotalLoops; i++)
                    {
                        if (data.loopTypes[i + 1].GetType() == typeof(LoopType1))
                        {
                            LoopType1 loop = (LoopType1)data.loopTypes[i + 1];
                            if (mePlayer.Position <= loop.StartTime)
                                break;
                        }
                        else
                        {
                            LoopType2 loop = (LoopType2)data.loopTypes[i + 1];
                            if (mePlayer.Position <= loop.FrameTime)
                                break;
                        }
                        counter++;
                    }
                    //mePlayer.Pause();
                    //mePlayer.Position = TimeSpan.FromMilliseconds(1);
                    StartPresentationButton.Visibility = Visibility.Collapsed;
                    StopPresentationButton.Visibility = Visibility.Visible;
                    onStartPresentationButtonClick(false);
                    //transitionTimer.Stop();
                    //dispatcherTimer.Stop();
                }
                else
                {
                    var msg = "Please select loop type for all the loop and set the value for the same. "
                              + "Also make sure that value is correct and does not exceed the video time limit.";
                    var title = "Set value for all loops";

                    MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StopPresentationButton_Click(object sender, RoutedEventArgs e)
        {
            if (isNewFileCreated && File.Exists(data.MediaFilePath))
            {
                isStartButtonPressed = false;
                if (!isFullScreen)
                    StartPresentationButton.Visibility = Visibility.Visible;
                StopPresentationButton.Visibility = Visibility.Collapsed;
                Grid.SetZIndex(StopPresentationButton, 0);
                onStartPresentationButtonClick(true);
                mePlayer.Pause();
                //mePlayer.Position = mePlayer.NaturalDuration.TimeSpan;
                mediaIsPlaying = false;
                //dispatcherTimer.Stop();
                //transitionTimer.Stop();
                //reverseTransitionTimer.Stop();
            }
        }

        void onStartPresentationButtonClick(bool value)
        {
            PlayerSlider.IsEnabled = value;
            EnterLoopStackPanel.IsEnabled = value;
            AtoBStack.IsEnabled = value;
            atobCheckbox.IsEnabled = value;
            singleFrame.IsEnabled = value;
            SelectFrame.IsEnabled = value;
            PlayButton.IsEnabled = value;
            PauseButton.IsEnabled = value;
            OpenAnotherMediaButton.IsEnabled = value;
            SaveProjectButton.IsEnabled = value;
        }

        //private void backwardTimer_Tick(object sender, EventArgs e)
        //{
        //    t = t - m;
        //    mePlayer.Position = TimeSpan.FromSeconds(t);
        //    if (t <= 2)
        //    {
        //        backwardTimer.Stop();
        //        mePlayer.Play();
        //    }
        //}

        //void Serialize(StoredData data)
        //{
        //    var ser = new DataContractSerializer(typeof(StoredData));
        //    FileStream writer = new FileStream(dataFilePath, FileMode.Open);
        //    ser.WriteObject(writer, data);
        //    writer.Close();
        //}

        private void SetDataButton_Click(object sender, RoutedEventArgs e)
        {    
            setData();
        }

        private void LoopComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var i = Convert.ToInt32(LoopComboBox.SelectedItem);
            if (!data.loopTypes.ContainsKey(i))
            {
                if (atobCheckbox.IsChecked == true)
                {
                    StartTimeTextBox.Text = "";
                    EndTimeTextBox.Text = "";
                }
                else if(singleFrame.IsChecked == true)
                {
                    FrameTimeTextBlock.Text = "Frame time: not set";
                }
                StartTimeTextBox.BorderBrush = EndTimeTextBox.BorderBrush = convert.ConvertFromString("#FFABADB3") as Brush;
                canStartPresentation = true;
            }
            else
            {
                if (data.loopTypes[i].GetType() == typeof(LoopType1))
                {
                    atobCheckbox.IsChecked = true;
                    LoopType1 loop = (LoopType1)data.loopTypes[i];
                    StartTimeTextBox.Text = TimeSpan.FromSeconds(loop.StartTime.TotalSeconds).ToString(@"hh\:mm\:ss\.fffffff");
                    EndTimeTextBox.Text = TimeSpan.FromSeconds(loop.EndTime.TotalSeconds).ToString(@"hh\:mm\:ss\.fffffff");
                    checkEnteredTimeOnLostFocus(StartTimeTextBox, StartTimeTextBox.Text);
                    checkEnteredTimeOnLostFocus(EndTimeTextBox, EndTimeTextBox.Text);
                }
                else
                {
                    LoopType2 loop = new LoopType2();
                    loop = (LoopType2)data.loopTypes[i];
                    singleFrame.IsChecked = true;
                    FrameTimeTextBlock.Text = "Frame time: " + loop.FrameTime.ToString("G");
                    if(mePlayer.NaturalDuration.HasTimeSpan && mePlayer.NaturalDuration.TimeSpan.TotalSeconds < loop.FrameTime.TotalSeconds)
                    {
                        FrameTimeTextBlock.Foreground = Brushes.Red;
                        canStartPresentation = false;
                    }
                    else
                    {
                        FrameTimeTextBlock.Foreground = convert.ConvertFromString("#FF035461") as Brush;
                        canStartPresentation = true;
                    }
                }
            }
        }

        private void setData()
        {
            if(!String.IsNullOrWhiteSpace(StartTimeTextBox.Text) && !String.IsNullOrWhiteSpace(StartTimeTextBox.Text))
            {
                try
                {
                    string startTime = StartTimeTextBox.Text;
                    string endTime = EndTimeTextBox.Text;
                    TimeSpan time1 = TimeSpan.Parse(startTime);
                    TimeSpan time2 = TimeSpan.Parse(endTime);
                    LoopType1 looptype = new LoopType1();
                    looptype.StartTime = time1;
                    looptype.EndTime = time2;
                    var i = Convert.ToInt32(LoopComboBox.SelectedItem);
                    if (!data.loopTypes.ContainsKey(i))
                        data.loopTypes.Add(i, looptype);
                    else
                    {
                        data.loopTypes.Remove(i);
                        data.loopTypes.Add(i, looptype);
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        private void SelectFrame_Click(object sender, RoutedEventArgs e)
        {
            FrameTimeTextBlock.Foreground = convert.ConvertFromString("#FF035461") as Brush;
            TimeSpan frameTime = mePlayer.Position;
            LoopType2 looptype = new LoopType2();
            looptype.FrameTime = frameTime;
            var i = Convert.ToInt32(LoopComboBox.SelectedItem);
            FrameTimeTextBlock.Text = "Frame time : " + looptype.FrameTime.ToString();
            if(!data.loopTypes.ContainsKey(i))
            {
                data.loopTypes.Add(i, looptype);
            }
            else
            {
                data.loopTypes.Remove(i);
                data.loopTypes.Add(i, looptype);
            }
        }

        /*private void loopVideo()
        {
            mePlayer.Play();
            Task.Run(() =>
            {
                while (true)
                {
                    if (!isStartButtonPressed)
                        break;
                    this.Dispatcher.Invoke(() =>
                    {
                        double position = mePlayer.Position.TotalSeconds;
                        if (position >= endTime.TotalSeconds) // Stop position
                            mePlayer.Position = TimeSpan.FromSeconds(startTime.TotalSeconds); // Start position
                    });
                }
            });
        }

        private void transition()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (!isStartButtonPressed)
                        break;
                    this.Dispatcher.Invoke(() =>
                    {
                        //mePlayer.Play();
                        double position = mePlayer.Position.TotalSeconds;
                        if (position >= startTime.TotalSeconds) // Stop position
                        {
                            mePlayer.Pause();
                            //mePlayer.Position = TimeSpan.FromSeconds(startTime.TotalSeconds); // Start position
                        }
                    });
                }
            });
        }*/

        private void SetEndPosition()
        {
            mePlayer.Position = pos;
            Task.Run(() =>
            {
                while (true)
                {
                    if (!isStartButtonPressed)
                        break;
                    Dispatcher.Invoke(() =>
                    {
                        double position = mePlayer.Position.TotalSeconds;
                        if (position >= endTime.TotalSeconds)
                            SetStartPosition();
                    });
                    Thread.Sleep(100);
                }
            });
        }

        private void SetStartPosition()
        {
            mePlayer.Position = startTime;
        }

        private void SetReverseEndPosition()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (!isStartButtonPressed)
                        break;
                    Dispatcher.Invoke(() =>
                    {
                        double position = mePlayer.Position.TotalSeconds;
                        if (position <= startTime.TotalSeconds)
                            SetStartPosition();
                    });
                    Thread.Sleep(100);
                }
            });
        }

        private void SetReverseStartPosition()
        {
            mePlayer.Position = endTime;
        }

        private void proPresentation_KeyUp(object sender, KeyEventArgs e)
        {
            pos = mePlayer.Position;
            switch (e.Key)
            {
                case Key.F:
                {
                    if (!isFullScreen && isNewFileCreated && File.Exists(data.MediaFilePath))
                    {
                        isFullScreen = !isFullScreen;
                        height = grid_Main.Height;
                        width = grid_Main.Width;
                        this.Cursor = Cursors.None;
                        Visibility v = Visibility.Collapsed;
                        changeOnSwitchBetweenFullscreen(v);
                        Grid.SetRowSpan(mePlayer, 5);
                        Grid.SetColumnSpan(mePlayer, 2);
                        mePlayer.Margin = new Thickness(0);
                        grid_Main.Height = SystemParameters.PrimaryScreenHeight;
                        this.Background = new SolidColorBrush(Colors.Black);
                        this.WindowStyle = WindowStyle.None;
                        this.WindowState = WindowState.Maximized;
                        this.ResizeMode = ResizeMode.NoResize;
                        WinApi.SetWinFullScreen(new WindowInteropHelper(this).Handle);
                        
                    }
                    else if(isNewFileCreated && File.Exists(data.MediaFilePath))
                    {
                        isFullScreen = !isFullScreen;
                        this.WindowStyle = WindowStyle.SingleBorderWindow;
                        this.Cursor = Cursors.Arrow;
                        Grid.SetRowSpan(mePlayer, 4);
                        Grid.SetColumnSpan(mePlayer, 1);
                        grid_Main.Height = height;
                        grid_Main.Width = width;
                        mePlayer.Margin = new Thickness(10, 25, 10, 10);
                        this.WindowState = WindowState.Normal;
                        this.Background = new SolidColorBrush(Colors.White);
                        this.ResizeMode = ResizeMode.CanResize;
                        Visibility v = Visibility.Visible;
                        changeOnSwitchBetweenFullscreen(v);
                        
                    }
                }
                break;

                case Key.Space:
                {
                    if (!isStartButtonPressed)
                    {
                        if (mediaIsPlaying)
                        {
                            mediaIsPlaying = !mediaIsPlaying;
                            mePlayer.Pause();
                        }
                        else
                        {
                            mediaIsPlaying = !mediaIsPlaying;
                            mePlayer.Play();
                        }
                    }
                }
                break;

                case Key.Right:
                {
                    if (counter < data.TotalLoops && isStartButtonPressed)
                    {
                        //dispatcherTimer.Stop();
                        //transitionTimer.Stop();
                        counter++;
                        LoopComboBox.SelectedIndex = counter - 1;
                        if (data.loopTypes[counter].GetType() == typeof(LoopType1))
                        {
                            type = 1;
                            LoopType1 loop = (LoopType1)data.loopTypes[counter];
                            startTime = loop.StartTime;
                            endTime = loop.EndTime;
                            mePlayer.Play();
                            SetEndPosition();

                        }
                        else
                        {
                            type = 2;
                            LoopType2 loop = (LoopType2)data.loopTypes[counter];
                            startTime = loop.FrameTime;
                            endTime = loop.FrameTime;
                            mePlayer.Play();
                            SetEndPosition();

                        }
                    }
                    else
                        StopPresentationButton_Click(new object(), new RoutedEventArgs());
                }
                break;

                case Key.Left:
                {
                    if (isStartButtonPressed)
                    {
                        //dispatcherTimer.Stop();
                        //transitionTimer.Stop();
                        counter--;
                        LoopComboBox.SelectedIndex = counter - 1;
                    }
                    if (isStartButtonPressed && counter != 0 && data.ReverseMediaFilePath == null && enabelReverseTransition.IsChecked == false)
                    {
                        if (data.loopTypes[counter].GetType() == typeof(LoopType1))
                        {
                            LoopType1 loop = (LoopType1)data.loopTypes[counter];
                            startTime = loop.StartTime;
                            endTime = loop.EndTime;
                            mePlayer.Position = startTime;
                            mePlayer.Play();
                            SetEndPosition();
                        }
                        else
                        {
                            LoopType2 loop = (LoopType2)data.loopTypes[counter];
                            frameTime = loop.FrameTime;
                            mePlayer.Position = frameTime;
                            mePlayer.Pause();
                        }
                    }

                    else if(isStartButtonPressed && counter != 0 && enabelReverseTransition.IsChecked == true)
                    {
                        m = 1 / frameRate;
                        m *= 1000;
                        t = mePlayer.Position.TotalMilliseconds;
                        if (data.loopTypes[counter].GetType() == typeof(LoopType1))
                        {
                            type = 1;
                            LoopType1 loop = (LoopType1)data.loopTypes[counter];
                            startTime = loop.StartTime;
                            endTime = loop.EndTime;
                            reverseTransitionTimer.Interval = TimeSpan.FromMilliseconds(m);
                            mePlayer.Pause();
                            reverseTransitionTimer.Start();
                        }
                        else
                        {
                            type = 2;
                            LoopType2 loop = (LoopType2)data.loopTypes[counter];
                            frameTime = loop.FrameTime;
                            reverseTransitionTimer.Interval = TimeSpan.FromMilliseconds(m);
                            mePlayer.Pause();
                            reverseTransitionTimer.Start();
                        }
                    }
                    else
                        StartPresentationButton_Click(new object(), new RoutedEventArgs());
                }
                break;
                
                case Key.S:
                {
                    if (isStartButtonPressed)
                        StopPresentationButton_Click(new object(), new RoutedEventArgs());
                    else
                        StartPresentationButton_Click(new object(), new RoutedEventArgs());
                }
                break;
            }
        }

        void changeOnSwitchBetweenFullscreen(System.Windows.Visibility value)
        {
            RightBorder.Visibility = BottomBorder.Visibility = Separator1.Visibility = 
                Separator2.Visibility = MenuBar.Visibility = MediaPlayerBorder.Visibility = value;

            if(!isStartButtonPressed)
            {
                if (data.loopTypes[1].GetType() == typeof(LoopType1))
                    type = 1;
                else
                    type = 2;
            }

            stackPanelVisibility(EnterLoopStackPanel);
            stackPanelVisibility(LoopTypeStackPanel);
            stackPanelVisibility(BottomStackPanel);
            radioButtonVisibility(atobCheckbox);
            radioButtonVisibility(singleFrame);
            buttonVisibility(SaveProjectButton);
            if(!isStartButtonPressed)
                buttonVisibility(StartPresentationButton);
            if (isFullScreen)
            {
                //stackPanelVisibility(AtoBStack);
                //textBlockVisibility(FrameTimeTextBlock);
                //buttonVisibility(SelectFrame);
                AtoBStack.Visibility = Visibility.Collapsed;
                FrameTimeTextBlock.Visibility = Visibility.Collapsed;
                SelectFrame.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (type == 1)
                    stackPanelVisibility(AtoBStack);
                if (type == 2)
                {
                    textBlockVisibility(FrameTimeTextBlock);
                    buttonVisibility(SelectFrame);
                }
            }
        }

        private void stackPanelVisibility(StackPanel s)
        {
            switch (s.Visibility)
            {
                case Visibility.Collapsed: s.Visibility = Visibility.Visible;
                    break;
                case Visibility.Visible: s.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void radioButtonVisibility(RadioButton r)
        {
            switch (r.Visibility)
            {
                case Visibility.Collapsed: r.Visibility = Visibility.Visible;
                    break;
                case Visibility.Visible: r.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void textBlockVisibility(TextBlock t)
        {
            switch (t.Visibility)
            {
                case Visibility.Collapsed: t.Visibility = Visibility.Visible;
                    break;
                case Visibility.Visible: t.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void buttonVisibility(Button b)
        {
            switch (b.Visibility)
            {
                case Visibility.Collapsed: b.Visibility = Visibility.Visible;
                    break;
                case Visibility.Visible: b.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void textBoxVisibility(TextBox t)
        {
            switch (t.Visibility)
            {
                case Visibility.Collapsed: t.Visibility = Visibility.Visible;
                    break;
                case Visibility.Visible: t.Visibility = Visibility.Collapsed;
                    break;
            }

        }

        private void proPresentation_KeyDown(object sender, KeyEventArgs e)
        {
            if (File.Exists(data.MediaFilePath))
            {
                if (e.Key == Key.W)
                {
                    mePlayer.Pause();
                    m = 1 / frameRate;
                    mePlayer.Position -= TimeSpan.FromMilliseconds(m * 1000);
                }

                else if (e.Key == Key.E)
                {
                    mePlayer.Pause();
                    m = 1 / frameRate;
                    mePlayer.Position += TimeSpan.FromMilliseconds(m * 1000);
                }
            }
        }

        private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !isStartButtonPressed;
        }

        private void proPresentation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SaveProjectButton.IsEnabled == true || isStartButtonPressed)
            {
                var result = beforeClosingApplication();
                if (!result)
                    e.Cancel = !result;
            }
        }

        private void StartTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            checkEnteredTimeOnLostFocus(StartTimeTextBox, StartTimeTextBox.Text);
        }

        private void EndTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            checkEnteredTimeOnLostFocus(EndTimeTextBox, EndTimeTextBox.Text);
        }

        private void checkEnteredTimeOnLostFocus(TextBox textbox, string time)
        {
            TimeSpan Time;
            if (time.Length <= 8)
            {
                try
                {
                    Time = TimeSpan.ParseExact(time, @"hh\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture);
                    if(mePlayer.NaturalDuration.TimeSpan.TotalSeconds < Time.TotalSeconds)
                        throw new Exception();
                    textbox.BorderBrush = convert.ConvertFromString("#FFABADB3") as Brush;
                    canStartPresentation = true;
                }
                catch (Exception)
                {
                    textbox.BorderBrush = Brushes.Red;
                    canStartPresentation = false;
                }
            }
            else
            {
                try
                {
                    Time = TimeSpan.ParseExact(time, @"hh\:mm\:ss\.fffffff", System.Globalization.CultureInfo.InvariantCulture);
                    if (mePlayer.NaturalDuration.TimeSpan.TotalSeconds < Time.TotalSeconds)
                        throw new Exception();
                    textbox.BorderBrush = convert.ConvertFromString("#FFABADB3") as Brush;
                    canStartPresentation = true;
                }
                catch (Exception)
                {
                    textbox.BorderBrush = Brushes.Red;
                    canStartPresentation = false;
                }
            }
        }

        private void StartTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(EndTimeTextBox.Text))
                checkEnteredTimeOnTextChange(StartTimeTextBox.Text);
        }

        private void EndTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(StartTimeTextBox.Text))
                checkEnteredTimeOnTextChange(EndTimeTextBox.Text);
        }

        private void checkEnteredTimeOnTextChange(string time)
        {
            TimeSpan Time;
            if (time.Length <= 8)
            {
                try
                {
                    Time = TimeSpan.ParseExact(time, @"hh\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture);
                    if (mePlayer.NaturalDuration.TimeSpan.TotalSeconds < Time.TotalSeconds)
                        throw new Exception();
                    SetDataButton.IsEnabled = true;
                    canStartPresentation = true; ;
                }
                catch (Exception)
                {
                    SetDataButton.IsEnabled = false;
                    canStartPresentation = false;
                }
            }
            else
            {
                try
                {
                    Time = TimeSpan.ParseExact(time, @"hh\:mm\:ss\.fffffff", System.Globalization.CultureInfo.InvariantCulture);
                    if (mePlayer.NaturalDuration.TimeSpan.TotalSeconds < Time.TotalSeconds)
                        throw new Exception();
                    SetDataButton.IsEnabled = true;
                    canStartPresentation = true;
                }
                catch (Exception)
                {
                    SetDataButton.IsEnabled = false;
                    canStartPresentation = false;
                }
            }
        }

        private void SelectCurrentStartFrameButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan time = mePlayer.Position;
            StartTimeTextBox.Text = time.ToString();
        }

        private void SelectCurrentEndFrameButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan time = mePlayer.Position;
            EndTimeTextBox.Text = time.ToString();
        }

        

        private string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        private void openReverseVideoFile_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog openMedia = new OpenFileDialog();
            //openMedia.Title = "Open Media";
            //openMedia.Filter = "Video files(*.mp4, *.flv, *.avi)|*.mp4;*.flv;*.avi";
            //openMedia.InitialDirectory = @mediaFilePath;
            //openMedia.RestoreDirectory = true;
            //if (openMedia.ShowDialog() == true)
            //{
            //    try
            //    {
            //        mePlayerReverse.Source = new Uri(openMedia.FileName);
            //        data.ReverseMediaFilePath = openMedia.FileName;
            //        totalTime = mePlayerReverse.NaturalDuration.TimeSpan;
            //        mePlayerReverse.Pause();
            //    }
            //    catch (Exception)
            //    {
            //        MessageBox.Show("The video file you selected is currupted or not a valid file. " +
            //            "Please select another video file.", "Video file is not valid video file", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //}

            MessageBox.Show("Coming soon");
        }

        private void viewHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow help = new HelpWindow();
            help.Show();
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ProPresentation v1.0\nCreated By Bhavesh Jadav", "About", MessageBoxButton.OK, MessageBoxImage.Information);
            //MessageBox.Show(Directory.GetCurrentDirectory());
        }


        private void proPresentation_MouseUp(object sender, MouseButtonEventArgs e)
        {
            pos = mePlayer.Position;
            if (e.ChangedButton == MouseButton.Left)
            {
                if (isStartButtonPressed && counter >= 2)
                {
                    counter--;
                    LoopComboBox.SelectedIndex = counter - 1;
                }
                if (isStartButtonPressed && counter != 0 && data.ReverseMediaFilePath == null && enabelReverseTransition.IsChecked == false)
                {
                    if (data.loopTypes[counter].GetType() == typeof(LoopType1))
                    {
                        LoopType1 loop = (LoopType1)data.loopTypes[counter];
                        startTime = loop.StartTime;
                        endTime = loop.EndTime;
                        mePlayer.Position = startTime;
                        mePlayer.Play();
                        SetEndPosition();
                    }
                    else
                    {
                        LoopType2 loop = (LoopType2)data.loopTypes[counter];
                        frameTime = loop.FrameTime;
                        mePlayer.Position = frameTime;
                        mePlayer.Pause();
                    }
                }

                else if (isStartButtonPressed && counter != 0 && enabelReverseTransition.IsChecked == true)
                {
                    m = 1 / frameRate;
                    m *= 1000;
                    t = mePlayer.Position.TotalMilliseconds;
                    if (data.loopTypes[counter].GetType() == typeof(LoopType1))
                    {
                        type = 1;
                        LoopType1 loop = (LoopType1)data.loopTypes[counter];
                        startTime = loop.StartTime;
                        endTime = loop.EndTime;
                        reverseTransitionTimer.Interval = TimeSpan.FromMilliseconds(m);
                        mePlayer.Pause();
                        reverseTransitionTimer.Start();
                    }
                    else
                    {
                        type = 2;
                        LoopType2 loop = (LoopType2)data.loopTypes[counter];
                        frameTime = loop.FrameTime;
                        reverseTransitionTimer.Interval = TimeSpan.FromMilliseconds(m);
                        mePlayer.Pause();
                        reverseTransitionTimer.Start();
                    }
                }
                else
                    StartPresentationButton_Click(new object(), new RoutedEventArgs());
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (counter < data.TotalLoops && isStartButtonPressed)
                {
                    counter++;
                    LoopComboBox.SelectedIndex = counter - 1;
                    if (data.loopTypes[counter].GetType() == typeof(LoopType1))
                    {
                        type = 1;
                        LoopType1 loop = (LoopType1)data.loopTypes[counter];
                        startTime = loop.StartTime;
                        endTime = loop.EndTime;
                        mePlayer.Play();
                        SetEndPosition();

                    }
                    else
                    {
                        type = 2;
                        LoopType2 loop = (LoopType2)data.loopTypes[counter];
                        startTime = loop.FrameTime;
                        endTime = loop.FrameTime;
                        mePlayer.Play();
                        SetEndPosition();

                    }
                }
                else
                    StopPresentationButton_Click(new object(), new RoutedEventArgs());
            }
        }
    }
}
