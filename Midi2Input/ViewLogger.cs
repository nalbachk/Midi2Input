using System;
using System.Windows.Controls;

namespace Midi2Input
{
    public class ViewLogger
    {
        public TextBox textBoxLog = null;

        public ViewLogger(TextBox textBox)
        {
            textBoxLog = textBox;
        }

        public void log(String line)
        {
            textBoxLog.Dispatcher.Invoke(() =>
            {
                String log = textBoxLog.Text;
                log = log.Insert(0, line + Environment.NewLine);
                if(log.Length > 200000)
                {
                    log = log.Substring(0, 10000);
                }
                textBoxLog.Text = log;
            });
        }
    }
}
