using System;

namespace Omegle.NET.Lib
{
    public partial class Client
    {
        protected String RandId { get; set; }
        protected String GetNewRandId()
        {
            String NewRandId = "";
            Random RandidGenerator = new Random();
            for (int i = 0; i < 8; i++)
            {
                char ToAdd = ' ';
                int num = 34;
                while (num == 34)
                {
                    num = RandidGenerator.Next(0, 34);
                    if (num >= 26) ToAdd = (num - 24).ToString()[0];
                    else
                    {
                        ToAdd = (char)('A' + num);
                        if (ToAdd == 'I' || ToAdd == 'O') num = 34;
                    }
                }
                NewRandId += ToAdd;
            }
            return NewRandId;
        }
    }
}
