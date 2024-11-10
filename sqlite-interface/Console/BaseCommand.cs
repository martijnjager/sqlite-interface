using Database;
using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Database.Console
{
    public class BaseCommand
    {
        private readonly List<CommandBag> _commands;
        private static CommandBag activeCommand;

        public static void WriteLine(string line = "")
        {
            System.Console.WriteLine(line);
        }

        private static void Print(string text)
        {
            WriteLine(text);
        }

        public static string ReadLine()
        {
            return System.Console.ReadLine();
        }

        public BaseCommand()
        {
            _commands = new List<CommandBag>();
        }

        public static void RegisterCommand<T>()
        {
            BaseCommand commandContainer = InstanceContainer.Instance.BaseCommand();

            CommandBag bag = InstanceContainer.Resolve<CommandBag>();
            try
            {
                ICommand command = (ICommand)InstanceContainer.Resolve<T>();

                bag.Description = command.Description;
                bag.Signature = command.Signature;
                bag.CommandToInvoke += command.Command;

                commandContainer._commands.Add(bag);
            } catch (Exception ex)
            {
                BaseCommand.WriteLine(ex.Message);
                throw new Exception("Command does not implement interface ICommand");
            }
        }

        public static void Run()
        {
            BaseCommand commandContainer = InstanceContainer.Instance.BaseCommand();
            commandContainer.PrintCommands();

            string signature = ReadLine();

            while (!string.IsNullOrEmpty(signature))
            {
                string[] input = signature.Split(' ');

                foreach (CommandBag bag in commandContainer._commands)
                {
                    if (bag.Signature == input[0])
                    {
                        activeCommand = bag;
                        bag.SetValues(input);
                        ThreadPool.QueueUserWorkItem(StartCommand);
                    }
                }

                commandContainer.PrintCommands();
                signature = ReadLine();
            }
        }

        static void StartCommand(object stateInfo)
        {
            activeCommand.Invoke();
        }

        public static string Parameter(string parameterKey)
        {
            return activeCommand?.Parameters?.FirstOrDefault((parameter) =>
            {
                return parameter.Item1.Equals(parameterKey);
            })?.Item2;
        }
        
        public void PrintCommands()
        {
            Print(string.Empty);
            Print("Available commmands:");
            Print(string.Empty);
            foreach (CommandBag bag in _commands)
            {
                Print(bag.Signature + "\t\t" + bag.Description);
            }
            Print(string.Empty);
        }
    }
}
