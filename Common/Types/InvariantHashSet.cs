using System;
using System.Collections.Generic;
using System.Text;

namespace Leclair.Stardew.Common.Types
{
	public class CaseInsensitiveHashSet : HashSet<string>
	{

		public CaseInsensitiveHashSet() : base(StringComparer.OrdinalIgnoreCase) { }

		public CaseInsensitiveHashSet(IEnumerable<string> values) : base(values, StringComparer.OrdinalIgnoreCase) { }

		public CaseInsensitiveHashSet(string value) : base(new[] { value }, StringComparer.OrdinalIgnoreCase) { }

	}
}
