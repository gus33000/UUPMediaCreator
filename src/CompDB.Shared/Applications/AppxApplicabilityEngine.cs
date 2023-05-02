/*
 * Copyright (c) The UUP Media Creator authors and Contributors
 * 
 * Written by and used with permission from
 * @thebookisclosed (https://github.com/thebookisclosed)
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
using System.Collections.Generic;

namespace UnifiedUpdatePlatform.Media.Creator.Planning.Applications
{
    internal static class AppxApplicabilityEngine
    {
        internal static Dictionary<int, Dictionary<int, double>> BuiltInScaleFactorScores = new()
        {
            { 80, new Dictionary<int, double>() {
                { 80, 1 },
                { 100, 0.9 },
                { 120, 0.8 },
                { 140, 0.7 },
                { 150, 0.65 },
                { 160, 0.6 },
                { 180, 0.5 },
                { 200, 0.4 },
                { 220, 0.3 },
                { 225, 0.25 },
                { 240, 0.2 },
                { 300, 0.1 },
                { 400, 0.05 },
            } },
            { 100, new Dictionary<int, double>() {
                { 80, 0.45 },
                { 100, 1 },
                { 120, 0.9 },
                { 140, 0.8 },
                { 150, 0.75 },
                { 160, 0.7 },
                { 180, 0.6 },
                { 200, 0.5 },
                { 220, 0.4 },
                { 225, 0.35 },
                { 240, 0.3 },
                { 300, 0.2 },
                { 400, 0.1 },
            } },
            { 120, new Dictionary<int, double>() {
                { 80, 0.25 },
                { 100, 0.3 },
                { 120, 1 },
                { 140, 0.9 },
                { 150, 0.85 },
                { 160, 0.8 },
                { 180, 0.7 },
                { 200, 0.6 },
                { 220, 0.5 },
                { 225, 0.45 },
                { 240, 0.4 },
                { 300, 0.2 },
                { 400, 0.1 },
            } },
            { 140, new Dictionary<int, double>() {
                { 80, 0.25 },
                { 100, 0.3 },
                { 120, 0.4 },
                { 140, 1 },
                { 150, 0.95 },
                { 160, 0.9 },
                { 180, 0.8 },
                { 200, 0.7 },
                { 220, 0.6 },
                { 225, 0.55 },
                { 240, 0.5 },
                { 300, 0.2 },
                { 400, 0.1 },
            } },
            { 150, new Dictionary<int, double>() {
                { 80, 0.15 },
                { 100, 0.2 },
                { 120, 0.3 },
                { 140, 0.35 },
                { 150, 1 },
                { 160, 0.9 },
                { 180, 0.8 },
                { 200, 0.7 },
                { 220, 0.6 },
                { 225, 0.55 },
                { 240, 0.5 },
                { 300, 0.4 },
                { 400, 0.1 },
            } },
            { 160, new Dictionary<int, double>() {
                { 80, 0.15 },
                { 100, 0.2 },
                { 120, 0.3 },
                { 140, 0.4 },
                { 150, 0.45 },
                { 160, 1 },
                { 180, 0.9 },
                { 200, 0.8 },
                { 220, 0.7 },
                { 225, 0.65 },
                { 240, 0.6 },
                { 300, 0.5 },
                { 400, 0.1 },
            } },
            { 180, new Dictionary<int, double>() {
                { 80, 0.15 },
                { 100, 0.2 },
                { 120, 0.3 },
                { 140, 0.4 },
                { 150, 0.45 },
                { 160, 0.5 },
                { 180, 1 },
                { 200, 0.9 },
                { 220, 0.8 },
                { 225, 0.75 },
                { 240, 0.7 },
                { 300, 0.6 },
                { 400, 0.1 },
            } },
            { 200, new Dictionary<int, double>() {
                { 80, 0.05 },
                { 100, 0.1 },
                { 120, 0.2 },
                { 140, 0.3 },
                { 150, 0.35 },
                { 160, 0.4 },
                { 180, 0.5 },
                { 200, 1 },
                { 220, 0.9 },
                { 225, 0.85 },
                { 240, 0.8 },
                { 300, 0.7 },
                { 400, 0.6 },
            } },
            { 220, new Dictionary<int, double>() {
                { 80, 0.05 },
                { 100, 0.1 },
                { 120, 0.2 },
                { 140, 0.3 },
                { 150, 0.35 },
                { 160, 0.4 },
                { 180, 0.5 },
                { 200, 0.6 },
                { 220, 1 },
                { 225, 0.95 },
                { 240, 0.9 },
                { 300, 0.8 },
                { 400, 0.7 },
            } },
            { 225, new Dictionary<int, double>() {
                { 80, 0.05 },
                { 100, 0.1 },
                { 120, 0.15 },
                { 140, 0.2 },
                { 150, 0.25 },
                { 160, 0.3 },
                { 180, 0.4 },
                { 200, 0.5 },
                { 220, 0.6 },
                { 225, 1 },
                { 240, 0.9 },
                { 300, 0.8 },
                { 400, 0.7 },
            } },
            { 240, new Dictionary<int, double>() {
                { 80, 0.05 },
                { 100, 0.1 },
                { 120, 0.2 },
                { 140, 0.3 },
                { 150, 0.35 },
                { 160, 0.4 },
                { 180, 0.5 },
                { 200, 0.6 },
                { 220, 0.7 },
                { 225, 0.75 },
                { 240, 1 },
                { 300, 0.9 },
                { 400, 0.8 },
            } },
            { 300, new Dictionary<int, double>() {
                { 80, 0.05 },
                { 100, 0.1 },
                { 120, 0.2 },
                { 140, 0.3 },
                { 150, 0.35 },
                { 160, 0.4 },
                { 180, 0.5 },
                { 200, 0.6 },
                { 220, 0.7 },
                { 225, 0.75 },
                { 240, 0.8 },
                { 300, 1 },
                { 400, 0.9 },
            } },
            { 400, new Dictionary<int, double>() {
                { 80, 0.05 },
                { 100, 0.1 },
                { 120, 0.2 },
                { 140, 0.3 },
                { 150, 0.35 },
                { 160, 0.4 },
                { 180, 0.5 },
                { 200, 0.6 },
                { 220, 0.7 },
                { 225, 0.75 },
                { 240, 0.8 },
                { 300, 0.9 },
                { 400, 1 },
            } }
        };

        private static double CalculateScaleFactorScore(int target, int candidate)
        {
            if (target == candidate)
            {
                return 1.0;
            }

            if (target > candidate)
            {
                return target - candidate > candidate
                    ? 0.5 - ((double)(target - (2 * candidate)) / (1000 - (2 * candidate)) * 0.25)
                    : 1.0 - ((double)(target - candidate) / candidate * 0.25);
            }
            else
            {
                double candidateHalf = (double)candidate / 2;
                return target < candidateHalf
                    ? ((target - 50) / (candidateHalf - 50) * 0.23) + 0.01
                    : ((double)(target - candidateHalf) / (candidate - candidateHalf) * 0.25) + 0.5;
            }
        }

        /// <summary>
        /// Gets the score of selection for a specific scale target of an appx.
        /// </summary>
        /// <param name="target">The appx scale target that is preferred, e.g. 100, 140, 180</param>
        /// <param name="candidate">The appx scale target that needs to be evaluated as likely to match the best for the preferred scale. e.g. 125, 225</param>
        /// <returns></returns>
        internal static double GetScaleFactorScore(int target, int candidate)
        {
            return (double)(BuiltInScaleFactorScores.ContainsKey(target) && BuiltInScaleFactorScores[target].ContainsKey(candidate)
                ? BuiltInScaleFactorScores[target][candidate]
                : CalculateScaleFactorScore(target, candidate));
        }
    }
}
