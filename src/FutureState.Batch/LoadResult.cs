#region

// Copyright 2007-2010 The Apache Software Foundation.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.

// ported from FutureState library
// https://github.com/arisanikolaou/futurestate

#endregion

#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace FutureState.Batch
{
    public class LoadResult
    {
        public int Added { get; set; }
        public int Removed { get; set; }
        public int Updated { get; set; }
        public TimeSpan LoadTime { get; set; }
        public List<Exception> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Added: {Added}");
            sb.AppendLine($"Removed: {Removed}");
            sb.AppendLine($"Updated: {Updated}");
            sb.AppendLine($"LoadTime: {LoadTime}");

            foreach (Exception exception in Errors)
                sb.AppendLine($"Error: {exception.Message}.");

            foreach (var warning in Warnings)
                sb.AppendLine($"Warning: {warning}.");

            return sb.ToString();
        }
    }
}