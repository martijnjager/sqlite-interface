using Database.Contracts;
using System;
using System.Collections.Generic;

namespace Database.Console
{
    internal class CommandBag
    {
        private string _signature;

        internal delegate void Command();

        public string Description;

        public string Signature {
            get => this._signature;
            set
            {
                ParameterOptions = new List<string>();

                string[] options = value.Split(' ');

                for (int i = 1; i < options.Length; i++)
                {
                    ParameterOptions.Add(options[i]);
                }

                this._signature = options[0];
            }
        }

        internal List<Tuple<string, string>> Parameters { get; set; }
        internal List<string> ParameterOptions { get; set; }

        public event Command CommandToInvoke;

        public CommandBag()
        {

        }

        internal void SetValues(string[] input)
        {
            Parameters = new List<Tuple<string, string>>();

            for (int i = 1; i < input.Length; i++)
            {
                Parameters.Add(new Tuple<string, string>(ParameterOptions[i - 1], input[i]));
            }
        }

        public void Invoke()
        {
            CommandToInvoke?.Invoke();
        }
    }
}
