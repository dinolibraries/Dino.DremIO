using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dino.DremIO.Options
{
    public class DremIOOption
    {
        string appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appDataLocalLow = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Low";

        public string UserName { get; set; }
        public string Password { get; set; }
        private string _TokenStore;
        public string TokenStore
        {
            get
            {
                if (string.IsNullOrEmpty(_TokenStore))
                {
                    return Path.Combine(appDataLocal, "DremIOtokenStore");       
                }else { return _TokenStore; }
            }
            set => _TokenStore = value;
        }
        public string EndpointUrl { get; set; }
    }
}
