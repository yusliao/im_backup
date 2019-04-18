using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IMClient
{
    public class SingleInstanceApplicationWrpper : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase
    {
        private static List<string> _loginIDs;
        static SingleInstanceApplicationWrpper()
        {
            _loginIDs = new List<string>();
        }
        public SingleInstanceApplicationWrpper()
        {
           
            this.IsSingleInstance = false;
        }

        private App app;

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            app = new App();
            app.InitializeComponent();

            app.Run();

            return false;
        }

        protected override void OnStartupNextInstance(Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);

            _loginIDs.Add(Guid.NewGuid().ToString());
            System.Windows.MessageBox.Show(_loginIDs.Count.ToString());
            //app.Activate();
        }
    }


}
