﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.Collections.Generic;
using System.Linq;

using System.Windows.Data;

using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Threading;


namespace WPFPageSwitch
{

    public partial class MainMenu : UserControl, ISwitchable
    {
        // HID class from HID reader sample for mifare cared 
        class HID
        {
            //*********************************************************************************************************
            // Define Constants, To Add "About Dialog Box" in System Menu
            //*********************************************************************************************************
            public const Int32 WM_SYSCOMMAND = 0x112;
            public const Int32 MF_SEPARATOR = 0x800;
            public const Int32 MF_BYPOSITION = 0x400;
            public const Int32 MF_STRING = 0x0;

            public const Int32 _SettingsSysMenuID = 1000;
            public const Int32 _AboutSysMenuID = 1001;


            //*********************************************************************************************************
            // Function Name: GetSystemMenu
            // In Parameter : hWnd - A handle to the window that will own a copy of the window menu.
            //                bRevert - The action to be taken. If this parameter is FALSE, GetSystemMenu returns a handle to the copy of the window menu currently in use. 
            // Out Parameter: ------
            // Description  : Enables the application to access the window menu for copying and modifying.
            //*********************************************************************************************************
            [DllImport("user32.dll")]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);


            //*********************************************************************************************************
            // Function Name: InsertMenu
            // In Parameter : hWnd - A handle to the menu to be changed. 
            //                Position - The menu item before which the new menu item is to be inserted, as determined by the uFlags parameter. 
            //                Flag - Controls the interpretation of the uPosition parameter and the content, appearance, and behavior of the new menu item.
            //                IDNewItem - The identifier of the new menu item or, if the uFlags parameter has the MF_POPUP flag set, a handle to the drop-down menu or submenu.
            //                newItem - The content of the new menu item.
            // Out Parameter: ---------
            // Description  : Inserts a new menu item into a menu, moving other items down the menu.
            //*********************************************************************************************************
            [DllImport("user32.dll")]
            public static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);


            // *********************************************************************************************************
            // Function Name: SCardEstablishContext
            // In Parameter : dwScope - Scope of the resource manager context.
            //                pvReserved1 - Reserved for future use and must be NULL
            //                pvReserved2 - Reserved for future use and must be NULL.
            // Out Parameter: phContext - A handle to the established resource manager context
            // Description  : Establishes context to the reader
            //*********************************************************************************************************
            [DllImport("WinScard.dll")]
            public static extern int SCardEstablishContext(uint dwScope,
            IntPtr notUsed1,
            IntPtr notUsed2,
            out IntPtr phContext);


            // *********************************************************************************************************
            // Function Name: SCardReleaseContext
            // In Parameter : phContext - A handle to the established resource manager context              
            // Out Parameter: -------
            // Description  :Releases context from the reader
            //*********************************************************************************************************
            [DllImport("WinScard.dll")]
            public static extern int SCardReleaseContext(IntPtr phContext);


            // *********************************************************************************************************
            // Function Name: SCardConnect
            // In Parameter : hContext - A handle that identifies the resource manager context.
            //                cReaderName  - The name of the reader that contains the target card.
            //                dwShareMode - A flag that indicates whether other applications may form connections to the card.
            //                dwPrefProtocol - A bitmask of acceptable protocols for the connection.  
            // Out Parameter: ActiveProtocol - A flag that indicates the established active protocol.
            //                hCard - A handle that identifies the connection to the smart card in the designated reader. 
            // Description  : Connect to card on reader
            //*********************************************************************************************************
            [DllImport("WinScard.dll")]
            public static extern int SCardConnect(IntPtr hContext,
            string cReaderName,
            uint dwShareMode,
            uint dwPrefProtocol,
            ref IntPtr hCard,
            ref IntPtr ActiveProtocol);


            // *********************************************************************************************************
            // Function Name: SCardDisconnect
            // In Parameter : hCard - Reference value obtained from a previous call to SCardConnect.
            //                Disposition - Action to take on the card in the connected reader on close.  
            // Out(Parameter)
            // Description  : Disconnect card from reader
            //*********************************************************************************************************
            [DllImport("WinScard.dll")]
            public static extern int SCardDisconnect(IntPtr hCard, int Disposition);


            //    *********************************************************************************************************
            // Function Name: SCardListReaders
            // In Parameter : hContext - A handle to the established resource manager context
            //                mszReaders - Multi-string that lists the card readers with in the supplied readers groups
            //                pcchReaders - length of the readerlist buffer in characters
            // Out Parameter: mzGroup - Names of the Reader groups defined to the System
            //                pcchReaders - length of the readerlist buffer in characters
            // Description  : List of all readers connected to system 
            //*********************************************************************************************************
            [DllImport("WinScard.dll", EntryPoint = "SCardListReadersA", CharSet = CharSet.Ansi)]
            public static extern int SCardListReaders(
              IntPtr hContext,
              byte[] mszGroups,
              byte[] mszReaders,
              ref UInt32 pcchReaders
              );


            // *********************************************************************************************************
            // Function Name: SCardState
            // In Parameter : hCard - Reference value obtained from a previous call to SCardConnect.
            // Out Parameter: state - Current state of smart card in  the reader
            //                protocol - Current Protocol
            //                ATR - 32 bytes buffer that receives the ATR string
            //                ATRLen - Supplies the length of ATR buffer
            // Description  : Current state of the smart card in the reader
            //*********************************************************************************************************
            [DllImport("WinScard.dll")]
            public static extern int SCardState(IntPtr hCard, ref IntPtr state, ref IntPtr protocol, ref Byte[] ATR, ref int ATRLen);


            // *********************************************************************************************************
            // Function Name: SCardTransmit
            // In Parameter : hCard - A reference value returned from the SCardConnect function.
            //                pioSendRequest - A pointer to the protocol header structure for the instruction.
            //                SendBuff- A pointer to the actual data to be written to the card.
            //                SendBuffLen - The length, in bytes, of the pbSendBuffer parameter. 
            //                pioRecvRequest - Pointer to the protocol header structure for the instruction ,Pointer to the protocol header structure for the instruction, 
            //                followed by a buffer in which to receive any returned protocol control information (PCI) specific to the protocol in use.
            //                RecvBuffLen - Supplies the length, in bytes, of the pbRecvBuffer parameter and receives the actual number of bytes received from the smart card.
            // Out Parameter: pioRecvRequest - Pointer to the protocol header structure for the instruction ,Pointer to the protocol header structure for the instruction, 
            //                followed by a buffer in which to receive any returned protocol control information (PCI) specific to the protocol in use.
            //                RecvBuff - Pointer to any data returned from the card.
            //                RecvBuffLen - Supplies the length, in bytes, of the pbRecvBuffer parameter and receives the actual number of bytes received from the smart card.
            // Description  : Transmit APDU to card 
            //*********************************************************************************************************
            [DllImport("WinScard.dll")]
            public static extern int SCardTransmit(IntPtr hCard, ref HiDWinscard.SCARD_IO_REQUEST pioSendRequest,
                                                               Byte[] SendBuff,
                                                               int SendBuffLen,
                                                               ref HiDWinscard.SCARD_IO_REQUEST pioRecvRequest,
                                                               Byte[] RecvBuff, ref int RecvBuffLen);


            // *********************************************************************************************************
            // Function Name: SCardGetStatusChange
            // In Parameter : hContext - A handle that identifies the resource manager context.
            //                value_TimeOut - The maximum amount of time, in milliseconds, to wait for an action.
            //                ReaderState -  An array of SCARD_READERSTATE structures that specify the readers to watch, and that receives the result.
            //                ReaderCount -  The number of elements in the rgReaderStates array.
            // Out Parameter: ReaderState - An array of SCARD_READERSTATE structures that specify the readers to watch, and that receives the result.
            // Description  : The current availability of the cards in a specific set of readers changes.
            //*********************************************************************************************************
            [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
            public static extern int SCardGetStatusChange(IntPtr hContext,
            int value_Timeout,
            ref HiDWinscard.SCARD_READERSTATE ReaderState,
            uint ReaderCount);

        }


        
        public MainMenu()
        {
            InitializeComponent();
        }


        // Forget fob button click 
        private void Foget_FOB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Try and Catch part are the release context button click. after release context 
            // switch to the Forget_FOB page 
            try
            {
                timer.Enabled = false;
                retval = HID.SCardReleaseContext(hContext);
                if (retval == 0)
                {
                    CardStatusTextBox.Text = "";

                    ConnectButton.IsEnabled = false;
                    sCardEstablishContextButton.IsEnabled = true;
                    SCardReleaseContextButton.IsEnabled = false;
                    CardStatusTextBox.Background = System.Windows.Media.Brushes.White;
                    CardStatusTextBox.Foreground = System.Windows.Media.Brushes.Black;
                    Disabled_Enabled_Controls();
                    Textcolorchange("> SCardReleaseContext" + "   Successful \n", System.Windows.Media.Brushes.Black);
                    rtb.ScrollToEnd();
                    timer.Enabled = false;


                }
                else
                {
                    Textcolorchange("> SCardReleaseContext" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n", System.Windows.Media.Brushes.Red);
                    rtb.ScrollToEnd();
                    timer.Enabled = false;
                }
                // After release the timer, then switch to the Forget_FOB page. 
                Switcher.Switch(new Forget_FOB());
            }
            catch (Exception Ex)
            {
                MessageBox.Show("In Main Menu Usercontrol page forget fob click, A handled exception just occurred: " + Ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // Global Variables from the HID reader Csharp_Samples_for_Mifare_Cards
        /**************************************************/
        //////////////////Global Variables//////////////////
        /**************************************************/
        //From the HID reader code 
        IntPtr hContext;                                        //Context Handle value
        String readerName;                                     //Global Reader Variable
        int retval;                                             //Return Value
        uint dwscope;                                           //Scope of the resource manager context
        Boolean IsAuthenticated;                                //Boolean variable to check the authentication
        Boolean release_flag;                                   //Flag to release 
        IntPtr hCard;                                           //Card handle
        IntPtr protocol;                                        //Protocol used currently
        Byte[] ATR = new Byte[33];                              //Array stores Card ATR
        int card_Type;                                          //Stores the card type
        Byte[] sendBuffer = new Byte[255];                        //Send Buffer in SCardTransmit
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x16)]
        // public byte receiveBuffer;
        Byte[] receiveBuffer = new Byte[255];                   //Receive Buffer in SCardTransmit
        int sendbufferlen, receivebufferlen;                    //Send and Receive Buffer length in SCardTransmit
        Byte bcla;                                             //Class Byte
        Byte bins;                                             //Instruction Byte
        Byte bp1;                                              //Parameter Byte P1
        Byte bp2;                                              //Parameter Byte P2
        Byte len;                                              //Lc/Le Byte
        Byte[] data = new Byte[255];                            //Data Bytes
        HiDWinscard.SCARD_READERSTATE ReaderState;              //Object of SCARD_READERSTATE
        int value_Timeout;                                      //The maximum amount of time to wait for an action
        uint ReaderCount;                                       //Count for number of readers
        String ReaderList;                                      //List Of Reader
        System.Object sender1;                                  //Object of the Sender
        System.Windows.RoutedEventArgs e1;                      //Object of the Event



        public delegate void DelegateTimer();                   //delegate of the Timer
        private System.Timers.Timer timer;                      //Object of the Timer
        public bool bTxtWrongInputChange;                       //Variable to check the wrong input in key textbox. Used in text change event




        string uid_temp = "";             // public string uid_temp
        //********************************************************
        //Function Name:MainMenu_loaded
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:First time when this usercontrol page is loaded.
        // It is from Loaded="MainMenu_Loaded" in MainMenu.xaml
        // from the HID reader Csharp_Samples_for_Mifare_Cards
        //********************************************************
        private void MainMenu_Loaded(object sender, RoutedEventArgs e)
        {

            uint pcchReaders = 0;
            int nullindex = -1;
            char nullchar = (char)0;
            dwscope = 2;

            // Establish context.
            retval = HID.SCardEstablishContext(dwscope, IntPtr.Zero, IntPtr.Zero, out hContext);
            retval = HID.SCardListReaders(hContext, null, null, ref pcchReaders);
            byte[] mszReaders = new byte[pcchReaders];

            // Fill readers buffer with second call.
            retval = HID.SCardListReaders(hContext, null, mszReaders, ref pcchReaders);

            // Populate List with readers.
            string currbuff = Encoding.ASCII.GetString(mszReaders);
            ReaderList = currbuff;
            int len = (int)pcchReaders;

            if (len > 0)
            {
                while (currbuff[0] != nullchar)
                {
                    nullindex = currbuff.IndexOf(nullchar);   // Get null end character.
                    string reader = currbuff.Substring(0, nullindex);
                    selectreadercombobox.Items.Add(reader);
                    //     selectreadercombobox.Items.Add(reader);

                    len = len - (reader.Length + 1);
                    currbuff = currbuff.Substring(nullindex + 1, len);
                }
            }

            //Tool Tip
            Tool_Tip();
            // In the MainMenu_Loaded page call sCardEstablishContextButton_Click 
            // Make Establish button click atomatically.
            sCardEstablishContextButton_Click(sender, e);

        }

        //*****************************************************************************
        //Function Name:WndProc
        //Input(Parameter) : System(Handler, Msg, wParam, lParam, Handled)
        //OutPutParameter: Int32(value)
        //Description:Handle event on click of About Dialog Box to Show Msg 
        //*****************************************************************************
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return IntPtr.Zero;
        }


        //********************************************************
        //Function Name: sCardEstablishContextButtonClick
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Establishing the context to the reader
        //From the HID reader Csharp_Samples_for_Mifare_Cards
        //********************************************************
        private void sCardEstablishContextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dwscope = 2;
                if (readerName != "" && readerName != null)
                {
                    retval = HID.SCardEstablishContext(dwscope, IntPtr.Zero, IntPtr.Zero, out hContext);
                    if (retval == 0)
                    {
                        IsAuthenticated = false;
                        sCardEstablishContextButton.IsEnabled = false;
                        SCardReleaseContextButton.IsEnabled = true;
                        ConnectButton.IsEnabled = true;
                        Textcolorchange("> SCardEstablishContext" + "  Successful \n", System.Windows.Media.Brushes.Black);
                        rtb.ScrollToEnd();
                        release_flag = true;
                    }
                    else
                    {
                        Textcolorchange("> SCardEstablishContext" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n", System.Windows.Media.Brushes.Red);
                        rtb.ScrollToEnd();
                        timer.Enabled = false;
                    }
                }
                else
                {
                    
                    Textcolorchange("> SCardEstablishContext" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n", System.Windows.Media.Brushes.Red);
                    rtb.ScrollToEnd();
                    timer.Enabled = false;

                    // if it failed, try to refresh this usercontrol 
                    Switcher.Switch(new MainMenu());
                }

                sender1 = sender;
                e1 = e;

                ////////////////////Timer//////////////////////////////

                // Creating a timer with a second interval.
                timer = new System.Timers.Timer(1000);
                // Hook up the Elapsed event for the timer.
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                timer.Enabled = true;
            }
            catch { }
        }

        //********************************************************
        //Function Name:OnTimedEvent
        //Input(Parameter) : source, e
        //OutPutParameter:-------
        //Description:Specify what you want to happen when the Elapsed event is raised.
        //From CSharp_Sample_for_Mifare_Cards 
        //********************************************************
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new DelegateTimer(timerWorkItem));
        }

        //********************************************************
        //Function Name: timerWorkItem
        //Input Parameter:-------
        //OutPutParameter:-------
        //Description:Perform action after time interval passed 
        //********************************************************
        private void timerWorkItem()
        {

            ReaderState.RdrName = readerName;
            ReaderState.RdrCurrState = HiDWinscard.SCARD_STATE_UNAWARE;
            ReaderState.RdrEventState = 0;
            ReaderState.UserData = "Mifare Card";
            value_Timeout = 0;
            ReaderCount = 1;

            if (SCardReleaseContextButton.IsEnabled == true)
            {
                if (ReaderList == "")
                {
                    CardStatusTextBox.Text = "SmartCard Removed";
                    CardStatusTextBox.Background = System.Windows.Media.Brushes.Red;
                    CardStatusTextBox.Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    retval = HID.SCardGetStatusChange(hContext, value_Timeout, ref ReaderState, ReaderCount);
                    if ((ReaderState.ATRLength == 0) || (retval != 0))
                    {
                        CardStatusTextBox.Text = "SmartCard Removed";
                        CardStatusTextBox.Background = System.Windows.Media.Brushes.Red;
                        CardStatusTextBox.Foreground = System.Windows.Media.Brushes.White;
                        DisconnectButton_Click(sender1, e1);
                        uidLabel.Content = "";

                    }

                    else
                    {
                        CardStatusTextBox.Text = "SmartCard Inserted";
                        CardStatusTextBox.Background = System.Windows.Media.Brushes.GreenYellow;
                        CardStatusTextBox.Foreground = System.Windows.Media.Brushes.Black;
                        
                        // After detect the SmartCard is inserted, then stop the timer. 
                        
                        timer.Stop();
                        timer.Enabled = false;

                        // Atomatically click the connect button. 
                        ConnectButton_Click(sender1, e1);
                        // After the connectin button, then click the realase context button 
                        SCardReleaseContextButton_Click(sender1, e1);                     
                    }

                }
            }

        }

        //********************************************************
        //Function Name:Disabled_Enabled_Controls
        //Description:Disabled and Enabled Controls and Label Content value null on runtime
        //********************************************************     

        private void Disabled_Enabled_Controls()
        {
            uidLabel.Content = "";
            atrLabel.Content = "";
            DisconnectButton.IsEnabled = false;
            IsAuthenticated = false;

        }

        //********************************************************
        //Function Name:sCardReleaseContextButtonClick
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Release the context
        //********************************************************
        private void SCardReleaseContextButton_Click(object sender, RoutedEventArgs e)
        {
            retval = HID.SCardReleaseContext(hContext);
            if (retval == 0)
            {
                CardStatusTextBox.Text = "";

                ConnectButton.IsEnabled = false;
                sCardEstablishContextButton.IsEnabled = true;
                SCardReleaseContextButton.IsEnabled = false;
                CardStatusTextBox.Background = System.Windows.Media.Brushes.White;
                CardStatusTextBox.Foreground = System.Windows.Media.Brushes.Black;
                Disabled_Enabled_Controls();
                Textcolorchange("> SCardReleaseContext" + "   Successful \n", System.Windows.Media.Brushes.Black);
                rtb.ScrollToEnd();
                timer.Enabled = false;

                // uid_temp is global varibale and if it is not null, then ValidateEmployee
                if (uid_temp.Length!=0)
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                        {
                            using (SqlCommand cmd = new SqlCommand("ValidateEmployee"))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = con;
                                cmd.Parameters.AddWithValue("@FobId", uid_temp);
                                con.Open();
                                SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                                returnParameter.Direction = ParameterDirection.ReturnValue;
                                cmd.ExecuteNonQuery();
                                int returnCd = (int)returnParameter.Value;
                                con.Close();
                                if (returnCd == 0)
                                {
                                    Switcher.Switch(new Enter_Amount(uid_temp));

                                }
                                if (returnCd == -1)
                                {
                                    MessageBox.Show("you have not is not setup in the Cafe Application!");

                                    // if not setup the Cafe Application, then still on the MainMenu usercontrol.
                                    Switcher.Switch(new MainMenu());

                                }
                                if (returnCd == -2)
                                {

                                    MessageBox.Show("you has not have Payroll Deductions enabled");
                                    Switcher.Switch(new MainMenu());

                                }
                                if (returnCd == -3)
                                {
                                    MessageBox.Show("you are not active");
                                    Switcher.Switch(new MainMenu());
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                
            }
            else
            {
                Textcolorchange("> SCardReleaseContext" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n", System.Windows.Media.Brushes.Red);
                rtb.ScrollToEnd();
                timer.Enabled = false;
                // after the Scard realse context failed what should we do 
                // refresh this user control page 
                Switcher.Switch(new MainMenu());                
            }

        }

        //********************************************************
        //Function Name: connectButtonClick
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Connect to SmartCard
        //********************************************************
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            {
                retval = HID.SCardConnect(hContext, readerName, HiDWinscard.SCARD_SHARE_SHARED, HiDWinscard.SCARD_PROTOCOL_T1,
                                 ref hCard, ref protocol
                                  );       //Command to connect the card ,protocol T=1
            }

            ReaderState.RdrName = readerName;
            ReaderState.RdrCurrState = HiDWinscard.SCARD_STATE_UNAWARE;
            ReaderState.RdrEventState = 0;
            ReaderState.UserData = "Mifare Card";
            value_Timeout = 0;
            ReaderCount = 1;

            if (retval == 0)
            {
                Textcolorchange("> SCardConnect" + "   Successful \n", System.Windows.Media.Brushes.Black);
                rtb.ScrollToEnd();
                retval = HID.SCardGetStatusChange(hContext, value_Timeout, ref ReaderState, ReaderCount);

                // After the Scard connect suceefully, Call the ATR_UID method to get the UID and ATR. 
                if (ReaderState.ATRValue[ReaderState.ATRLength - 0x6].Equals(1))
                {
                    card_Type = 1;
                    ATR_UID(card_Type);
                }
                else if (ReaderState.ATRValue[ReaderState.ATRLength - 0x6].Equals(2))
                {
                    card_Type = 2;
                    ATR_UID(card_Type);
                }
                else
                {
                    card_Type = 3;
                    ATR_UID(card_Type);
                }
            }

            else if (retval != 0 && DisconnectButton.IsEnabled == false)
            {
                Textcolorchange("> SCardConnect" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n", System.Windows.Media.Brushes.Red);
                rtb.ScrollToEnd();               
                timer.Enabled = true;               
            }
        }

        //********************************************************
        //Function Name: DisconnectButtonClick
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Disconnect the Smartcard
        //********************************************************
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            retval = HID.SCardDisconnect(hCard, HiDWinscard.SCARD_UNPOWER_CARD); //Command to disconnect the card
            if (retval == 0)
            {

                Disabled_Enabled_Controls();
                ConnectButton.IsEnabled = true;
                Textcolorchange("> SCardDisconnect" + "   Successful \n", System.Windows.Media.Brushes.Black);
                rtb.ScrollToEnd();
                timer.Enabled = true;
            }
            else if (retval != 0 && DisconnectButton.IsEnabled == true)
            {
                Textcolorchange("> SCardDisConnect" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n", System.Windows.Media.Brushes.Red);
                rtb.ScrollToEnd();
                timer.Enabled = true;

                // If the disconnect button fail
                Switcher.Switch(new MainMenu());  

            }

        }

        //********************************************************
        //Function Name:keynumberloadComboBox_SelectionChanged
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Event on keynumberloadComboBox text changed
        //********************************************************


        //********************************************************
        //Function Name:DataTextBox_TextChanged
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Event on DataTextBox text changed
        //********************************************************

        //********************************************************
        //Function Name:clearButton_Click
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Clear the return values shown in rich text box
        //********************************************************
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange trange;
            trange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            trange.Text = " ";
        }

        //********************************************************
        //Function Name:keyloadTextBox_KeyDown
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Event on keyloadTextBox KeyDown
        //********************************************************


        //********************************************************
        //Function Name:DataTextBox_KeyDown
        //Input(Parameter) : sender, e
        //OutPutParameter:-------
        //Description:Event on DataTextBox KeyDown
        //********************************************************
        private void DataTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

    

        //********************************************************
        //Function Name:ToolTip
        //Description:Add Tool Tip on Every Controls
        //********************************************************
        private void Tool_Tip()
        {
            selectreadercombobox.ToolTip = "Please select a contactless slot" + "\n" + "of an available reader";
            sCardEstablishContextButton.ToolTip = "The SCardEstablishContext function establishes the resource manager context (the scope)" + "\n" + "within functions can be used later.";
            SCardReleaseContextButton.ToolTip = "The SCardReleaseContext function closes an established resource manager context," + "\n" + "freeing any resources allocated under that context.";
            CardStatusTextBox.ToolTip = "Current card state: Inserted \\ Removed";
            ConnectButton.ToolTip = "This function establishes a connection, using a specific resource manager context," + "\n" + "between the calling application and a smart card contained by a specific reader.";
            DisconnectButton.ToolTip = "This function terminates a connection previously opened between the calling application" + "\n" + "and a smart card in the target reader.";
            rtb.ToolTip = "Return Values";
        }



        //********************************************************
        //Function Name:Textcolorchange
        //Input(Parameter) : strtext, color_red_black
        //OutPutParameter:-------
        //Description:Change the text color of return values if error occured 
        //********************************************************
        private void Textcolorchange(String strtext, Brush color_red_black)
        {
            TextRange trange;
            trange = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
            trange.Text = strtext;
            trange.ApplyPropertyValue(TextElement.ForegroundProperty, color_red_black);
        }

        //********************************************************
        //Function Name:card_Type_Identification
        //Description:Function to add blocks in block combo box based on card type
        //********************************************************


        //********************************************************
        //Function Name:ATR_UID
        //Description:Gives ATR and UID of the card 
        //********************************************************
        private void ATR_UID(int card_type)
        {
            HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = 0x2;
            sioreq.cbPciLength = 8;
            HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = 0x2;

            // String uid_temp;
            String atr_temp;
            String s;
            atr_temp = "";
            // uid_temp = "";
            s = "";
            StringBuilder hex = new StringBuilder(ReaderState.ATRValue.Length * 2);
            foreach (byte b in ReaderState.ATRValue)
                hex.AppendFormat("{0:X2}", b);
            atr_temp = hex.ToString();
            atr_temp = atr_temp.Substring(0, ((int)(ReaderState.ATRLength)) * 2);



            for (int k = 0; k <= ((ReaderState.ATRLength) * 2 - 1); k += 2)
            {
                s = s + atr_temp.Substring(k, 2) + " ";
            }

            atr_temp = s;

            bcla = 0xFF;
            bins = 0xCA;
            bp1 = 0x0;
            bp2 = 0x0;
            len = 0x0;
            sendBuffer[0] = bcla;
            sendBuffer[1] = bins;
            sendBuffer[2] = bp1;
            sendBuffer[3] = bp2;
            sendBuffer[4] = len;
            sendbufferlen = 0x5;
            receivebufferlen = 255;
            retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
            if (retval == 0)
            {
                if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                {
                    StringBuilder hex1 = new StringBuilder((receivebufferlen - 2) * 2);
                    foreach (byte b in receiveBuffer)
                        hex1.AppendFormat("{0:X2}", b);
                    uid_temp = hex1.ToString();
                    uid_temp = uid_temp.Substring(0, ((int)(receivebufferlen - 2)) * 2);
                }
                else
                {
                    ;
                }
            }
            else
            {
                Textcolorchange("> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n", System.Windows.Media.Brushes.Red);
                rtb.ScrollToEnd();
                timer.Enabled = false;

                // After SCardTransmit failed, then switch the to the Mainmenu usercontrol 
                Switcher.Switch(new MainMenu());
                
            }
            if (uid_temp == "")
            {
            }
            else
            {
                s = "";
                for (int k = 0; k <= ((receivebufferlen - 2) * 2 - 1); k += 2)
                {
                    s = s + uid_temp.Substring(k, 2) + " ";
                }
                uid_temp = s;
                uidLabel.Content = uid_temp;
               

            }
            if (atr_temp.Length <= 66)
                atrLabel.Content = "ATR=" + atr_temp;
            else
                atrLabel.Content = "ATR=" + atr_temp.Substring(1, 66) + "\n" + atr_temp.Substring(67, atr_temp.Length);

        }

        private void CardStatusTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }


        private void Convert_ID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SCardReleaseContextButton_Click(sender, e);
                Switcher.Switch(new Login());
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }


        #region Event For Child Window


        #endregion

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion



        public class HiDWinscard
        {
            // Context Scope

            public const int SCARD_STATE_UNAWARE = 0x0;

            //The application is unaware about the curent state, This value results in an immediate return
            //from state transition monitoring services. This is represented by all bits set to zero

            public const int SCARD_SHARE_SHARED = 2;

            // Application will share this card with other 
            // applications.

            //   Disposition

            public const int SCARD_UNPOWER_CARD = 2; // Power down the card on close

            //   PROTOCOL

            public const int SCARD_PROTOCOL_T0 = 0x1;                  // T=0 is the active protocol.
            public const int SCARD_PROTOCOL_T1 = 0x2;                  // T=1 is the active protocol.
            public const int SCARD_PROTOCOL_UNDEFINED = 0x0;

            //IO Request Control
            public struct SCARD_IO_REQUEST
            {
                public int dwProtocol;
                public int cbPciLength;
            }


            //Reader State

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct SCARD_READERSTATE
            {
                public string RdrName;
                public string UserData;
                public uint RdrCurrState;
                public uint RdrEventState;
                public uint ATRLength;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24, ArraySubType = UnmanagedType.U1)]
                public byte[] ATRValue;
            }
            //Card Type
            public const int card_Type_Mifare_1K = 1;
            public const int card_Type_Mifare_4K = 2;

        }

        //**************************************************************************
        //class for Hexidecimal to Byte and Byte to Hexidecimal conversion
        //**************************************************************************
        public class HexToBytenByteToHex
        {
            public HexToBytenByteToHex()
            {

                // constructor

            }
            public static int GetByteCount(string hexString)
            {
                int numHexChars = 0;
                char c;
                // remove all none A-F, 0-9, characters
                for (int i = 0; i < hexString.Length; i++)
                {
                    c = hexString[i];
                    if (IsHexDigit(c))
                        numHexChars++;
                }
                // if odd number of characters, discard last character
                if (numHexChars % 2 != 0)
                {
                    numHexChars--;
                }
                return numHexChars / 2; // 2 characters per byte
            }

            public static byte[] GetBytes(string hexString, out int discarded)
            {
                discarded = 0;
                string newString = "";
                char c;
                // remove all none A-F, 0-9, characters
                for (int i = 0; i < hexString.Length; i++)
                {
                    c = hexString[i];
                    if (IsHexDigit(c))
                        newString += c;
                    else
                        discarded++;
                }
                // if odd number of characters, discard last character
                if (newString.Length % 2 != 0)
                {
                    discarded++;
                    newString = newString.Substring(0, newString.Length - 1);
                }

                int byteLength = newString.Length / 2;
                byte[] bytes = new byte[byteLength];
                string hex;
                int j = 0;
                for (int i = 0; i < bytes.Length; i++)
                {
                    hex = new String(new Char[] { newString[j], newString[j + 1] });
                    bytes[i] = HexToByte(hex);
                    j = j + 2;
                }
                return bytes;
            }
            public static string ToString(byte[] bytes)
            {
                string hexString = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    hexString += bytes[i].ToString("X2");
                }
                return hexString;
            }
            public static bool InHexFormat(string hexString)
            {
                bool hexFormat = true;

                foreach (char digit in hexString)
                {
                    if (!IsHexDigit(digit))
                    {
                        hexFormat = false;
                        break;
                    }
                }
                return hexFormat;
            }

            public static bool IsHexDigit(Char c)
            {
                int numChar;
                int numA = Convert.ToInt32('A');
                int num1 = Convert.ToInt32('0');
                c = Char.ToUpper(c);
                numChar = Convert.ToInt32(c);
                if (numChar >= numA && numChar < (numA + 6))
                    return true;
                if (numChar >= num1 && numChar < (num1 + 10))
                    return true;
                return false;
            }
            private static byte HexToByte(string hex)
            {
                if (hex.Length > 2 || hex.Length <= 0)
                    throw new ArgumentException("hex must be 1 or 2 characters in length");
                byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                return newByte;
            }
        }

        public void selectreadercombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            readerName = (String)selectreadercombobox.SelectedItem;
            // "HID OMNIKEY 5427 CK CL 0";
            if (release_flag == true)
            {
                SCardReleaseContextButton_Click(sender, e);
            }
        }

        private void Terms_Conditions_Button_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new Terms_Conditions());
        }
    }



}



