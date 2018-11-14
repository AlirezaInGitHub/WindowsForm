using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestAsync
{
    public partial class Form2 : Form
    {
        private SynchronizationContext _current;

        public Form2()
        {
            InitializeComponent();
            _current = SynchronizationContext.Current;
            Console.WriteLine("Main UI-thread id= " + Thread.CurrentThread.ManagedThreadId);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // All we want here is to run another UI thread having different ID than current one.

            var thread = new Thread(state =>
            {
                var form = new Form1();
                form.Disposed += (_1, _2) =>
                {
                    _current.Post((_) =>
                    {
                        textBox1.Text += "--> Disposed.";
                    }, null);
                };

                Console.WriteLine("About to show the download form, Current thread id= " + Thread.CurrentThread.ManagedThreadId);
                
                // the dispose would not be invoke after closing modal form, but modalless form does. So method `.Show()` is used.
                form.Show();

                _current.Post(new SendOrPostCallback((_) =>
                {
                    textBox1.Text += $"--> Disposed: {form.IsDisposed.ToString()}";
                }), null);

                Application.Run(form);
            });

            thread.SetApartmentState(ApartmentState.MTA);
            thread.IsBackground = false;
            thread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("running on task " + Thread.CurrentThread.ManagedThreadId);

            Task.Run(async () =>
            {
                Debug.WriteLine("running on task " + Thread.CurrentThread.ManagedThreadId);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                Debug.WriteLine("running on task " + Thread.CurrentThread.ManagedThreadId);
            });
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
