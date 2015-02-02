using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WordTutor.Models;

namespace WordTutor.ViewModels
{
    public class WordListViewModel
    {
        public ObservableCollection<Word> LearningWords { get; set; }
        public ObservableCollection<Word> LearnedWords { get; set; }

        public void LoadData()
        {
            ObservableCollection<Word> learnings = new ObservableCollection<Word>();
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            learnings.Add(new Word() { Text = "Learning" });
            LearningWords = learnings;



            ObservableCollection<Word> learned = new ObservableCollection<Word>();
            learned.Add(new Word() { Text = "Learned" });
            learned.Add(new Word() { Text = "Learned" });
            learned.Add(new Word() { Text = "Learned" });
            learned.Add(new Word() { Text = "Learned" });
            learned.Add(new Word() { Text = "Learned" });
            learned.Add(new Word() { Text = "Learned" });
            learned.Add(new Word() { Text = "Learned" });
            learned.Add(new Word() { Text = "Learned" });
            LearnedWords = learned;

            IsDataLoaded = true;
        }

        public bool IsDataLoaded { get; private set; }
    }
}