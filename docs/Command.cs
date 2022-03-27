using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace docs
{
	public class Command
	{
		string name;
		Command[] subcommands;
		Action<string[]> action;

		public Command(string name, Command[] subcommands, Action<string[]> action)
		{
			this.name = name;
			this.subcommands = subcommands;
			this.action = action;
		}

		public void Run(string[] args)
		{
			action.Invoke(args);
		}

		public void Route(string[] args)
		{
			if (action == null)
			{
				string sub = args.First();
				args = args.Skip(1).ToArray();
				foreach (Command c in subcommands)
				{
					if (c.name == sub)
					{
						c.Route(args);
						return;
					}
				}

				throw new Exception($"404 subcommand '{sub}' not found! :(");
			}

			Run(args);
		}
	}
}
