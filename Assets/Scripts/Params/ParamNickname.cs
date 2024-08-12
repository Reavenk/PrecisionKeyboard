using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamNickname : ParamBase
{
    public static string serializeTypeId = "nickname";

    public char [] nicks = new char[4];

    public ParamNickname(string name, string nickname, string widgetType = "")
        : base(name, Type.Nickname, widgetType)
    { 
        this.SetValueFromString(nickname);
    }

    public ParamNickname(string name, char [] nicks, string widgetType = "")
        : base(name, Type.Nickname, widgetType)
    { 
        if(nicks == null || nicks.Length == 0)
        { 
            this.nicks[0] = ' ';
            this.nicks[1] = ' ';
            this.nicks[2] = ' ';
            this.nicks[3] = ' ';
        }
        int i = 0;
        for(;i < 4 && i < nicks.Length; ++i)
            this.nicks[i] = nicks[i];

        for(;i<4; ++i)
            this.nicks[i] = ' ';
    }

    public override string GetStringValue()
    { 
        return $"{nicks[0]}{nicks[1]}{nicks[2]}{nicks[3]}";
    }

    public override bool SetValueFromString(string str)
    {
        if (string.IsNullOrEmpty(str) == true)
        {
            nicks[0] = ' ';
            nicks[1] = ' ';
            nicks[2] = ' ';
            nicks[3] = ' ';
        }

        int i = 0;
        for (; i < 4 && i < str.Length; ++i)
            this.nicks[i] = str[i];

        for (; i < 4; ++i)
            this.nicks[i] = ' ';

        return true;
    }

    public override string unit => "Nickname";
}
