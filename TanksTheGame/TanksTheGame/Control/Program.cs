using System;
using System.Windows.Forms;
using System.Threading;
using Microsoft.VisualBasic;

namespace TanksTheGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static string message;

        [STAThread]
        static void Main(String []args)
        {
            if (MessageBox.Show("Switch to 127.0.0.1 instead of network IPs " + ComLink.inIP + " and " + ComLink.outIP + "?",
                "T.A.N.K.S.", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ComLink.inIP = ComLink.outIP = "127.0.0.1";
            }

           
                //try join
                ComLink.send("JOIN#");

                String msg;

                //String msg = ComLink.receive();
                //if (msg.StartsWith("S")) //accepted
                //{
                //    MessageHandler.doAccept(msg);
                //    new GameWindow().Run();

                    //continue monitoring server responses
                while (true)
                {
                    msg = ComLink.receive();
                    if (msg != null)
                    {
                        MessageHandler.handle(msg);
                    }
                }

                //}
                //else
                //{
                //    Console.WriteLine("Error! Unable to join the game");
                //}
            
        }

        //launches GUI
        public static void runGame()
        {
            new GameWindow().Run();
        }

        //closes all threads and the game
        public static void exit()
        {
            Environment.Exit(0);
        }

        //show message on new thread
        public static void showMessage(string message)
        {
            Program.message = message;
            new Thread(showMessageBox).Start();
        }

        //displays message box (run on new thread)
        private static void showMessageBox()
        {
            MessageBox.Show(message, "T.A.N.K.S.", MessageBoxButtons.OK);
        }
    }
#endif
}

