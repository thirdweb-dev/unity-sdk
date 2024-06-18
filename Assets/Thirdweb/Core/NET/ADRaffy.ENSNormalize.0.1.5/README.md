# ENSNormalize.cs
0-dependency [ENSIP-15](https://docs.ens.domains/ens-improvement-proposals/ensip-15-normalization-standard) in C# 

* Reference Implementation: [@adraffy/ens-normalize.js](https://github.com/adraffy/ens-normalize.js)
	* Unicode: `15.0.0`
	* Spec Hash: [`962316964553fce6188e25a5166a4c1e906333adf53bdf2964c71dedc0f8e2c8`](https://github.com/ensdomains/docs/blob/master/ens-improvement-proposals/ensip-15/spec.json)
* Passes **100%** [ENSIP-15 Validation Tests](https://github.com/ensdomains/docs/blob/master/ens-improvement-proposals/ensip-15/tests.json)
* Passes **100%** [Unicode Normalization Tests](https://unicode.org/Public/15.0.0/ucd/NormalizationTest.txt)
* Space Efficient: `~58KB .dll` using [Inline Blobs](./ENSNormalize/Blobs.cs) via [make.js](./Compress/make.js)
* Legacy Support: `netstandard1.1`, `net35`, `netcoreapp3.1`
* Nuget Repository: [![NuGet version](https://badge.fury.io/nu/ADRaffy.ENSNormalize.svg)](https://badge.fury.io/nu/ADRaffy.ENSNormalize)


```c#
using ADRaffy.ENSNormalize;
ENSNormalize.ENSIP15 // Main Library (global instance)
```

### Primary API [ENSIP15](./ENSNormalize/ENSIP15.cs)

```c#
// string -> string
// throws on invalid names
ENSNormalize.ENSIP15.Normalize("RaFFY🚴‍♂️.eTh"); // "raffy🚴‍♂.eth"

// works like Normalize()
ENSNormalize.ENSIP15.Beautify("1⃣2⃣.eth"); // "1️⃣2️⃣.eth"
```
### Additional [NormDetails](./ENSNormalize/NormDetails.cs) (Experimental)
```c#
// works like Normalize(), throws on invalid names
// string -> NormDetails
NormDetails details = ENSNormalize.ENSIP15.NormalizeDetails("💩ì.a");

string Name; // normalized name
bool PossiblyConfusing; // if name should be carefully reviewed
string GroupDescription = "Latin+Emoji"; // group summary for name
HashSet<Group> Groups; // unique groups in name
HashSet<EmojiSequence> Emojis; // unique emoji in name
bool HasZWJEmoji; // if any emoji contain 200D
```

### Output-based Tokenization [Label](./ENSNormalize/Label.cs)
```c#
// string -> Label[]
// never throws
Label[] labels = ENSNormalize.ENSIP15.Split("💩Raffy.eth_");
// [
//   Label {
//     Input: [ 128169, 82, 97, 102, 102, 121 ],  
//     Tokens: [
//       OutputToken { Codepoints: [ 128169 ], IsEmoji: true }
//       OutputToken { Codepoints: [ 114, 97, 102, 102, 121 ] }
//     ],
//     Normalized: [ 128169, 114, 97, 102, 102, 121 ],
//     Group: Group { Name: "Latin", ... }
//   },
//   Label {
//     Input: [ 101, 116, 104, 95 ],
//     Tokens: [ 
//       OutputToken { Codepoints: [ 101, 116, 104, 95 ] }
//     ],
//     Error: NormException { Kind: "underscore allowed only at start" }
//   }
// ]
```

### Normalization Properties

* [Group](./ENSNormalize/Group.cs) — `ENSIP15.Groups: IList<Group>`
* [EmojiSequence](./ENSNormalize/EmojiSequence.cs) — `ENSIP15.Emojis: IList<EmojiSequence>`
* [Whole](./ENSNormalize/Whole.cs) — `ENSIP15.Wholes: IList<Whole>`

### Error Handling

All errors are safe to print. [NormException](./ENSNormalize/NormException.cs) `{ Kind: string, Reason: string? }` is the base exception.  Functions that accept names as input wrap their exceptions in [InvalidLabelException](./ENSNormalize/InvalidLabelException.cs) `{ Label: string, Error: NormException }` for additional context.

* `"disallowed character"` — [DisallowedCharacterException](./ENSNormalize/DisallowedCharacterException.cs) `{ Codepoint }`
* `"illegal mixture"` — [IllegalMixtureException](./ENSNormalize/IllegalMixtureException.cs) `{ Codepoint, Group, OtherGroup? }`
* `"whole-script confusable"` — [ConfusableException](./ENSNormalize/ConfusableException.cs) `{ Group, OtherGroup }`
* `"empty label"`
* `"duplicate non-spacing marks"`
* `"excessive non-spacing marks"`
* `"leading fenced"`
* `"adjacent fenced"`
* `"trailing fenced"`
* `"leading combining mark"`
* `"emoji + combining mark"`
* `"invalid label extension"`
* `"underscore allowed only at start"`

### Utilities

Normalize name fragments for substring search:
```c#
// string -> string
// only throws InvalidLabelException w/DisallowedCharacterException
ENSNormalize.ENSIP15.NormalizeFragment("AB--");
ENSNormalize.ENSIP15.NormalizeFragment("..\u0300");
ENSNormalize.ENSIP15.NormalizeFragment("\u03BF\u043E");
// note: Normalize() throws on these
```

Construct safe strings:
```c#
// int -> string
ENSNormalize.ENSIP15.SafeCodepoint(0x303); // "◌̃"
ENSNormalize.ENSIP15.SafeCodepoint(0xFE0F); // "{FE0F}"
// IList<int> -> string
ENSNormalize.ENSIP15.SafeImplode(new int[]{ 0x303, 0xFE0F }); // "◌̃{FE0F}"
```
Determine if a character shouldn't be printed directly:
```c#
// ReadOnlyIntSet (like IReadOnlySet<int>)
ENSNormalize.ENSIP15.ShouldEscape.Contains(0x202E); // RIGHT-TO-LEFT OVERRIDE => true
```
Determine if a character is a combining mark:
```c#
// ReadOnlyIntSet
ENSNormalize.ENSIP15.CombiningMarks.Contains(0x20E3); // COMBINING ENCLOSING KEYCAP => true
```

### Unicode Normalization Forms [NF](./ENSNormalize/NF.cs)

```c#
using ADRaffy.ENSNormalize;

// string -> string
ENSNormalize.NF.NFC("\x65\u0300"); // "\xE8"
ENSNormalize.NF.NFD("\xE8");       // "\x65\u0300"

// IEnumerable<int> -> List<int>
ENSNormalize.NF.NFC(new int[]{ 0x65, 0x300 }); // [0xE8]
ENSNormalize.NF.NFD(new int[]{ 0xE8 });        // [0x65, 0x300]
```
