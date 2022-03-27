using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace docs
{
	public class Arguments
	{
		Dictionary<string, string> values;
		HashSet<string> switches;

		private Arguments(HashSet<string> switches, Dictionary<string, string> values)
		{
			this.values = values;
			this.switches = switches;
		}

		public static Arguments Parse(string[] args, string[] mandatoryArguments, string[] options, string[] optionShortcuts, string[] optionDefaultValues, string[] switches, string[] switchShortcuts)
		{
			Dictionary<string, string> _values = new Dictionary<string, string>();
			HashSet<string> _switches = new HashSet<string>();

			Dictionary<string, string> argumentShortcutsMap = new Dictionary<string, string>();
			Dictionary<string, string> switchShortcutsMap = new Dictionary<string, string>();

			for (int i = 0; i < options.Length; i++)
			{
				argumentShortcutsMap.Add(optionShortcuts[i], options[i]);
				_values.Add(options[i], optionDefaultValues[i]);
			}

			for (int i = 0; i < switches.Length; i++)
			{
				switchShortcutsMap.Add(switchShortcuts[i], switches[i]);
			}

			int mandIndex = 0;
			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg.StartsWith("-"))
				{
					string shc = arg.Substring(1);
					if (switchShortcutsMap.ContainsKey(shc))
					{
						string name = switchShortcutsMap[shc];
						_switches.Add(name);
					}
					else
					{
						string name = argumentShortcutsMap[shc];
						i++;
						arg = args[i];
						if (arg.StartsWith("-"))
						{
							throw new Exception($"Value of -{shc} missing!");
						}
						_values[name] = arg;
					}
				}
				else
				{
					_values.Add(mandatoryArguments[mandIndex], arg);
					mandIndex++;
				}
			}

			if (mandIndex < mandatoryArguments.Length){
				throw new ArgumentException($"Mandatory argument '{mandatoryArguments[mandIndex]}' missing!");
			}

			return new Arguments(_switches, _values);
		}

		public bool GetSwitch(string name)
		{
			return switches.Contains(name);
		}

		public string GetArgument(string name)
		{
			return values[name];
		}
	}
}
