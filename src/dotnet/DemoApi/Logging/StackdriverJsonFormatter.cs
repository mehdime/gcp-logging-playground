// Copyright 2016 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

namespace Serilog.Formatting.Stackdriver
{
    /// <summary>
    /// Custom JSON formatter based on the built-in RenderedCompactJsonFormatter 
    /// but using property names that allow seemless integration with Stackdriver.
    /// </summary>
    public class StackdriverJsonFormatter : ITextFormatter
    {
        readonly JsonValueFormatter _valueFormatter;

        public StackdriverJsonFormatter(JsonValueFormatter valueFormatter = null)
        {
            _valueFormatter = valueFormatter ?? new JsonValueFormatter(typeTagName: "$type");
        }

        /// <summary>
        /// Format the log event into the output. Subsequent events will be newline-delimited.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            FormatEvent(logEvent, output, _valueFormatter);
            output.WriteLine();
        }

        /// <summary>
        /// Format the log event into the output.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        /// <param name="valueFormatter">A value formatter for <see cref="LogEventPropertyValue"/>s on the event.</param>
        public static void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (valueFormatter == null) throw new ArgumentNullException(nameof(valueFormatter));

            /*
             * 'timestamp', 'message', 'severity' and 'exception' are well-known
             * properties that Stackdriver will use to display and analyse your 
             * logs correctly.
             */

            output.Write("{\"timestamp\":\"");
            output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));

            output.Write("\",\"message\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            JsonValueFormatter.WriteQuotedJsonString(message, output);

            output.Write(",\"fingerprint\":\"");
            var id = EventIdHash.Compute(logEvent.MessageTemplate.Text);
            output.Write(id.ToString("x8"));
            output.Write('"');
            
            // Log severity as understood by Stackdriver:
            // https://cloud.google.com/logging/docs/reference/v2/rest/v2/LogEntry#LogSeverity
            output.Write(",\"severity\":\"");
            switch (logEvent.Level)
            {
                case LogEventLevel.Debug:
                case LogEventLevel.Verbose: // Stackdriver doesn't have a Verbose level
                    output.Write("DEBUG");
                    break;
                case LogEventLevel.Warning:
                    output.Write("WARNING");
                    break;
                case LogEventLevel.Error:
                    output.Write("ERROR");
                    break;
                case LogEventLevel.Fatal:
                    output.Write("CRITICAL");
                    break;
                case LogEventLevel.Information:
                    output.Write("INFO");
                    break;
                default:
                    output.Write("DEFAULT");
                    break;
            }
            output.Write('\"');

            if (logEvent.Exception != null)
            {
                output.Write(",\"exception\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            foreach (var property in logEvent.Properties)
            {
                var name = property.Key;
                if (name.Length > 0 && name[0] == '@')
                {
                    // Escape first '@' by doubling
                    name = '@' + name;
                }

                output.Write(',');
                JsonValueFormatter.WriteQuotedJsonString(name, output);
                output.Write(':');
                valueFormatter.Format(property.Value, output);
            }

            output.Write('}');
        }
    }
}