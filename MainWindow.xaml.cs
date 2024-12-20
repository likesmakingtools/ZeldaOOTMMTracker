using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ZeldaOOTMMTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SECTIONSPLITTER = "===========================================================================\n";
        private const string SPHERES = "Spheres";
        private const string LOCATIONS = "Location List";
        private const string GROUPEND = "\n\n";
        private const string FILESPOILER = "saved_details";
        private const string FILESAVE = "saved_items";
        private string spoiler = string.Empty;
        private string message = string.Empty;
        private bool isGameLoaded = false;

        private List<Location> locationList = new List<Location>();
        private List<CheckBox> cbList = new List<CheckBox>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoadSpoiler_Click(object sender, RoutedEventArgs e)
        {
            ClearMessage();
            ClearContent();

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text files (.txt)|*.txt";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                LoadSpoiler(dialog.FileName);
            }

            SetMessage("spoiler loaded");
            ClearSavedFiles();
        }

        private void btnLoadSave_Click(object sender, RoutedEventArgs e)
        {
            ClearMessage();
            if (File.Exists(FILESPOILER) && File.Exists(FILESAVE))
            {
                // load save game
                LoadSpoiler(FILESPOILER);
                LoadGame();
                SetMessage("game loaded");
            }
            else
            {
                SetMessage("nothing to load");
            }
        }

        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            ClearMessage();
            if (isGameLoaded)
            {
                // save game
                File.WriteAllText(FILESPOILER, spoiler);
                SaveGame();
                SetMessage("game data saved");
            }
            else
            {
                SetMessage("no game loaded");
            }
        }

        private void ClearMessage()
        {
            message = string.Empty;
            tbInfo.Text = message;
        }

        private void SetMessage(string newMessage)
        {
            message += newMessage;
            tbInfo.Text = message;
        }

        private void LoadSpoiler(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                spoiler = reader.ReadToEnd();
            }

            tbSpoiler.Text = spoiler;
            SetSections();

            isGameLoaded = true;
        }

        private void LoadGame()
        {
            Dictionary<string, bool> cbData = new Dictionary<string, bool>();
            string jsonData = string.Empty;
            using (StreamReader reader = new StreamReader(FILESAVE))
            {
                jsonData = reader.ReadToEnd();
            }
            cbData = JsonSerializer.Deserialize<Dictionary<string, bool>>(jsonData);

            foreach(CheckBox cb in cbList)
            {
                bool value = cbData[cb.Name];
                cb.IsChecked = value;
            }
        }

        private void SaveGame()
        {
            Dictionary<string, bool?> cbData = new Dictionary<string, bool?>();

            foreach(CheckBox cb in cbList)
            {
                cbData.Add(cb.Name, cb.IsChecked);
            }
            string jsonData = JsonSerializer.Serialize(cbData);
            File.WriteAllText(FILESAVE, jsonData);
        }

        private void SetSections()
        {
            string[] groups = spoiler.Split(SECTIONSPLITTER);
            bool hasSpheres = false;
            bool hasLocations = false;
            int sphereIndex = -1;
            int locationIndex = -1;

            for(int i = 0; i < groups.Length; i++)
            {
                if (groups[i].StartsWith(SPHERES))
                {
                    hasSpheres = true;
                    sphereIndex = i;
                }
                if (groups[i].StartsWith(LOCATIONS))
                {
                    hasLocations = true;
                    locationIndex = i;
                }
            }

            if (hasSpheres) tbSpheres.Text = groups[sphereIndex];
            if (hasLocations) ProcessLocations(groups[locationIndex]);
        }

        private void ProcessLocations(string locations)
        {
            string[] groups = locations.Split(GROUPEND);
            for (int i = 0; i < groups.Length; i++)
            {
                locationList.Add(new Location(i, groups[i]));
            }

            BuildContent();
        }

        private void ClearContent()
        {
            spOOT.Children.Clear();
            spMM.Children.Clear();
            spoiler = string.Empty;
            locationList = new List<Location>();
        }

        private void BuildContent()
        {
            BuildOOTPanel();
            BuildMMPanel();
        }

        private void BuildOOTPanel()
        {
            StackPanel outerPanel = spOOT;
            int index = -1;
            foreach(Location location in locationList)
            {
                if (location.IsOOT())
                {
                    index++;
                    Expander expander = new Expander();
                    expander.Name = $"ootE{index}";
                    expander.HorizontalAlignment = HorizontalAlignment.Left;
                    expander.Header = location.Name;
                    expander.ExpandDirection = ExpandDirection.Down;
                    expander.IsExpanded = true;
                    expander.Content = BuildVerticalStack(location.ID, "OOT", location.ItemLocations, location.Items);

                    outerPanel.Children.Add(expander);
                }
            }
        }

        private void BuildMMPanel()
        {
            StackPanel outerPanel = spMM;
            int index = -1;
            foreach (Location location in locationList)
            {
                if (!location.IsOOT())
                {
                    index++;
                    Expander expander = new Expander();
                    expander.Name = $"mmE{index}";
                    expander.HorizontalAlignment = HorizontalAlignment.Left;
                    expander.Header = location.Name;
                    expander.ExpandDirection = ExpandDirection.Down;
                    expander.IsExpanded = true;
                    expander.Content = BuildVerticalStack(location.ID, "MM", location.ItemLocations, location.Items);

                    outerPanel.Children.Add(expander);
                }
            }
        }

        private StackPanel BuildVerticalStack(string id, string game, List<string> itemLocations, List<string> items)
        {
            StackPanel vStack = new StackPanel();
            vStack.Orientation = Orientation.Vertical;
            for (int i = 0; i < items.Count; i++)
            {
                string name = $"cb{id}{game}item{i}";
                StackPanel hStack = new StackPanel();
                hStack.Orientation = Orientation.Horizontal;
                CheckBox cb = new CheckBox();
                cb.Name = name;
                cb.Margin = new Thickness(20, 0, 0, 0);
                TextBlock location = new TextBlock();
                location.Text = itemLocations[i];
                TextBlock item = new TextBlock();
                item.Text = $": {items[i]}";
                Binding binding = new Binding();
                binding.Source = cb;
                binding.Path = new PropertyPath("IsChecked");
                binding.Converter = new BooleanToVisibilityConverter();
                BindingOperations.SetBinding(item, TextBlock.VisibilityProperty, binding);

                hStack.Children.Add(cb);
                hStack.Children.Add(location);
                hStack.Children.Add(item);
                vStack.Children.Add(hStack);

                cbList.Add(cb);
            }

            return vStack;
        }

        public void ClearSavedFiles()
        {
            if (File.Exists(FILESPOILER))
            {
                File.Delete(FILESPOILER);
            }
            if (File.Exists(FILESAVE))
            {
                File.Delete(FILESAVE);
            }
        }
    }
}
