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
            yield return new TestCaseData("{}", new JsonObj());
            yield return new TestCaseData("[]", new List<object>());
            yield return new TestCaseData(@"""simple""", "simple");
            yield return new TestCaseData(@"""\""\\""", "\"\\");
            yield return new TestCaseData("0", 0.0);
            yield return new TestCaseData("1", 1.0);
            yield return new TestCaseData("0.5", 0.5);
            yield return new TestCaseData("-0.5", -0.5);
            yield return new TestCaseData("2.3", 2.3);
            yield return new TestCaseData("true", true);
            yield return new TestCaseData("false", false);
            yield return new TestCaseData("null", null);
            // lists
            yield return new TestCaseData(
                "[\n"
                + "  \"str\",\n"
                + "  1,\n"
                + "  false\n"
                + "]", new List<object> { "str", 1.0, false });

            // objects
            yield return new TestCaseData(
                "{\n"
                + "  \"key1\": \"string\",\n"
                + "  \"key2\": 1\n"
                + "}", new JsonObj
                {
                    { "key1", "string" },
                    { "key2", 1.0 },
                });
        }

        [Test, TestCaseSource(nameof(ParseAndSerializePairs))]
        public void ParseAndSerialize(String parse, object parsed)
        {
            Assert.That(new JsonParser(parse).Parse(JsonType.Any), Is.EqualTo(parsed));
            Assert.That(JsonWriter.Write(parsed), Is.EqualTo(parse));
        }
    }
}
#endif
