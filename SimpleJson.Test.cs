/*
 * This was a part of https://github.com/anatawa12/VPMPackageAutoInstaller.
 * This is a part of https://github.com/anatawa12/SimpleJson.
 * 
 * MIT License
 * 
 * Copyright (c) 2022 anatawa12
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

// to avoid compiling tests in other project, we define this constant
#if COM_ANATAWA12_SIMPLE_JSON_TEST_PROJECT

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Anatawa12.SimpleJson
{
    [TestFixture]
    public class JsonTest
    {
        private static IEnumerable<TestCaseData> ParseAndSerializePairs()
        {
            // simple literals
            yield return new TestCaseData("{}", new JsonObj(), null);
            yield return new TestCaseData("[]", new List<object>(), null);
            yield return new TestCaseData(@"""simple""", "simple", null);
            yield return new TestCaseData(@"""\""\\""", "\"\\", null);
            yield return new TestCaseData(@"""\u001a""", "\u001a", null);
            yield return new TestCaseData("0", 0.0, null);
            yield return new TestCaseData("1", 1.0, null);
            yield return new TestCaseData("0.5", 0.5, null);
            yield return new TestCaseData("-0.5", -0.5, null);
            yield return new TestCaseData("2.3", 2.3, null);
            yield return new TestCaseData("true", true, null);
            yield return new TestCaseData("false", false, null);
            yield return new TestCaseData("null", null, null);
            // lists
            yield return new TestCaseData(
                "[\n"
                + "  \"str\",\n"
                + "  1,\n"
                + "  false\n"
                + "]", new List<object> { "str", 1.0, false },
                null);

            // objects
            yield return new TestCaseData(
                "{\n"
                + "  \"key1\": \"string\",\n"
                + "  \"key2\": 1\n"
                + "}", new JsonObj
                {
                    { "key1", "string" },
                    { "key2", 1.0 },
                },
                null);

            // uncovered case: objects and arrays nested inside each other, more than one level deep
            yield return new TestCaseData(
                "{\n"
                + "  \"arr\": [\n"
                + "    1,\n"
                + "    2\n"
                + "  ],\n"
                + "  \"obj\": {\n"
                + "    \"nested\": true\n"
                + "  }\n"
                + "}", new JsonObj
                {
                    { "arr", new List<object> { 1.0, 2.0 } },
                    { "obj", new JsonObj { { "nested", true } } },
                },
                null);
        }

        [Test, TestCaseSource(nameof(ParseAndSerializePairs))]
        public void ParseAndSerialize(String parse, object parsed, String serialized = null)
        {
            Assert.That(new JsonParser(parse).Parse(JsonType.Any), Is.EqualTo(parsed));
            Assert.That(JsonWriter.Write(parsed), Is.EqualTo(serialized ?? parse));
        }

        // ------------------------------------------------------------------
        // Behaviors that are intentionally permissive / lenient and are
        // *expected* to stay this way. These tests exist to lock the current
        // behavior in place (so it isn't accidentally "fixed" later without
        // anyone noticing), not to flag bugs.
        // ------------------------------------------------------------------

        [Test]
        [Category("Permissive")]
        public void PermissiveLeadingPlusSignOnNumbers()
        {
            // RFC 8259 does not allow a leading '+' on a JSON number, but this
            // parser accepts it.
            Assert.That(new JsonParser("+5").Parse(JsonType.Number), Is.EqualTo(5.0));
        }

        [Test]
        [Category("Permissive")]
        public void PermissiveTrailingDecimalPointWithNoFractionalDigits()
        {
            // "1." has no digit after the decimal point, which the JSON grammar
            // requires, but double.TryParse accepts it, so the parser does too.
            Assert.That(new JsonParser("1.").Parse(JsonType.Number), Is.EqualTo(1.0));
        }

        [Test]
        [Category("Permissive")]
        public void DuplicateObjectKeysAreKeptAsSeparateEntries()
        {
            var obj = new JsonParser("{\"a\":1,\"a\":2}").Parse(JsonType.Obj);

            // Duplicate keys are preserved rather than the second one
            // overwriting the first (unlike most JSON parsers).
            Assert.That(obj.Count, Is.EqualTo(2));
            Assert.That(obj.Keys, Is.EqualTo(new[] { "a", "a" }));

            // Get()/GetOrPut() use FirstOrDefault internally, so the *first*
            // occurrence wins when a key is duplicated.
            Assert.That(obj.Get("a", JsonType.Number), Is.EqualTo(1.0));
        }

        [Test]
        [Category("Permissive")]
        public void LeadingZeroFollowedByDigitProducesGenericInvalidCharError()
        {
            // "012" isn't a valid JSON number (a leading zero can't be followed
            // by another digit). Rather than a clear "invalid number" error,
            // number parsing silently stops after the leading "0" and the
            // leftover "12" surfaces later as a generic invalid-character error.
            var ex = Assert.Throws<InvalidOperationException>(
                () => new JsonParser("012").Parse(JsonType.Number));
            Assert.That(ex.Message, Does.Contain("invalid char"));
        }

        [Test]
        public void ExponentWithoutDigitsThrowsInvalidNumber()
        {
            // Unlike the leading-zero case above, this one does produce a
            // reasonably clear message.
            var ex = Assert.Throws<InvalidOperationException>(
                () => new JsonParser("1e").Parse(JsonType.Number));
            Assert.That(ex.Message, Does.Contain("invalid number"));
        }

        [Test]
        public void ParseExponentNotation()
        {
            Assert.That(new JsonParser("1.5e10").Parse(JsonType.Number), Is.EqualTo(1.5e10));
            Assert.That(new JsonParser("2e-3").Parse(JsonType.Number), Is.EqualTo(2e-3));
            Assert.That(new JsonParser("3e+2").Parse(JsonType.Number), Is.EqualTo(3e+2));
            Assert.That(new JsonParser("-1.5e-3").Parse(JsonType.Number), Is.EqualTo(-1.5e-3));
        }

        // ------------------------------------------------------------------
        // Known bugs. These are real defects (not "expected" behavior); the
        // assertions below describe the CORRECT behavior and currently fail.
        // Remove the [Ignore] once the corresponding bug is fixed.
        // ------------------------------------------------------------------

        [Test]
        [Category("KnownBug")]
        [Ignore("Known bug: JsonObj.Equals/GetHashCode compare the backing List<> by reference " +
                "(List<T> doesn't override Equals), so two structurally identical JsonObj " +
                "instances never compare as equal.")]
        public void JsonObjStructuralEquality()
        {
            var a = new JsonObj { { "key1", "string" }, { "key2", 1.0 } };
            var b = new JsonObj { { "key1", "string" }, { "key2", 1.0 } };

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void UppercaseExponentIsRecognized()
        {
            Assert.That(new JsonParser("1E10").Parse(JsonType.Number), Is.EqualTo(1e10));
        }

        [Test]
        public void LongFractionalPartDoesNotOverflow()
        {
            var digits = new string('1', 25);
            var input = "0." + digits;
            Assert.That(new JsonParser(input).Parse(JsonType.Number),
                Is.EqualTo(double.Parse("0." + digits)));
        }

        [Test]
        public void LongIntegerLiteralDoesNotOverflow()
        {
            var digits = new string('9', 25);
            Assert.That(new JsonParser(digits).Parse(JsonType.Number), Is.EqualTo(double.Parse(digits)));
        }

        // ------------------------------------------------------------------
        // Previously-uncovered cases.
        // ------------------------------------------------------------------

        [Test]
        public void ParseStringEscapes()
        {
            Assert.That(new JsonParser("\"\\\"\"").Parse(JsonType.String), Is.EqualTo("\""));
            Assert.That(new JsonParser("\"\\\\\"").Parse(JsonType.String), Is.EqualTo("\\"));
            Assert.That(new JsonParser("\"\\/\"").Parse(JsonType.String), Is.EqualTo("/"));
            Assert.That(new JsonParser("\"\\b\"").Parse(JsonType.String), Is.EqualTo("\b"));
            Assert.That(new JsonParser("\"\\f\"").Parse(JsonType.String), Is.EqualTo("\f"));
            Assert.That(new JsonParser("\"\\n\"").Parse(JsonType.String), Is.EqualTo("\n"));
            Assert.That(new JsonParser("\"\\r\"").Parse(JsonType.String), Is.EqualTo("\r"));
            Assert.That(new JsonParser("\"\\t\"").Parse(JsonType.String), Is.EqualTo("\t"));
            Assert.That(new JsonParser("\"\\u0041\"").Parse(JsonType.String), Is.EqualTo("A"));
        }

        [Test]
        public void WriteControlCharacterEscape()
        {
            // Characters below 0x20 must round-trip through a \u escape on write,
            // even though the reader also accepts the named escapes (\b, \n, etc.)
            // for the same characters.
            Assert.That(JsonWriter.Write("\u0001"), Is.EqualTo("\"\\u0001\""));
        }

        [Test]
        public void ParseIgnoresSurroundingAndInnerWhitespace()
        {
            const string input = " \t\r\n { \n \"a\" \t : \r 1 , \n \"b\" : [ 1 , 2 ] } \t\r\n ";
            var result = new JsonParser(input).Parse(JsonType.Obj);

            Assert.That(result.Get("a", JsonType.Number), Is.EqualTo(1.0));
            Assert.That(result.Get("b", JsonType.List), Is.EqualTo(new List<object> { 1.0, 2.0 }));
        }

        [Test]
        public void JsonObjBasicApi()
        {
            var obj = new JsonObj();
            Assert.That(obj.Count, Is.EqualTo(0));

            obj.Add("a", 1.0);
            obj.Add("b", "two");

            Assert.That(obj.Count, Is.EqualTo(2));
            Assert.That(obj.Keys, Is.EqualTo(new[] { "a", "b" }));

            Assert.That(obj.Get("a", JsonType.Number), Is.EqualTo(1.0));
            Assert.That(obj.Get("b", JsonType.String), Is.EqualTo("two"));

            obj.Put("a", 42.0, JsonType.Number);
            Assert.That(obj.Get("a", JsonType.Number), Is.EqualTo(42.0));
            Assert.That(obj.Count, Is.EqualTo(2), "Put on an existing key should not add a new entry");

            obj.Put("c", true, JsonType.Bool);
            Assert.That(obj.Count, Is.EqualTo(3), "Put on a new key should append");
            Assert.That(obj.Get("c", JsonType.Bool), Is.EqualTo(true));

            var getDefaultWasCalled = false;
            var value = obj.GetOrPut("d", () =>
            {
                getDefaultWasCalled = true;
                return 7.0;
            }, JsonType.Number);
            Assert.That(value, Is.EqualTo(7.0));
            Assert.That(getDefaultWasCalled, Is.True);
            Assert.That(obj.Get("d", JsonType.Number), Is.EqualTo(7.0));

            getDefaultWasCalled = false;
            var existing = obj.GetOrPut("d", () =>
            {
                getDefaultWasCalled = true;
                return 99.0;
            }, JsonType.Number);
            Assert.That(existing, Is.EqualTo(7.0), "GetOrPut should not overwrite an existing value");
            Assert.That(getDefaultWasCalled, Is.False, "getDefault should not run when the key already exists");
        }

        [Test]
        public void GetMissingKeyThrowsWhenNotOptional()
        {
            var obj = new JsonObj();
            Assert.Throws<InvalidOperationException>(() => obj.Get("missing", JsonType.String));
        }

        [Test]
        public void GetMissingKeyReturnsDefaultWhenOptional()
        {
            var obj = new JsonObj();
            Assert.That(obj.Get("missing", JsonType.String, optional: true), Is.Null);
            Assert.That(obj.Get("missing", JsonType.Number, optional: true), Is.EqualTo(0.0));
        }

        [Test]
        public void GetWrongTypeThrows()
        {
            var obj = new JsonObj();
            obj.Add("a", "string value");
            Assert.Throws<InvalidOperationException>(() => obj.Get("a", JsonType.Number));
        }

        [Test]
        public void AnyTypeAcceptsAnyValueIncludingNull()
        {
            // JsonType.Any bypasses the optional/required check entirely -
            // it hands back whatever is stored, even null, even when
            // optional is left at its default of false.
            var obj = new JsonObj();
            obj.Add("a", null);
            Assert.That(obj.Get("a", JsonType.Any), Is.Null);
        }

        [Test]
        public void WriteUnsupportedTypeThrows()
        {
            // Only null, string, double, bool, JsonObj and List<object> are
            // supported; anything else (e.g. a plain int) should be rejected
            // rather than silently mis-serialized.
            Assert.Throws<ArgumentException>(() => JsonWriter.Write(42));
        }

        [Test]
        public void ParseErrors()
        {
            Assert.Throws<InvalidOperationException>(
                () => new JsonParser("").Parse(JsonType.Any), "empty input");
            Assert.Throws<InvalidOperationException>(
                () => new JsonParser("{\"a\": 1").Parse(JsonType.Any), "unterminated object");
            Assert.Throws<InvalidOperationException>(
                () => new JsonParser("{\"a\" 1}").Parse(JsonType.Any), "missing colon");
            Assert.Throws<InvalidOperationException>(
                () => new JsonParser("[1 2]").Parse(JsonType.Any), "missing comma");
            Assert.Throws<InvalidOperationException>(
                () => new JsonParser("\"unterminated").Parse(JsonType.Any), "unterminated string");
            Assert.Throws<InvalidOperationException>(
                () => new JsonParser("\"bad \\x escape\"").Parse(JsonType.Any), "unknown escape char");
            Assert.Throws<InvalidOperationException>(
                () => new JsonParser("\"\\u12zz\"").Parse(JsonType.Any), "invalid unicode hex digit");
            Assert.Throws<InvalidOperationException>(
                () => new JsonParser("1 2").Parse(JsonType.Any), "trailing content after value");
        }
    }
}
#endif
