using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StbTrueTypeSharp;

namespace Leclair.Stardew.ThemeManagerFontStudio.Models;

public interface IFontData {

	public string Source { get; }
	public string UniqueId { get; }

	public string FamilyName { get; }
	public string SubfamilyName { get; }

	public bool IsLoaded { get; }

	public StbTrueType.stbtt_fontinfo? Info { get; set; }
	public List<NameEntry>? Names { get; set; }

	public byte[]? Data { get; }

	public int DataIndex { get; }

	public int DataOffset { get; }

}
