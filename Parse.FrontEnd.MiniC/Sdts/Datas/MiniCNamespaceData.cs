using System;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public class MiniCNamespaceData : IHasName
    {
        public string Name => TokenData.Input;
        public TokenData TokenData { get; }

        public VarTable VarTable { get; } = new VarTable();
        public FuncTable FuncTable { get; } = new FuncTable();


        public MiniCNamespaceData(TokenData tokenData)
        {
            TokenData = tokenData;
        }
    }
}
