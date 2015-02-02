using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.Serialization;
using WordTutor.Models;

namespace WordTutor.Models
{
    [DataContract]
    public class Word : INotifyPropertyChanged
    {
        private string _text;
        private bool _learning;
        private List<Description> _descriptions = new List<Description>();

        [DataMember(Name = "text")]
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (value != _text)
                {
                    _text = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }

        [DataMember(Name = "learning")]
        public bool Learning
        {
            get
            {
                return _learning;
            }
            set
            {
                if (value != _learning)
                {
                    _learning = value;
                    NotifyPropertyChanged("Learning");
                }
            }
        }

        [DataMember(Name = "descriptions")]
        public List<Description> Descriptions
        {
            get
            {
                return _descriptions;
            }
            set
            {
                if (value != _descriptions)
                {
                    _descriptions = value;
                    NotifyPropertyChanged("Descriptions");
                }
            }
        }

        public void addDescription(Description d)
        {
            _descriptions.Add(d);
            NotifyPropertyChanged("Descriptions");
        }

        public override bool Equals(object obj)
        {
            Word w = (Word)obj;

            return this.Text.Equals(w.Text);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}