using System.Windows;

namespace Aldurcraft.WurmAssistantLauncher2.Views
{
    /// <summary>
    /// Interaction logic for TestView.xaml
    /// </summary>
    public partial class TestView : Window
    {
        public TestView()
        {
            InitializeComponent();
        }

        //[DataContract]
        //class SomeData
        //{
        //    [DataMember] public int SomeStuff2;
        //}

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //var settings = new PersistentObject<SomeData>(new SomeData());
            //settings.SetFilePathAndLoad(GeneralHelper.PathCombineWithCodeBasePath("WaLauncherSettings.xml"));
            //settings.Value.SomeStuff2 = 2; // TextData.Text;
            //settings.Save();
        }

        private void TestView_OnLoaded(object sender, RoutedEventArgs e)
        {
            //var settings = new PersistentObject<SomeData>(new SomeData());
            //settings.SetFilePathAndLoad(GeneralHelper.PathCombineWithCodeBasePath("WaLauncherSettings.xml"));
            //TextData.Text = settings.Value.SomeStuff2.ToString();
        }
    }
}
