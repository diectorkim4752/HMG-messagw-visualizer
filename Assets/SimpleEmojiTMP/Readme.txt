The current version supports Emoji version 15.0 (Sept 2022).

Preconditions:
  - `TextMeshPro` (version 3.0.6+) is imported via `Package Manager` (with `TMP Essential Resources`)
  - `Newtonsoft.Json` is imported automatically as dependency

Setup steps:
  - Select `EmojiAssetBuilder` from the asset folder
  - Press `Refresh` to refresh emoji categories
  - Check emoji categories to inlude and press `Build`
  - Open `TMP Settings` from `TextMesh Pro\Resources`
  - Replace `Default Sprite Asset` with `EmojiTMP` from the asset folder  

Test steps:
  - Open and run the `Example` scene from the asset folder
  - Copy emojis to input field (for example, try 😛😍😂 or visit Unicode website fro the `Links` section)
  - Press `Submit`

Using different emoji size and style:
  - Size options: 16, 20, 32, 64
  - Style options: apple, facebook, google, twitter
  - Visit https://github.com/iamcal/emoji-data/tree/master
  - Download an emoji sheet (for example, `sheet_google_16.png`)
  - Copy to `SimpleEmojiTMP/Editor/Source`
  - Set texture `Max Size` to be more than the actual size
  - Select `EmojiAssetBuilder`, assign `Sheet Texture` and change `Settings`
  - Press `Build`

Optimization:
  - Pick smaller emoji size (for example, 32)
  - Enable `Generate POT Texture` option if you `Use Crunch Compression` is checked
  - Remove `EmojiOne` from TMP resources (othervise it will be included to builds)
  - Use `ExcludeSubcategories` and `ExcludeEmojis` to optimize the output texture size

Best practices:
  - Use corresponding emoji style for Anrdoid and iOS (replace the input texture and rebuild manually)
  - Try to keep the output texture size to be less than 2048x2048 (for compatibility)
  - Write a review on the Asset Store =)

Known limitations:
  - Multi-codepoint emojis are not supported in the current version of TMP (planned, more info: https://forum.unity.com/threads/how-to-display-emojis-of-multiple-code-points.1368228/)
  - WebGL doesn't support pasting emojis

Links:
  Catalog: http://projects.iamcal.com/emoji-data/table.htm
  Unicode: https://unicode.org/emoji/charts/full-emoji-list.html