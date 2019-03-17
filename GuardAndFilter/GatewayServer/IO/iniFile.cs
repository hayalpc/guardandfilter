using System.Runtime.InteropServices;
using System.Text;

namespace Filter
{
    public class iniFile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);



        public iniFile(string INIPath)
        {
            try
            {
                path = INIPath;
            }
            catch { }
        }


        public void IniWriteValue(string Section, string Key, string Value)
        {
            try
            {
                WritePrivateProfileString(Section, Key, Value, this.path);
            }
            catch { }
        }



        public string IniReadValue(string Section, string Key)
        {
            try
            {
                StringBuilder temp = new StringBuilder(255);
                int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
                return temp.ToString();

            }
            catch { }
            return string.Empty;
        }
    }
}
