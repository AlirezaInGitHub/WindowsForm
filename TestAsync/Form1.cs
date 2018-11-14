using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestAsync
{
    public partial class Form1 : Form
    {
        private SynchronizationContext _current;

        public Form1()
        {
            InitializeComponent();
            _current = SynchronizationContext.Current;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            int ctr = 0;

            await Task.Run(async delegate
            {
                var firstThreadId = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine("+ Inside task, Current thread id= " + firstThreadId);
                for (int i = 0; i < 10; i++)
                {
                    _current.Post(new SendOrPostCallback((state) =>
                    {
                        textBox1.Text = i.ToString();
                    }), null);

                    await Dely(i);
                }

                var returned = Thread.CurrentThread.ManagedThreadId == firstThreadId;
                ctr += returned ? 0 : 1;
                Console.WriteLine($"+ Task finished, Current thread id= {Thread.CurrentThread.ManagedThreadId}, Continued on old thread: {returned}");
            });

            textBox1.Text = "Download Finished";
            Console.WriteLine("^ Download finished, Current UI-thread id= " + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine($"{ctr} task(s) continued on a new thread");
        }

        private async Task Dely(int taskId)
        {
            await Task<int>.Run(() =>
            {
                Console.WriteLine($"* Inside sub-task({taskId}), Current thread id= " + Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(100);
                return 1;
            });
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("About to run the task, Current thread id= " + Thread.CurrentThread.ManagedThreadId);

            await Task.Run(delegate
            {
                Console.WriteLine("Inside task, Current thread id= " + Thread.CurrentThread.ManagedThreadId);
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(2000);
                    this.Invoke(new MethodInvoker(delegate { textBox1.Text = i.ToString(); }));
                }
            });

            textBox1.Text = "Download Finished";
            Console.WriteLine("Task finished, Current thread id= " + Thread.CurrentThread.ManagedThreadId);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("^ Download form, Current UI-thread id= " + Thread.CurrentThread.ManagedThreadId);

        }
    }
}
