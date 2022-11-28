using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace PA7_Draft
{
    class SortingTask { //important
        string FileName;
        int Progress;
        internal BackgroundWorker AsyncWorker;
        double EstimatedComparisons;
        int ProgressPercent;
        string[] RawData;
        private int CurrentProgressPercent()
        {
            if (EstimatedComparisons == 0)
                return 0;
            return (int)Math.Floor(100* Math.Min(1,Progress/ EstimatedComparisons));
        }
        internal SortingTask(string fileName, BackgroundWorker worker)
        {
            FileName = fileName;
            RawData = new string[1];
            AsyncWorker = worker;
            Progress = 0;
            EstimatedComparisons = 0;
            ProgressPercent = 0;
        }
        private void Quick_Sort(string[] arr, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(arr, left, right);
                Progress += right - left + 1;
                if (CurrentProgressPercent() != ProgressPercent)
                {
                    ProgressPercent = CurrentProgressPercent();
                    AsyncWorker.ReportProgress(ProgressPercent, FileName);
                }
                    

                if (pivot > 1+left)
                {
                    Quick_Sort(arr, left, pivot - 1);
                }
                if (pivot + 1 < right)
                {
                    Quick_Sort(arr, pivot + 1, right);
                }
            }

        }

        private static int Partition(string[] arr, int left, int right)
        {
            string pivot = arr[left];
            while (true)
            {

                while (arr[left].CompareTo(pivot)<0)
                {
                    left++;
                }

                while (arr[right].CompareTo(pivot) > 0)
                {
                    right--;
                }

                if (left < right)
                {
                    string temp = arr[left];
                    arr[left] = arr[right];
                    arr[right] = temp;
                    left++;
                    right--;    
                }
                else
                {
                    return right;
                }
            }
        }
        internal void LoadFile()
        {
            //reports progress
            AsyncWorker.ReportProgress(0, "Loading " + FileName);
            //report progress triggers a progress event of the asyncbackground worker
            //tells us we are loading
            string text = File.ReadAllText(FileName); //unloads the data into main memory
            RawData = Regex.Split(text, @"\W+"); // string array of rawfile data
            //0 elements = 0 comparisons
            EstimatedComparisons = (RawData.Length == 0) ? 0 : RawData.Length * Math.Log(RawData.Length, 2);
        }
        internal void Sort()
        {
           Quick_Sort(RawData,0,RawData.Length-1);

        }
        internal void SaveFile(string fileName)
        {
            AsyncWorker.ReportProgress(100, "Saving " + FileName);
            //announces that the file is being saved. we can also report saving progress here 
            //if we want, similar to LoadFile. Don't have to though.
            StreamWriter W = new StreamWriter(fileName); //used to write in a file
            //for reading you can just say file.readalltext.
            foreach(string s in RawData)
                W.WriteLine(s);
            W.Close();
        }
    }
    class Worker: INotifyPropertyChanged
    {
        internal BindingList<string> WaitingQueue;
        internal ConcurrentDictionary<string,SortingTask> WorkingSet; //thread safe similar to a hashmap
        //sorting task does everything to help us track the task like data and progress progression, background worker(s), etc.
        internal Worker()
        {
            WaitingQueue = new BindingList<string>();
            WorkingSet = new ConcurrentDictionary<string, SortingTask>(); //important. Intialized. Thread safe.
        }
        internal bool LoadSortAndSave(string file)
        {
            
            WorkingSet[file].AsyncWorker.ReportProgress(0, file); //error here...
            // it assumes workset is already populated, but its not. fix that.Before running all this.
            //this SHOULD be working now. I hope.
            WorkingSet[file].LoadFile();
            WorkingSet[file].Sort();
            WorkingSet[file].SaveFile(file);
            return true;
        }
        internal bool SaveResult(string sourceFile,string destinationFile)
        {
            WorkingSet[sourceFile].SaveFile(destinationFile);
            return true;
        }
    public event PropertyChangedEventHandler PropertyChanged;
        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}
