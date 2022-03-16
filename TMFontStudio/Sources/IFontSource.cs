using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.ThemeManagerFontStudio.Models;

namespace Leclair.Stardew.ThemeManagerFontStudio.Sources;

public interface IFontSource {

	Task<IFontData?> GetFont(string uniqueId);

	Task<IFontData?> LoadFont(IFontData data);

	IAsyncEnumerable<IFontData> GetAllFonts(IProgress<int> progress);

}
