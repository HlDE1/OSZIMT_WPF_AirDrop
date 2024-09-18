namespace WinDrop
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnSendClicked(object sender, EventArgs e)
        {

                FileSendBtn.Text = "Sending Files...";

            //hier jetzt code starten zum senden der ausgewählten datein zu dem ausgewählten gerät 


            SemanticScreenReader.Announce(FileSendBtn.Text);
        }
    }

}
