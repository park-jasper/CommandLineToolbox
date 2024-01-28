namespace ChecksumApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var args = Environment.GetCommandLineArgs();
            string fileName = string.Empty;
            if (args.Length != 0)
            {
                fileName = args[0];
            }

            MainPage = new MainPage()
            {
                BindingContext = new MainViewModel(fileName),
            };
        }
    }
}