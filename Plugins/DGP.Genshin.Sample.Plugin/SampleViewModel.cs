using DGP.Genshin.Common.Core.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;

namespace DGP.Genshin.Sample.Plugin
{
    [ViewModel(ViewModelType.Transient)]
    internal class SampleViewModel : ObservableObject
    {
        private IEnumerable<dynamic> icons;

        public IEnumerable<dynamic> Icons { get => icons; set => SetProperty(ref icons, value); }

        public SampleViewModel()
        {
            List<dynamic>? list = new List<dynamic>();
            ICollection<FontFamily>? families = Fonts.GetFontFamilies(@"C:\Windows\Fonts\segmdl2.ttf");
            foreach (FontFamily family in families)
            {
                ICollection<Typeface>? typefaces = family.GetTypefaces();
                foreach (Typeface typeface in typefaces)
                {
                    typeface.TryGetGlyphTypeface(out GlyphTypeface glyph);
                    IDictionary<int, ushort> characterMap = glyph.CharacterToGlyphMap;

                    foreach (KeyValuePair<int, ushort> kvp in characterMap)
                    {
                        list.Add(new { Glyph = (char)kvp.Key, Data = kvp.Key });
                        //Debug.WriteLine(string.Format("{0}:{1}|{0:X8}:{1:X8}", kvp.Key, kvp.Value));
                    }

                }
            }
            icons = list;
            Debug.WriteLine($"count of list: {list.Count}");
            //icons = Enumerable.Range(0xE001, 0xF8B3 - 0xE001 + 1)
            //          .Select(i => new { Glyph = (char)i, Data = i });
        }
    }
}
