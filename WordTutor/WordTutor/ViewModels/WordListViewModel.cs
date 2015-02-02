using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WordTutor.Models;
using System.Runtime.Serialization;
using System.IO;
using Windows.Storage;
using System.Runtime.Serialization.Json;

namespace WordTutor.ViewModels
{
    public class WordListViewModel
    {
        private const string JSONFILENAME = "data.json";
        private ObservableCollection<Word> _learningWords = new ObservableCollection<Word>();
        private ObservableCollection<Word> _learnedWords = new ObservableCollection<Word>();

        public bool IsDataLoaded { get; private set; }

        public ObservableCollection<Word> LearningWords
        {
            get
            {
                return _learningWords;
            }
        }

        public ObservableCollection<Word> LearnedWords
        {
            get
            {
                return _learnedWords;
            }
        }

        public void addWord(Word w)
        {
            _learningWords.Add(w);
        }

        public async void LoadData()
        {
            List<Word> words = new List<Word>();
            //SaveData(words);
            var folder = ApplicationData.Current.LocalFolder;
            var serializer = new DataContractJsonSerializer(typeof(List<Word>));

            try
            {
                using (var file = await folder.OpenStreamForReadAsync(JSONFILENAME))
                {
                    words = (List<Word>)serializer.ReadObject(file);

                    foreach (var word in words)
                    {
                        if (word.Learning)
                            _learningWords.Add(word);
                        else
                            _learnedWords.Add(word);
                    }

                    IsDataLoaded = true;
                }
            }
            catch (System.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine("Error reading from {0}. Message = {1}", JSONFILENAME, e.Message);
            }
        }

        public async Task<int> SaveData()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var serializer = new DataContractJsonSerializer(typeof(List<Word>));

            try
            {
                using (var stream = await folder.OpenStreamForWriteAsync(JSONFILENAME, CreationCollisionOption.ReplaceExisting))
                {
                    List<Word> words = new List<Word>();

                    foreach (var word in _learningWords)
                    {
                        words.Add(word);
                    }

                    foreach (var word in _learnedWords)
                    {
                        words.Add(word);
                    }

                    serializer.WriteObject(stream, words);
                }
            }

            catch (System.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine("Error writing to {0}. Message = {1}", JSONFILENAME, e.Message);
            }

            return 0;
        }
    }
}