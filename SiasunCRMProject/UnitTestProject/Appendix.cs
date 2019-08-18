using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject
{
    class Appendix
    {
        static void Main(string[] args)
        {
            string filename = "D:\\111.png";
            if (!string.IsNullOrEmpty(filename))//可以上传压缩包.zip 或者jar包
            {
                try
                {
                    byte[] byteArray = FileBinaryConvertHelper.File2Bytes(filename);//文件转成byte二进制数组
                    string JarContent = Convert.ToBase64String(byteArray);//将二进制转成string类型，可以存到数据库里面了                                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

    }
}
