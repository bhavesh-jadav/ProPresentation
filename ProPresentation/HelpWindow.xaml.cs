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
using System.Web;
using System.IO;

namespace proPresentation
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        private List<String> steps = new List<string>();
        private List<BitmapImage> stepsImages = new List<BitmapImage>();
        private int start, end, counter;
        private string wanted_path;

        public HelpWindow()
        {
            InitializeComponent();
            fillLists();
        }

        private void part1_Click(object sender, RoutedEventArgs e)
        {
            onAnyPartClick(0, 2, part1);
        }

        private void part2_Click(object sender, RoutedEventArgs e)
        {
            onAnyPartClick(3, 4, part2);
        }

        private void part3_Click(object sender, RoutedEventArgs e)
        {
            onAnyPartClick(5, 12, part3);
        }

        private void part4_Click(object sender, RoutedEventArgs e)
        {
            onAnyPartClick(13, 14, part4);
        }

        private void part5_Click(object sender, RoutedEventArgs e)
        {
            onAnyPartClick(15, 15, part5);
        }

        private void part6_Click(object sender, RoutedEventArgs e)
        {
            onAnyPartClick(16, 16, part6);
        }

        private void onAnyPartClick(int s, int e, Button b)
        {
            this.start = s;
            this.end = e;
            topTextBlock.Text = b.Content.ToString();
            startContentStackPanel.Visibility = Visibility.Collapsed;
            imageBorder.Visibility = bottomStackPanel.Visibility =
                imageBox.Visibility = stepsTextBlock.Visibility = Visibility.Visible;
            counter = start;
            stepsTextBlock.Text = steps[s];
            imageBox.Source = stepsImages[s];
            if (s == e)
            {
                previous.IsEnabled = false;
                next.IsEnabled = false;
            }
            else
            {
                previous.IsEnabled = false;
                next.IsEnabled = true;
            }
        }

        private void fillLists()
        {
            //How to create new project. 0 to 2
            steps.Add("Step 1: Select File>New. 'Select New File' dialog will appear. " +
                        "Browse to your desire location. Enter the file name and click on Save Button.");
            steps.Add("Step 2: After creating a new file click on Open Video Button to open a video file.");
            steps.Add("Step 3: After creating new project your window will look like following image. " +
                      "Make sure that your video plays properly by clicking play button.");
            
            //How to open project. 3 to 4
            steps.Add("Step 1: Select File>Open. 'Open File' dialog will appear. " +
                        "Browse to location where file is present and click on Open Button. Make sure that " + 
                        "chosen file is correct in case of some error.");
            steps.Add("Step 2: If you see folloing error, click on Browse button to browse your video or select appropriate option.");


            //Steps after creating or opening new project. 5 to 12
            steps.Add("Step 1: After opening or creating new file your window will look like following image.");
            steps.Add("Step 2: Enter the total number of loops in Enter no. of loops:");
            steps.Add("Step 3: Select loop number from drop down menu(default 1 will be selected) new loop no.");
            steps.Add("Step 4: Select appropriate loop type.");
            steps.Add("A to B For A to B you must have to enter the loop start time and loop end time in the format " + 
                      "hh:mm:ss\n" + 
                      "WARNING: YOU MUST PRESS 'SET DATA' TO SET THE VALUE OTHERWISE VALUE WON'T BE SET.");
            steps.Add("You can also select circular button near textbox to select current time of the video.");
            steps.Add("Single FrameSelect Select current frame button to select currnt position of video.");
            steps.Add("Step 5: Make sure to save(Ctrl + S) your project frequently otherwise your data won't be saved.");


            //Starting Presentation. 13 to 14
            steps.Add("To start presentation click on Start button or press S on keyboard.Press F to go " + 
                      "in fullscreen mode. You will get error if you did not specify the loop type " + 
                      "for all loops & did not set the value for the same.");
            steps.Add("To stop presentation click on Stop button or press S on keyboard.");


            //Keyboard shortcuts. 15
            steps.Add("Following are the key board shortcuts.");


            //Things to note before using this program. 16
            steps.Add("Keep following things in mind while using this program.");

            //Adding images
            //try
            //{
            wanted_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()) + "\\Images\\"; //System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
            //}
            //catch(Exception)
            //{
            //    wanted_path = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
            //}
            string[] images = Directory.GetFiles(wanted_path);

            foreach (string img in images)
            {
                var uri = new Uri(img);
                stepsImages.Add(new BitmapImage(uri));
            }
            imageBox.Source = stepsImages[5];
        }

        private void menu_Click(object sender, RoutedEventArgs e)
        {
            topTextBlock.Text = "Please select your option.";
            startContentStackPanel.Visibility = Visibility.Visible;
            imageBorder.Visibility = bottomStackPanel.Visibility =
                imageBox.Visibility = stepsTextBlock.Visibility = Visibility.Collapsed;
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            if(counter >= start && counter < end)
            {
                counter++;
                stepsTextBlock.Text = steps[counter];
                imageBox.Source = stepsImages[counter];
                if (counter == end)
                {
                    next.IsEnabled = false;
                    previous.IsEnabled = true;
                    counter = end;
                }
                else
                {
                    next.IsEnabled = true;
                    previous.IsEnabled = true;
                }
            }
        }

        private void previous_Click(object sender, RoutedEventArgs e)
        {
            if(counter > start && counter <= end)
            {
                counter--;
                stepsTextBlock.Text = steps[counter];
                imageBox.Source = stepsImages[counter];
                if (counter == start)
                {
                    previous.IsEnabled = false;
                    next.IsEnabled = true;
                    counter = start;
                }
                else
                {
                    previous.IsEnabled = true;
                    next.IsEnabled = true;
                }
            }
        }


    }
}
