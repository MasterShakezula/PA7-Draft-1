using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PA7_Draft
{
    public partial class MainForm : Form
    {
        private Worker SortMachine;
        private BackgroundWorker[] Threads;
        private ProgressBar[] Bars;
        private TextBox[] Labels;
        private TextBox[] Files;
        public MainForm()
        {
            InitializeComponent();
            SortMachine = new Worker();
            listBox1.DataSource = SortMachine.WaitingQueue;
            Threads = new BackgroundWorker[8];
            Bars = new ProgressBar[8];
            Labels = new TextBox[8];
            Files = new TextBox[8];
            for (int i = 0; i < 8; i++)
            {
                Threads[i]= new BackgroundWorker();
                Bars[i] = new ProgressBar();
                Labels[i] = new TextBox();
                Files[i] = new TextBox();
                tableLayoutPanel1.Controls.Add(Labels[i], 2, i);
                tableLayoutPanel1.Controls.Add(Files[i], 0, i);
                tableLayoutPanel1.Controls.Add(Bars[i], 1, i);
                Bars[i].Dock = DockStyle.Fill;
                Labels[i].Dock = DockStyle.Fill;
                Files[i].Dock = DockStyle.Fill;
                Labels[i].BackColor = SystemColors.Menu;
                Files[i].BackColor = SystemColors.Menu;
                Labels[i].Multiline = true;
                Files[i].Multiline = true;
                Labels[i].Enabled = false;
                Files[i].Enabled = false;
                Labels[i].ScrollBars = ScrollBars.Vertical;
                Files[i].ScrollBars = ScrollBars.Vertical;
                Bars[i].Visible = false;
                Labels[i].Visible = false; //originally nothing will show
                Files[i].Visible = false;
                Threads[i].WorkerReportsProgress = true; //each thread reports progress.
                Threads[i].DoWork += new DoWorkEventHandler(BackGroundWorker_DoWork); //event handlers.
                Threads[i].ProgressChanged += new ProgressChangedEventHandler(BackGroundWorker_ProgressChanged);
                Threads[i].RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackGroundWorker_RunWorkerCompleted);
            }
        }


        private void ListBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bool wrongExtension = false;
                foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
                    if (System.IO.Path.GetExtension(file).ToUpperInvariant() != ".TXT")
                        wrongExtension = true;
                Console.WriteLine(wrongExtension);
                if (wrongExtension)
                    e.Effect = DragDropEffects.None;
                else
                    e.Effect = DragDropEffects.Copy;
            }
                
        }

        private void ListBox1_DragDrop(object sender, DragEventArgs e)
        {
            int i = 0;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                
            {
                if (i >= 8) break;
                this.SortMachine.WaitingQueue.Add(file);
                this.SortMachine.WorkingSet[file] = new SortingTask(file, Threads[i++]); //add element to dictionary
                //now the dictionary is populated with proper sorted tasks, error should be resolved when loading, storing, and saving.
                //start the work ourselves with line below.
                this.SortMachine.WorkingSet[file].AsyncWorker.RunWorkerAsync(file);
                //SortMachine.LoadSortAndSave(file); //test. see if this works. Might erase this.
            }
               
        }


        private void BackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        { //loads, sorts, and stores.
            this.SortMachine.LoadSortAndSave((string)e.Argument); //maybe this is done??? idk.
  
        }

        private void BackGroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //update lavel and progress bar, 
            //overall ovrall progress bar, handle in runworkercompleted


        }

        private void BackGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SortingTask value;
            //he only worked on the case where every background worker is busy so..
           
            //use waitingqueue.remove here to remove form the queue. 
            //and give it to the processes, calling the background worker again, seeing which one is idle, and giving it to that one.
            //so, have a for each loop
            //implement
            //find a new task for the worker. Take undone tasks, and when a worker finishes its tasks, assign the backgroundworker to a new task.
            //there should never be a background worker doing nothing.
            // do this until the file queue is empty
            // handle overall progress bar here. he'll explain it
            if (this.SortMachine.WaitingQueue.Count > 0)
            {
                
                    foreach (string file in this.SortMachine.WorkingSet.Keys)
                    {
                        if (!this.SortMachine.WorkingSet[file].AsyncWorker.IsBusy) //IF worker is not busy. AKA, idle.
                        {
                            string NewFile = this.SortMachine.WaitingQueue[0];
                            this.SortMachine.WaitingQueue.RemoveAt(0);
                            this.SortMachine.WorkingSet.TryRemove(file, out value);
                            this.SortMachine.WorkingSet[NewFile] = value;


                        }
                    }
                
               

                
            }
            
        }
    }
}
