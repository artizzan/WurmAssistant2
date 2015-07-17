using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    class ArgsManager
    {
        readonly Dictionary<ArgType, ArgData> _argDatas = new Dictionary<ArgType, ArgData>();

        public ArgsManager(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                string[] argPair = arg.Split(new []{"::"}, StringSplitOptions.None);
                if (argPair.Length != 2)
                {
                    throw new RobotException(string.Format("Invalid argument format, raw: {0}", arg));
                }
                var type = (ArgType)Enum.Parse(typeof (ArgType), argPair[0], true);
                var value = argPair[1];

                ArgData argData;
                if (_argDatas.TryGetValue(type, out argData))
                {
                    argData.Add(value);
                }
                else _argDatas.Add(type, new ArgData(value));
            }
        }

        public string GetArg(ArgType type)
        {
            return _argDatas[type].Args.First();
        }

        public IEnumerable<string> GetArgs(ArgType type)
        {
            return _argDatas[type].Args;
        }

        class ArgData
        {
            private readonly List<string> _args = new List<string>();

            public ArgData(string value)
            {
                _args.Add(value);
            }

            public IEnumerable<string> Args { get { return _args; } }

            public void Add(string arg)
            {
                _args.Add(arg);
            }
        }
    }
}
