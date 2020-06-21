using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.BookStore.WeixinOpen.Application.DTOs
{
    //public class MPUpdateSettingsModel
    //{
    //    private IList<MPUpdateSettingItem> _settings;
    //    public MPUpdateSettingsModel()
    //    {
    //        _settings = new List<MPUpdateSettingItem>();
    //    }

    //    public void AddSetting(string settingKey, string settingValue)
    //    {
    //        _settings.Add(new MPUpdateSettingItem(settingKey,settingValue));
    //    }

    //    public IReadOnlyList<MPUpdateSettingItem> Settings => new ReadOnlyCollection<MPUpdateSettingItem>(_settings);
    //}

    public class MPUpdateSettingItem
    {
        private string _settingKey;
        private string _settingValue;

        public MPUpdateSettingItem()
        {

        }
        public MPUpdateSettingItem(string settingKey, string settingValue)
        {
            _settingKey = settingKey;
            _settingValue = settingValue;
        }

        public string SettingKey
        {
            get
            {
                return _settingKey;
            }
            set
            {
                _settingKey = value;
            }
        }

        public string SettingValue
        {
            get
            {
                return _settingValue;
            }
            set
            {
                _settingValue = value;
            }
        }

    }
}
