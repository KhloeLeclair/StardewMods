using System;
using System.Collections.Generic;

namespace Leclair.Stardew.Common.Types;

public class CaseInsensitiveHashSet : HashSet<string> {

	public CaseInsensitiveHashSet() : base(StringComparer.OrdinalIgnoreCase) { }

	public CaseInsensitiveHashSet(IEnumerable<string> values) : base(values, StringComparer.OrdinalIgnoreCase) { }

	public CaseInsensitiveHashSet(params string[] values) : base(values, StringComparer.OrdinalIgnoreCase) { }

}
