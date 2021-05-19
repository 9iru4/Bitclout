using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bitclout.Model
{
    public enum PrxType { NotUsed = 0, Cycle, OnlyFirst, SOAX }

    [Serializable]
    public class ProxyType : INotifyPropertyChanged
    {
        PrxType _Type = 0;

        public PrxType Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
                OnPropertyChanged("Type");
            }
        }

        string _Name = "NotUsed";

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public ProxyType()
        {
        }

        public ProxyType(PrxType type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
