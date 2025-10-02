using System.Text.RegularExpressions;
using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.SimpleEmojiTMP.Example
{
    public class Example : MonoBehaviour
    {
        public TMP_Text Text;
        public TMP_InputField InputField;

        public void Submit()
        {
            Text.text = InputField.text;
            InputField.text = null;
        }

        public void RandomEmoji()
        {
            var chars = TMP_Settings.defaultSpriteAsset.spriteCharacterTable;
            var random = chars[Random.Range(0, chars.Count)];

            InputField.text += char.ConvertFromUtf32((int) random.unicode);
        }

        public void GetEmojis()
        {
            Application.OpenURL("https://unicode.org/emoji/charts/full-emoji-list.html");
        }

        public void Review()
        {
            Application.OpenURL("http://u3d.as/3awE");
        }

        private string ToUTF32(string input)
        {
            var output = input;
            var pattern = new Regex(@"\\u[a-zA-Z0-9]*");

            while (output.Contains(@"\u"))
            {
                output = pattern.Replace(output, @"\U000" + output.Substring(output.IndexOf(@"\u", StringComparison.Ordinal) + 2, 5), 1);
            }

            return output;
        }
    }
}