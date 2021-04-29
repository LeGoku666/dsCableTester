using PI_TesterUniwersalnyKabli_V1.Database;
using PI_TesterUniwersalnyKabli_V1.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;


//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234238

namespace PI_TesterUniwersalnyKabli_V1.Okna
{
    public sealed partial class Page_Testing : Page
    {
        MainPage mainPage;

        enum TestProgress
        {
            NONE,
            PREP_COMMAND_LIST,
            SENDRECEIVE_COMMAND_LIST,
            CHECK_CABLE_CONNECTIONS,
            SUCCESS,
            FAIL,
            END
        }
        TestProgress tProgress = TestProgress.NONE;

        private List<int[]> L_ExpectedConnectionList = new List<int[]>();
        private List<int[]> L_ErrorsList = new List<int[]>();

        private SQL_CableTable cable;
        private bool isTestInProgress = false;
        private List<ComFrame> L_CommandsList = new List<ComFrame>();

        private SolidColorBrush color_Succes = new SolidColorBrush(Windows.UI.Colors.YellowGreen);
        private SolidColorBrush color_Error = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush color_None = new SolidColorBrush(Windows.UI.Colors.White);

        //  int counter = 0;
        private static int reveivedCommands = 0;
        public delegate void Del_receivedMsg();
        public Del_receivedMsg receiverMsg = ReceivedDelegate;

        //stat cytaty
        private int testsDoneCounter = 0;
        private int showQuoteOn = 30;
        private bool isTimeForAuthor = false;

        public Page_Testing(MainPage mainPage)
        {
            this.InitializeComponent();

            this.mainPage = mainPage;

            ResetText();
        }

        public List<Grid> ShowHELP(bool show)
        {
            if (show == true)
            {
                Grid_Help.Visibility = Visibility.Visible;
            }
            else
            {
                Grid_Help.Visibility = Visibility.Collapsed;
            }

            return new List<Grid>() { Grid_help1, Grid_help2, Grid_help3, Grid_help4, Grid_help5, Grid_help6, Grid_help7 };
        }

        public void ShowQuote()
        {
            mainPage.page_Top.ShowQuote();

            Grid_Quote.Visibility = Visibility.Visible;

            TextBlock__Sentence.Opacity = 0;
            TextBlock__Author.Opacity = 0;

            DoubleAnimation _OpacityAnimation = new DoubleAnimation();

            Storyboard _Storyboard = new Storyboard();
            _Storyboard.Children.Add(_OpacityAnimation);
            Storyboard.SetTargetProperty(_OpacityAnimation, "Image.Opacity");
            Storyboard.SetTarget(_Storyboard, Image_BlackQuote);

            _OpacityAnimation.From = 0;
            _OpacityAnimation.To = 1;

            _Storyboard.Begin();

            //todo pokazanie napisów
            mainPage.backgroundWorker.StartQuoterTimerText();
        }

        public void ShowTextsQuote()
        {
            if (isTimeForAuthor == false)
            {
                ShowQuoteSentence();
                isTimeForAuthor = true;
                mainPage.backgroundWorker.StartQuoterTimerText();
            }
            else
            {
                ShowQuoteAuthor();
                isTimeForAuthor = false;
            }
        }

        // odbiera inkrementacje postępu odebranych komend
        public static void ReceivedDelegate()
        {
            reveivedCommands++;
        }

        public void CheckReceivedCounter()
        {
            ProgressBar_Test.Maximum = L_CommandsList.Count();

            ProgressBar_Test.Value = reveivedCommands;
            double rec = reveivedCommands;
            TextBlock_Counter.Text = ((int)((rec / (L_CommandsList.Count() == 0 ? 1 : L_CommandsList.Count())) * 100)).ToString();
        }

        public void ResetText()
        {
            TextBlock_TestCom.Foreground = color_None;

            TextBlock_Status.Text = "";
            TextBlock_TimeLeft.Text = "";
            TextBlock_Errors.Text = "";

            TextBlock_Stat.Visibility = Visibility.Collapsed;
            TextBlock_Time.Visibility = Visibility.Collapsed;
            TextBlock_Error.Visibility = Visibility.Collapsed;
            TextBlock_Errors.Visibility = Visibility.Collapsed;

            TextBlock_TestCom.Visibility = Visibility.Collapsed;

            Image_Result.Visibility = Visibility.Collapsed;
            ShowHideResultImageAsync();
            ShowErrors(false);
            // reveivedCommands = 0;
            ProgressBar_Test.Value = 0;
            TextBlock_Counter.Text = "0";
        }

        public void SetCable(SQL_CableTable cable)
        {
            UpdateStatusText();

            this.cable = cable;

            TextBlock_Name.Text = cable.Name;
            TextBlock_Symbol.Text = cable.Symbol;
        }

        public void StartButtonIsEnabled(bool enabled)
        {
            if (IsTestInProgress() == false)
                Button_Start.IsEnabled = enabled;
        }

        public bool IsTestInProgress()
        {
            return isTestInProgress;
        }

        public void AllCommandsAreReceived()
        {
            tProgress = TestProgress.CHECK_CABLE_CONNECTIONS;
        }

        public void CommandsReceivedFailed()
        {
            tProgress = TestProgress.END;
        }

        public void UpdateTimePassFromLastTest(int timePass)
        {
            SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.White);
            int timeM = timePass / 60;
            int timeS = timePass - (timeM * 60);

            if (timeM > 0)
                TextBlock_TimeLeft.Text = "0" + timeM + ":" + (timeS < 10 ? "0" : "") + timeS;
            else
                TextBlock_TimeLeft.Text = timeS + "s";

            if (timeM >= 5)
            {
                color = new SolidColorBrush(Windows.UI.Colors.DarkRed);
                TextBlock_TimeLeft.Text = "Dawno, dawno temu...";
            }
            else if (timeM >= 4)
            {
                color = new SolidColorBrush(Windows.UI.Colors.Red);
            }
            else if (timeM >= 3)
            {
                color = new SolidColorBrush(Windows.UI.Colors.OrangeRed);
            }
            else if (timeM >= 2)
            {
                color = new SolidColorBrush(Windows.UI.Colors.Orange);
            }
            else if (timeM >= 1)
            {
                color = new SolidColorBrush(Windows.UI.Colors.Yellow);
            }

            TextBlock_TimeLeft.Foreground = color;
        }

        private void ShowErrors(bool show = false)
        {
            //  Grid_Errors.Children.Clear();
            Grid gridErrors = new Grid();
            ScrollVIewer_Errors.Content = gridErrors;

            if (show == true)
            {
                TextBlock_Errors.Text = L_ErrorsList.Count.ToString();

                int topDist = 0;

                foreach (int[] item in L_ErrorsList)
                {
                    string pins = "";
                    int connections = 0;
                    for (int i = 1; i < item.Length; i++)
                    {
                        if (item[i] == 1)
                        {
                            connections++;
                            if (i + 1 < item.Length)
                                pins += i + ",";
                            else
                                pins += i;
                        }
                    }

                    string textErr = "PIN NR. " + item[0] + ": ";

                    if (connections == 0)
                        textErr += "BRAK POŁĄCZENIA";
                    else if (connections == 1)
                        textErr += "BŁĘDNE POŁĄCZENIE";
                    else if (connections > 1)
                        textErr += "ZWARCIE";

                   // textErr += "\n- " + pins;

                    TextBlock error = new TextBlock
                    {
                        Text = textErr,
                        Foreground = new SolidColorBrush(Windows.UI.Colors.White),
                        FontSize = 24,
                        Padding = new Thickness(0, topDist, 0, 0)
                    };

                    //Grid_Errors.Children.Add(error);
                    gridErrors.Children.Add(error);

                    topDist += 30;
                }

                Button butt = new Button
                {
                    Content = "POKAŻ SZCZEGÓŁY",
                    IsEnabled = true,
                    FontSize = 24,
                    Height = 60,
                    Width = 300,
                    Margin = new Thickness { Top = topDist+10 },
                    Background = new SolidColorBrush(Windows.UI.Colors.Red),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                butt.Click += ShowErrorsDetails;

                gridErrors.Children.Add(butt);
            }
        }

        private void ShowErrorsDetails(object sender, RoutedEventArgs e)
        {
            Page_ErrorsDetails page_errorsDetails = new Page_ErrorsDetails(mainPage, L_ExpectedConnectionList, L_ErrorsList);

            mainPage.SetMenuTitle(MainPage.MENU.ERRORS_DETAILS);  
            mainPage.LoadPage(page_errorsDetails);
        }

        private async Task ShowHideResultImageAsync(bool show = false, string imgName = "succes")
        {
            DoubleAnimation _OpacityAnimation = new DoubleAnimation();

            Storyboard _Storyboard = new Storyboard();
            _Storyboard.Children.Add(_OpacityAnimation);
            Storyboard.SetTargetProperty(_OpacityAnimation, "Image.Opacity");
            Storyboard.SetTarget(_Storyboard, Image_Result);

            if (show == true)
            {
                Windows.Storage.StorageFolder roamingFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + imgName + ".png"));
                using (var fileStream = (await file.OpenAsync(Windows.Storage.FileAccessMode.Read)))
                {
                    var bitImg = new BitmapImage();
                    bitImg.SetSource(fileStream);
                    Image_Result.Source = bitImg;

                    _OpacityAnimation.From = 0;
                    _OpacityAnimation.To = 1;

                    // Image_Result.Visibility = Visibility.Visible;
                }
            }
            else
            {
                _OpacityAnimation.From = 1;
                _OpacityAnimation.To = 0;
            }
            // Image_Result.Visibility = Visibility.Collapsed;

            _Storyboard.Begin();
        }

        private void Button_BACK_Click(object sender, RoutedEventArgs e)
        {
            ResetText();
            tProgress = TestProgress.NONE;
            mainPage.backgroundWorker.STOP_TimerCountingTimeFromLastTest();

            if (mainPage.lastMenu == MainPage.MENU.WYBOR_KABLA)
            {
                mainPage.page_SelectCable.FillListWithPrinters();
                mainPage.SetMenuTitle(MainPage.MENU.WYBOR_KABLA);
                mainPage.LoadPage(mainPage.page_SelectCable);
            }
            else if (mainPage.lastMenu == MainPage.MENU.WYBOR_KABLA_PO_SYMBOLU)
            {
                mainPage.SetMenuTitle(MainPage.MENU.WYBOR_KABLA_PO_SYMBOLU);
                mainPage.LoadPage(mainPage.page_SelectCableBySymbol);
            }
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            if (isTestInProgress == false)
            {
                mainPage.backgroundWorker.STOP_TimerCountingTimeFromLastTest();
                ResetText();

               // Button_Start.IsEnabled = false;
                Button_BACK.IsEnabled = false;
                RunBackground();

                //Button_STOP.IsEnabled = true;

                if (++testsDoneCounter > showQuoteOn)
                {
                    testsDoneCounter = 0;

                    mainPage.page_Top.MakeQuoteActive();
                    FillQuotesText();
                }

                Button_Start.Content = "STOP";
            }
            else
            {
                if (testsDoneCounter > 0)
                    testsDoneCounter--;

                StopTest();
            }
        }

        private void UpdateStatusText()
        {
            if (tProgress == TestProgress.SUCCESS)
            {
                SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.YellowGreen);

                TextBlock_Status.Text = "TEST OK!";
                TextBlock_Status.Foreground = color;

                TextBlock_Stat.Visibility = Visibility.Visible;
                TextBlock_Time.Visibility = Visibility.Visible;

                mainPage.backgroundWorker.START_TimerCountingTimeFromLastTest();

                Image_Result.Visibility = Visibility.Visible;
                ShowHideResultImageAsync(true);
                ShowErrors(false);
                mainPage.uart.UartSend("PB=LEDSTATUS=1");
            }
            else if (tProgress == TestProgress.FAIL)
            {
                SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.Red);

                TextBlock_Status.Text = "! BŁĄD !";
                TextBlock_Status.Foreground = color;

                TextBlock_Stat.Visibility = Visibility.Visible;
                TextBlock_Time.Visibility = Visibility.Visible;
                TextBlock_Error.Visibility = Visibility.Visible;
                TextBlock_Errors.Visibility = Visibility.Visible;

                mainPage.backgroundWorker.START_TimerCountingTimeFromLastTest();

                Image_Result.Visibility = Visibility.Visible;
                ShowHideResultImageAsync(true, "error");
                ShowErrors(true);
                mainPage.uart.UartSend("PB=LEDSTATUS=2");
            }
            else if (tProgress == TestProgress.NONE)
            {
                TextBlock_Status.Text = "";
                TextBlock_TimeLeft.Text = "";
                //ShowHideResultImageAsync();
                mainPage.backgroundWorker.STOP_TimerCountingTimeFromLastTest();
                ShowErrors(false);
                ProgressBar_Test.Value = 0;
                TextBlock_Counter.Text = "0";
            }
        }

        // ############################################################################################
        private async void RunBackground()
        {
            //int counter = 0;

            tProgress = TestProgress.PREP_COMMAND_LIST;
            isTestInProgress = true;
            while (isTestInProgress == true)
            {
                await Task.Run(() => DoWork());
                await Task.Delay(100);  // ms

                // Update the UI with results
                //TextBlock_Counter.Text = counter++.ToString();
                //ProgressBar_Test.Value = counter;
                CheckReceivedCounter();
                ProgressRing_Test.IsActive = true;
                TextBlock_TestCom.Visibility = Visibility.Visible;

                switch (tProgress)
                {
                    case TestProgress.NONE:
                        TextBlock_TestCom.Text = "none";
                        break;
                    case TestProgress.PREP_COMMAND_LIST:
                        // TextBlock_TestCom.Text = "PrepComm";
                        break;
                    case TestProgress.SENDRECEIVE_COMMAND_LIST:
                        TextBlock_TestCom.Text = "TESTUJE";
                        break;
                    case TestProgress.CHECK_CABLE_CONNECTIONS:
                        // TextBlock_TestCom.Text = "CHECK";
                        break;
                    case TestProgress.SUCCESS:
                        TextBlock_TestCom.Foreground = color_Succes;
                        TextBlock_TestCom.Text = "TEST OK";
                        //if (counter < 97)
                        //    counter = 97;
                        //if (ProgressBar_Test.Value >= 100D)
                        StopTest();
                        break;
                    case TestProgress.FAIL:
                        TextBlock_TestCom.Foreground = color_Error;
                        TextBlock_TestCom.Text = "! ERROR !";
                        //if (counter < 97)
                        //    counter = 97;
                        //if (ProgressBar_Test.Value >= 100D)
                        StopTest();
                        break;
                    case TestProgress.END:
                        //TextBlock_TestCom.Text = "KUNIEC";
                        StopTest();
                        break;
                    default:
                        TextBlock_TestCom.Text = "default";
                        break;
                }
            }

            // TextBlock_Counter.Text = "0";
            //ProgressBar_Test.Value = 0;
            reveivedCommands = 0;
            ProgressRing_Test.IsActive = false;
            TextBlock_TestCom.Visibility = Visibility.Collapsed;
        }

        private async Task DoWork()
        {
            switch (tProgress)
            {
                case TestProgress.NONE:
                    break;
                case TestProgress.PREP_COMMAND_LIST:
                    PrepareCommandListAndPushIt();
                    break;
                case TestProgress.SENDRECEIVE_COMMAND_LIST:

                    break;
                case TestProgress.CHECK_CABLE_CONNECTIONS:
                    TestConnections();
                    break;
                case TestProgress.SUCCESS:

                    break;
                case TestProgress.FAIL:

                    break;
                case TestProgress.END:
                    break;
                default:
                    break;
            }
        }

        private void PrepareCommandListAndPushIt()
        {
            if (cable.PinsConnections != null && cable.PinsConnections != "")
            {
                string[] token_CutAPins = cable.PinsConnections.Split(';');

                bool isNotEmpty = false;
                foreach (string item in token_CutAPins)
                {
                    if (item != "")
                        isNotEmpty = true;
                }

                if (isNotEmpty == false)
                {
                    tProgress = TestProgress.FAIL;
                }
                else
                {
                    L_CommandsList.Clear();

                    //Policz ile mamy członow w srodku dla progres barra
                    CountStepsForProgressBar(token_CutAPins);

                    if (token_CutAPins.Length == cable.Pins_A)
                    {
                        for (int i = 0; i < cable.Pins_A; i++)
                        {
                            string[] tokenPinsIn = token_CutAPins[i].Split(',');
                            int mask = 0;

                            foreach (string item in tokenPinsIn)
                            {
                                string[] tokenNumber = item.Split('B');

                                if (tokenNumber.Length < 2)
                                    continue;

                                int pinToSet = 1 << int.Parse(tokenNumber[1]) - 1;
                                mask |= pinToSet;
                            }

                            L_CommandsList.Add(new ComFrame { SAddress = "1", SCommand = "SP", SIsValue = true, SValue = i + 1, IsSended = false, IsReceived = false, NrSended = 0, ReceivedMaskB = mask });
                            L_CommandsList.Add(new ComFrame { SAddress = "1", SCommand = "A", SIsValue = false, IsSended = false, IsReceived = false, NrSended = 0 });
                            L_CommandsList.Add(new ComFrame { SAddress = "2", SCommand = "A", SIsValue = false, IsSended = false, IsReceived = false, NrSended = 0 });
                        }
                    }

                    if (mainPage.sendReceiveQueue.actualComSet == SendReceiveQueue.Com_Queue.NONE)
                    {
                        mainPage.sendReceiveQueue.actualComSet = SendReceiveQueue.Com_Queue.CABLE_TEST;
                        mainPage.sendReceiveQueue.atcualComSetStatus = SendReceiveQueue.Com_QueueResult.WORKING;
                        tProgress = TestProgress.SENDRECEIVE_COMMAND_LIST;

                        mainPage.sendReceiveQueue.RunQueue(L_CommandsList);
                    }
                    else
                    {
                        tProgress = TestProgress.FAIL;
                    }
                }
            }
            else
            {
                tProgress = TestProgress.FAIL;
            }
        }

        private void TestConnections()
        {
            L_ErrorsList.Clear();

            if (L_CommandsList != null && L_CommandsList.Count > 0)
            {
                bool isAllOK = true;

                for (int i = 0; i < L_CommandsList.Count; i += 3)
                {
                    int maskB = L_CommandsList[i].ReceivedMaskB;
                    int receivedMask = L_CommandsList[i + 2].MaskReceived;

                    if (maskB != receivedMask)
                    {
                        isAllOK = false;

                        string goodBinary = Convert.ToString(maskB, 2);
                        string receivedBinary = Convert.ToString(receivedMask, 2);

                        // numer pinu testowanego oraz oczekiwane połączenia
                        int[] expected = new int[goodBinary.Length + 1];   // oczekiwane połączenia plus 1 na pin testowany
                        expected[0] = L_CommandsList[i].SValue;            // testowany pin

                        // numer pinu testowanego oraz ilosc mozliwych bledow
                        int[] error = new int[receivedBinary.Length + 1];   // ilosc mozliwych bledow plus 1 na pin testowany
                        error[0] = L_CommandsList[i].SValue;                // testowany pin

                        //************** odwrocenie kolejnosci
                        char[] goodBinaryByte = new char[32];
                        char[] receivedBinaryByte = new char[32];

                        int nrOfBits = goodBinary.Length;
                        for (int g = 0; g < 32; g++)
                        {
                            if (nrOfBits-- > 0)
                                goodBinaryByte[g] = goodBinary[nrOfBits];
                            else
                                goodBinaryByte[g] = '0';
                        }
                        nrOfBits = receivedBinary.Length;
                        for (int g = 0; g < 32; g++)
                        {
                            if (nrOfBits-- > 0)
                                receivedBinaryByte[g] = receivedBinary[nrOfBits];
                            else
                                receivedBinaryByte[g] = '0';
                        }
                        //**********************************************************

                        // Piny podlaczone/zwarte do naszego pinu
                        for (int j = 0; j < receivedBinary.Length; j++)
                        {
                            //  if (goodBinaryByte[j] != receivedBinaryByte[j])
                            //  {
                            // if (receivedBinaryByte[j] != '0')
                            error[j + 1] = int.Parse(receivedBinaryByte[j].ToString());
                            //  }
                        }
                        L_ErrorsList.Add(error);

                        // Oczekiwane połączenia DOBRE
                        for (int x = 0; x < goodBinary.Length; x++)
                        {
                            expected[x + 1] = int.Parse(goodBinaryByte[x].ToString());
                        }
                        L_ExpectedConnectionList.Add(expected);
                    }
                }

                if (isAllOK == true)
                    tProgress = TestProgress.SUCCESS;
                else
                    tProgress = TestProgress.FAIL;
            }
        }

        // To chyba nie uzywane jest
        private void CountStepsForProgressBar(string[] tok)
        {
            int allStep = 0;

            foreach (string item in tok)
            {
                string[] tokenPinsIn = item.Split(',');
                allStep += tokenPinsIn.Length;
            }
        }

        private void FillQuotesText()
        {
            string quote = Cycaty.GetQuote();

            string[] token = quote.Split(';');

            if (token != null)
            {
                TextBlock__Sentence.Text = token[0];

                if (token[0].Length > 150)
                    TextBlock__Sentence.FontSize = 34;
                else
                    TextBlock__Sentence.FontSize = 48;

                if (token.Length > 1)
                    TextBlock__Author.Text = token[1];
                else
                    TextBlock__Author.Text = "";
            }
        }

        private void StopTest()
        {
            if (IsTestInProgress() == true)
            {
                mainPage.sendReceiveQueue.actualComSet = SendReceiveQueue.Com_Queue.NONE;
                isTestInProgress = false;
                mainPage.slotsConnection.IsABConnected();

                //Button_STOP.IsEnabled = false;
                Button_BACK.IsEnabled = true;

                UpdateStatusText();

                Button_Start.Content = "START";
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Grid_Quote.Visibility = Visibility.Collapsed;
            TextBlock__Sentence.Opacity = 0;
            TextBlock__Author.Opacity = 0;

            mainPage.page_Top.ResetQuoteBlacknes();
        }

        private void ShowQuoteSentence()
        {
            // TextBlock__Sentence.Visibility = Visibility.Visible;

            DoubleAnimation _OpacityAnimation = new DoubleAnimation();

            Storyboard _Storyboard = new Storyboard();
            _Storyboard.Children.Add(_OpacityAnimation);
            Storyboard.SetTargetProperty(_OpacityAnimation, "TextBlock.Opacity");
            Storyboard.SetTarget(_Storyboard, TextBlock__Sentence);

            _OpacityAnimation.From = 0;
            _OpacityAnimation.To = 1;

            _Storyboard.Begin();
        }

        private void ShowQuoteAuthor()
        {
            // TextBlock__Sentence.Visibility = Visibility.Visible;

            DoubleAnimation _OpacityAnimation = new DoubleAnimation();

            Storyboard _Storyboard = new Storyboard();
            _Storyboard.Children.Add(_OpacityAnimation);
            Storyboard.SetTargetProperty(_OpacityAnimation, "TextBlock.Opacity");
            Storyboard.SetTarget(_Storyboard, TextBlock__Author);

            _OpacityAnimation.From = 0;
            _OpacityAnimation.To = 1;

            _Storyboard.Begin();
        }
    }
}
