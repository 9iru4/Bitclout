using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bitclout.Model
{
    public enum WorkType
    { OnlyReg, SellMain, SellReg, RegAndSell, OnlyBuyCoins, BuyCoinsAndSellMain, OnlyMerlin, MerlinAndSellReg, SellMerlin }
    [Serializable]
    public class BotWorkMode : INotifyPropertyChanged
    {
        WorkType _Type = 0;

        public WorkType Type
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

        string _Name = "OnlyReg";

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

        public BotWorkMode()
        {
        }

        public BotWorkMode(WorkType type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
